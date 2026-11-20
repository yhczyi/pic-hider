using PicHider.Core;
using System.Drawing.Imaging;

internal static class Program
{
    private static int _failures;
    private static string _tempDir = string.Empty;

    [STAThread]
    private static int Main()
    {
        try
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "PicHiderChecks_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);

            Paragraph("掩码");
            RunMaskChecks();

            Paragraph("描述信息与加密");
            RunProfileChecks();

            Paragraph("嵌入/解码往返");
            RunRoundTripChecks();

            Paragraph("生成载体");
            RunCoverGeneratorChecks();

            Paragraph("密码加密");
            RunPasswordChecks();

            Paragraph("内嵌掩码页脚");
            RunFooterChecks();

            Paragraph("异常路径");
            RunErrorChecks();

            Paragraph("结果");
            Console.WriteLine(_failures == 0 ? "全部通过。" : $"失败 {_failures} 项。");
            return _failures == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 2;
        }
        finally
        {
            if (Directory.Exists(_tempDir))
            {
                try { Directory.Delete(_tempDir, true); } catch { }
            }
        }
    }

    private static void RunMaskChecks()
    {
        Check(Mask.Default.ToBinaryString() == "00001111000011110000111100001111", "默认掩码格式");
        Check(Mask.Default.PopCount == 16, "默认掩码位数=16");
        Check(Mask.FromBits(0xFFFF_FFFF).PopCount == 32, "全 1 掩码位数=32");

        const string custom = "10101010110011001111000010010001";
        Check(Mask.Parse(custom).ToBinaryString() == custom, "掩码解析/格式化往返");
        Check(Mask.Parse("10000000000000000000000000000000").Bits == 0x8000_0000, "首位对应 bit31");

        CheckThrows<StegoException>(() => Mask.Parse("0101"), "长度非法掩码被拒绝");
        CheckThrows<StegoException>(() => Mask.Parse(new string('1', 32).Remove(3, 1) + "2"), "非法字符掩码被拒绝");
    }

    private static void RunProfileChecks()
    {
        byte[] profile = ProfileCodec.Build(12345, "测试文件.txt");
        Check(System.Text.Encoding.UTF8.GetString(profile) == "12345,测试文件.txt", "中文文件名 profile 往返（UTF-8 明文）");

        var (size, name) = ProfileCodec.Parse("12345,a,b.txt");
        Check(size == 12345 && name == "a,b.txt", "文件名含逗号按第一个逗号拆分");

        CheckThrows<StegoException>(() => ProfileCodec.Parse("not-a-size,x.txt"), "非法描述信息被拒绝");
        CheckThrows<StegoException>(() => ProfileCodec.Parse("-1,x.txt"), "负文件大小被拒绝");

        byte[] salt = new byte[PayloadCrypto.SaltSize];
        salt[0] = 7;
        byte[] key = PayloadCrypto.DeriveKey("口令123", salt);
        Check(key.Length == PayloadCrypto.KeySize && key.SequenceEqual(PayloadCrypto.DeriveKey("口令123", salt)),
            "PBKDF2 同盐同密码派生一致");

        byte[] nonce = new byte[PayloadCrypto.NonceSize];
        nonce[0] = 9;
        byte[] plain = { 1, 2, 3, 4, 5 };
        byte[] aad = { 0x50, 0x48, 0x50, 0x4C };
        byte[] sealedBytes = PayloadCrypto.Encrypt(key, nonce, plain, aad);
        Check(sealedBytes.Length == plain.Length + PayloadCrypto.TagSize, "GCM 密文长度 = 明文 + tag");
        Check(PayloadCrypto.Decrypt(key, nonce, sealedBytes, aad).SequenceEqual(plain), "GCM 加解密往返");

        CheckThrows<StegoException>(
            () => PayloadCrypto.Decrypt(PayloadCrypto.DeriveKey("另一个", salt), nonce, sealedBytes, aad),
            "GCM 错误密钥解密失败");
        sealedBytes[0] ^= 1;
        CheckThrows<StegoException>(() => PayloadCrypto.Decrypt(key, nonce, sealedBytes, aad), "GCM 篡改后解密失败");
    }

    private static void RunRoundTripChecks()
    {
        RoundTrip(64, 64, Mask.Default, MakePayload(500), "hello.txt", "默认掩码");
        RoundTrip(37, 29, Mask.Parse("10101010101010101010101010101010"), MakePayload(300), "odd.bin", "自定义掩码+奇数宽度");
        RoundTrip(64, 64, Mask.FromBits(0xFFFF_FFFF), MakePayload(1024), "all.bin", "32 位全掩码");
        RoundTrip(64, 64, Mask.Parse("00000000000000001000000000000000"), MakePayload(32), "single.bin", "单 bit 掩码");
        RoundTrip(64, 64, Mask.Default, MakePayload(600), "数据 文件.bin", "中文文件名");
        RoundTrip(64, 64, Mask.Default, MakePayload(500), "hello.txt", "密码加密", "P@ssw0rd");
        RoundTrip(64, 64, Mask.Parse("10101010110011001111000010010001"), MakePayload(300), "加密 文件.bin",
            "中文/含空格密码+自定义掩码", "密码 123");

        CheckCapacityMath();
        CheckJpegCoverRoundTrip();
        CheckCoverNotModified();
        CheckDeterministicPixelBits();
    }

    private static void RunCoverGeneratorChecks()
    {
        long smallPayload = LsbEncoder.ComputePayloadBytes(500, "gen.bin", false);
        (int smallW, int smallH) = CoverGenerator.ComputeSize(16, 9, smallPayload, 16);
        Check(smallW == 1920 && smallH == 1080, "16:9 载体默认长边 1920");

        long largePayload = LsbEncoder.ComputePayloadBytes(6_000_000, "big.bin", false);
        (int largeW, int largeH) = CoverGenerator.ComputeSize(16, 9, largePayload, 16);
        long capacity = ((long)largeW * largeH - MaskFooter.FooterBits) * 16 / 8;
        Check(largeW > 1920 && (long)largeW * 9 == (long)largeH * 16, "大文件按比例放大且保持 16:9");
        Check(capacity >= largePayload, "自动尺寸满足容量并预留页脚");

        (int encW, int encH) = CoverGenerator.ComputeSize(
            4, 3, LsbEncoder.ComputePayloadBytes(10, "e.bin", true), 16);
        Check(encW == 1920 && encH == 1440, "4:3 载体默认尺寸");

        using (Bitmap bitmap = CoverGenerator.Create(160, 90))
        {
            Check(bitmap.Width == 160 && bitmap.Height == 90 && bitmap.PixelFormat == PixelFormat.Format32bppArgb,
                "生成位图尺寸/格式正确");
            Check(bitmap.GetPixel(0, 0).ToArgb() != bitmap.GetPixel(159, 89).ToArgb(),
                "生成图为非纯色渐变");
        }

        byte[] payload = MakePayload(1000);
        string dataPath = WriteTemp("generated_secret.bin", payload);
        long required = LsbEncoder.ComputePayloadBytes(
            payload.Length, Path.GetFileName(dataPath), encrypted: false);
        (int width, int height) = CoverGenerator.ComputeSize(3, 2, required, Mask.Default.PopCount);
        Bitmap cover = CoverGenerator.Create(width, height);
        EncodeResult result;
        try
        {
            result = LsbEncoder.Encode(cover, dataPath, Mask.Default);
        }
        catch
        {
            cover.Dispose();
            throw;
        }

        string stegoPath = Path.Combine(_tempDir, "generated_stego.png");
        using (result.StegoImage)
        {
            Check(result.FooterWritten, "生成载体嵌入后写入页脚");
            result.StegoImage.Save(stegoPath, ImageFormat.Png);
        }

        DecodedPayload decoded = LsbDecoder.ExtractAuto(
            stegoPath, Mask.FromBits(0xFFFF_FFFF), out Mask used, out bool embedded);
        Check(embedded && used.Bits == Mask.Default.Bits && decoded.Data.SequenceEqual(payload),
            "生成载体 → 嵌入 → 内嵌掩码自动解码往返");
    }

    private static void CheckCapacityMath()
    {
        string cover = CreateCover(100, 100, "cap", ImageFormat.Png);
        string data = WriteTemp("cap.bin", MakePayload(10));
        int profileLength = ProfileCodec.Build(10, "cap.bin").Length;
        var result = LsbEncoder.Encode(cover, data, Mask.Default);
        using (result.StegoImage)
        {
            Check(result.CapacityBytes == (100L * 100 - MaskFooter.FooterBits) * 16 / 8,
                "容量计算 = (像素数 - 页脚像素) * 位数 / 8");
            Check(result.FooterWritten, "容量充足时写入内嵌掩码页脚");
            Check(!result.WasEncrypted, "未填密码时 WasEncrypted=false");
            Check(result.HeaderBytes == 6 + 4 + profileLength, "明文头部长度 = 6 + 4 + profile");
        }

        var encrypted = LsbEncoder.Encode(cover, WriteTemp("cap.bin", MakePayload(10)), Mask.Default, "pw");
        using (encrypted.StegoImage)
        {
            int expected = 6 + PayloadCrypto.SaltSize + PayloadCrypto.NonceSize * 2 + 4 + profileLength + PayloadCrypto.TagSize;
            Check(encrypted.WasEncrypted, "填写密码时 WasEncrypted=true");
            Check(encrypted.HeaderBytes == expected, "加密头部长度 = 前缀 + 盐/nonce + 长度 + 密文 profile");
            Check(encrypted.FooterWritten, "加密后仍写入页脚");
        }
    }

    private static void CheckJpegCoverRoundTrip()
    {
        string cover = CreateCover(80, 60, "coverjpg", ImageFormat.Jpeg);
        byte[] payload = MakePayload(200);
        string data = WriteTemp("jpegdata.bin", payload);
        string stego = Path.Combine(_tempDir, "stego_jpeg.png");

        var result = LsbEncoder.Encode(cover, data, Mask.Default);
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }

        DecodedPayload decoded = LsbDecoder.Extract(stego, Mask.Default);
        Check(decoded.Data.SequenceEqual(payload), "JPEG 载体转换为 32bpp 后往返");
    }

    private static void CheckCoverNotModified()
    {
        string cover = CreateCover(64, 64, "unmodified", ImageFormat.Png);
        string data = WriteTemp("m.bin", MakePayload(50));
        string before = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(cover)));

        var result = LsbEncoder.Encode(cover, data, Mask.Default);
        using (result.StegoImage)
        {
            result.StegoImage.Save(Path.Combine(_tempDir, "unmodified_stego.png"), ImageFormat.Png);
        }

        string after = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(cover)));
        Check(before == after, "嵌入过程不修改载体原文件");
    }

    private static void CheckDeterministicPixelBits()
    {
        // 单 bit 掩码：bit15（Green 最高位）。使用已知数据检查像素位映射。
        var mask = Mask.Parse("00000000000000001000000000000000");
        byte[] payload = { 0b10110010, 0b01000000, 0b11111111 };
        string cover = CreateCover(64, 64, "bits", ImageFormat.Png);
        string data = WriteTemp("bits.bin", payload);
        string stegoPath = Path.Combine(_tempDir, "bits_stego.png");

        var result = LsbEncoder.Encode(cover, data, mask);
        using (result.StegoImage)
        {
            result.StegoImage.Save(stegoPath, ImageFormat.Png);
        }

        using var check = ImageUtil.Load32bppArgb(stegoPath);
        // 头部：4B 魔数 PHPL + 1B 版本 + 1B 标志 + 4B profile 长度 + profile 明文，随后是文件内容。
        byte[] profile = ProfileCodec.Build(payload.Length, "bits.bin");
        byte[] embedded = new byte[6 + 4 + profile.Length + payload.Length];
        LsbEncoder.MagicBytes.CopyTo(embedded);
        embedded[4] = LsbEncoder.FormatVersion;
        embedded[5] = 0;
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(embedded.AsSpan(6, 4), profile.Length);
        profile.CopyTo(embedded, 10);
        payload.CopyTo(embedded, 10 + profile.Length);

        // 取前 8 个字节 = 64 位，检查前 64 个像素依次承载这些位的 bit7..bit0。
        byte[] expectedBits = embedded.Take(8)
            .SelectMany(b => Enumerable.Range(0, 8).Select(i => (byte)((b >> (7 - i)) & 1)))
            .ToArray();
        bool ok = true;
        for (int i = 0; i < expectedBits.Length; i++)
        {
            Color pixel = check.GetPixel(i % check.Width, i / check.Width);
            int actual = (pixel.G >> 7) & 1;
            if (actual != expectedBits[i])
            {
                ok = false;
                break;
            }
        }
        Check(ok, "像素位映射与扫描顺序");
    }

    private static void RunPasswordChecks()
    {
        string cover = CreateCover(128, 128, "pw", ImageFormat.Png);
        byte[] payload = MakePayload(800);
        string data = WriteTemp("secret.bin", payload);
        string stego = Path.Combine(_tempDir, $"pw_stego_{Guid.NewGuid():N}.png");

        var result = LsbEncoder.Encode(cover, data, Mask.Default, "P@ss 密码");
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }

        byte[] head = ReadEmbeddedBytes(stego, Mask.Default, 6);
        Check(head.AsSpan(0, 4).SequenceEqual(LsbEncoder.MagicBytes)
            && head[4] == LsbEncoder.FormatVersion
            && head[5] == LsbEncoder.FlagEncrypted, "加密图片头部：PHPL/版本1/加密标志");

        DecodedPayload decoded = LsbDecoder.Extract(stego, Mask.Default, "P@ss 密码");
        Check(decoded.Data.SequenceEqual(payload) && decoded.WasEncrypted, "密码正确时解码成功");

        try
        {
            LsbDecoder.Extract(stego, Mask.Default);
            Check(false, "加密图片未提供密码应报错");
        }
        catch (StegoException ex)
        {
            Check(ex.Error == StegoError.WrongPassword, "加密图片未提供密码提示输入密码");
        }

        try
        {
            LsbDecoder.Extract(stego, Mask.Default, "wrong-password");
            Check(false, "错误密码应报错");
        }
        catch (StegoException ex)
        {
            Check(ex.Error == StegoError.WrongPassword, "错误密码提示密码错误");
        }

        // 翻转内容区一位（第 128 个像素的 Green bit0，默认掩码覆盖该位）后认证失败
        string tampered = Path.Combine(_tempDir, $"pw_tampered_{Guid.NewGuid():N}.png");
        using (Bitmap image = ImageUtil.Load32bppArgb(stego))
        {
            Color c = image.GetPixel(0, 1);
            image.SetPixel(0, 1, Color.FromArgb(c.A, c.R, c.G ^ 0x01, c.B));
            image.Save(tampered, ImageFormat.Png);
        }
        try
        {
            LsbDecoder.Extract(tampered, Mask.Default, "P@ss 密码");
            Check(false, "篡改数据应报错");
        }
        catch (StegoException ex)
        {
            Check(ex.Error == StegoError.WrongPassword, "篡改数据后认证失败");
        }

        CheckPlaintextMode(cover, data, payload);
        CheckEmptyFileEncrypted(cover);
        CheckEncryptedCrossesFooterBoundary();
    }

    private static void CheckPlaintextMode(string cover, string data, byte[] payload)
    {
        foreach (string? password in new[] { null, "" })
        {
            string stego = Path.Combine(_tempDir, $"pw_plain_{Guid.NewGuid():N}.png");
            var result = LsbEncoder.Encode(cover, data, Mask.Default, password);
            using (result.StegoImage)
            {
                result.StegoImage.Save(stego, ImageFormat.Png);
            }

            byte[] head = ReadEmbeddedBytes(stego, Mask.Default, 6);
            Check(!result.WasEncrypted && head[5] == 0,
                $"空密码（{(password is null ? "null" : "空串")}）不加密");
            DecodedPayload decoded = LsbDecoder.Extract(stego, Mask.Default);
            Check(decoded.Data.SequenceEqual(payload) && !decoded.WasEncrypted, "明文图片无需密码解码");
        }
    }

    private static void CheckEmptyFileEncrypted(string cover)
    {
        string data = WriteTemp("empty.bin", Array.Empty<byte>());
        string stego = Path.Combine(_tempDir, $"pw_empty_{Guid.NewGuid():N}.png");
        var result = LsbEncoder.Encode(cover, data, Mask.Default, "pw");
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }

        DecodedPayload decoded = LsbDecoder.Extract(stego, Mask.Default, "pw");
        Check(decoded.Data.Length == 0 && decoded.FileName == "empty.bin" && decoded.WasEncrypted,
            "加密零字节文件往返");
    }

    private static void CheckEncryptedCrossesFooterBoundary()
    {
        string cover = CreateCover(64, 64, "boundary", ImageFormat.Png);
        byte[] payload = MakePayload(7970);
        string data = WriteTemp("boundary.bin", payload);

        var plain = LsbEncoder.Encode(cover, data, Mask.Default);
        using (plain.StegoImage) { }
        Check(plain.FooterWritten, "临界大小：明文可写页脚");

        string stego = Path.Combine(_tempDir, $"pw_boundary_{Guid.NewGuid():N}.png");
        var encrypted = LsbEncoder.Encode(cover, data, Mask.Default, "pw");
        using (encrypted.StegoImage)
        {
            encrypted.StegoImage.Save(stego, ImageFormat.Png);
        }

        Check(!encrypted.FooterWritten, "临界大小：加密后放不下页脚");
        DecodedPayload decoded = LsbDecoder.Extract(stego, Mask.Default, "pw");
        Check(decoded.Data.SequenceEqual(payload), "跳过页脚的加密图片仍可解码");
    }

    private static byte[] ReadEmbeddedBytes(string stegoPath, Mask mask, int count)
    {
        using Bitmap image = ImageUtil.Load32bppArgb(stegoPath);
        var bytes = new byte[count];
        int bitIndex = 0;
        int limit = count * 8;
        for (int y = 0; y < image.Height && bitIndex < limit; y++)
        {
            for (int x = 0; x < image.Width && bitIndex < limit; x++)
            {
                uint pixel = (uint)image.GetPixel(x, y).ToArgb();
                uint maskBit = 0x8000_0000u;
                for (int i = 0; i < Mask.BitWidth && bitIndex < limit; i++)
                {
                    if ((mask.Bits & maskBit) != 0)
                    {
                        if ((pixel & maskBit) != 0)
                        {
                            bytes[bitIndex >> 3] |= (byte)(0x80 >> (bitIndex & 7));
                        }
                        bitIndex++;
                    }
                    maskBit >>= 1;
                }
            }
        }
        return bytes;
    }

    private static void RunFooterChecks()
    {
        CheckFooterWriteRead();
        CheckFooterAutoDecode();
        CheckFooterCorruptedFallback();
        CheckTinyImageNoFooter();
        CheckNoRoomFooterSkip();
        CheckStaleFooterInvalidated();
        CheckNoRoomFooterSkipWithoutBlueLsb();
    }

    private static void CheckFooterWriteRead()
    {
        string cover = CreateCover(64, 64, "footer", ImageFormat.Png);
        Mask mask = Mask.Parse("10101010110011001111000010010001");
        using var original = ImageUtil.Load32bppArgb(cover);
        using var image = (Bitmap)original.Clone();
        MaskFooter.Write(image, mask);

        Check(MaskFooter.TryRead(image, out Mask read, out byte version)
            && read.Bits == mask.Bits
            && version == MaskFooter.CurrentVersion, "页脚写/读往返");

        long pixelCount = (long)image.Width * image.Height;
        byte[] footer = new byte[MaskFooter.FooterBytes];
        for (int k = 0; k < MaskFooter.FooterBits; k++)
        {
            long p = pixelCount - MaskFooter.FooterBits + k;
            Color c = image.GetPixel((int)(p % image.Width), (int)(p / image.Width));
            if ((c.B & 1) != 0)
            {
                footer[k >> 3] |= (byte)(0x80 >> (k & 7));
            }
        }

        byte checksum = 0;
        for (int i = 0; i < MaskFooter.FooterBytes - 1; i++)
        {
            checksum ^= footer[i];
        }
        bool layoutOk = footer[0] == (byte)'P' && footer[1] == (byte)'H'
            && footer[2] == (byte)'M' && footer[3] == (byte)'F'
            && footer[4] == MaskFooter.CurrentVersion
            && System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(footer.AsSpan(5, 4)) == mask.Bits
            && footer[9] == checksum;
        Check(layoutOk, "页脚字节布局（魔数/版本/大端掩码/XOR 校验）");

        bool outsideOk = true;
        bool onlyBlueOk = true;
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                long p = (long)y * image.Width + x;
                Color a = original.GetPixel(x, y);
                Color b = image.GetPixel(x, y);
                if (p < pixelCount - MaskFooter.FooterBits)
                {
                    if (a.ToArgb() != b.ToArgb())
                    {
                        outsideOk = false;
                    }
                }
                else if (a.A != b.A || a.R != b.R || a.G != b.G || (a.B & 0xFE) != (b.B & 0xFE))
                {
                    onlyBlueOk = false;
                }
            }
        }
        Check(outsideOk, "页脚不修改最后 80 像素之外的像素");
        Check(onlyBlueOk, "页脚只修改 Blue 通道 bit0");
    }

    private static void CheckFooterAutoDecode()
    {
        string cover = CreateCover(128, 128, "auto", ImageFormat.Png);
        byte[] payload = MakePayload(700);
        string data = WriteTemp("auto-secret.bin", payload);
        Mask custom = Mask.Parse("10101010110011001111000010010001");
        string stego = Path.Combine(_tempDir, $"footer_auto_{Guid.NewGuid():N}.png");

        var result = LsbEncoder.Encode(cover, data, custom);
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }

        Check(result.FooterWritten, "自动掩码：页脚已写入");
        Check(result.CapacityBytes == ((long)128 * 128 - MaskFooter.FooterBits) * custom.PopCount / 8,
            "自动掩码：容量扣除页脚像素");

        DecodedPayload decoded = LsbDecoder.ExtractAuto(stego, Mask.Default, out Mask used, out bool embedded);
        Check(decoded.Data.SequenceEqual(payload) && decoded.FileName == "auto-secret.bin", "自动掩码：数据往返");
        Check(embedded && used.Bits == custom.Bits, "自动掩码：优先使用内嵌掩码");

        DecodedPayload explicitDecode = LsbDecoder.Extract(stego, custom);
        Check(explicitDecode.Data.SequenceEqual(payload), "显式掩码解码忽略页脚");
    }

    private static void CheckFooterCorruptedFallback()
    {
        string cover = CreateCover(64, 64, "corrupt", ImageFormat.Png);
        byte[] payload = MakePayload(64);
        string data = WriteTemp("corrupt.bin", payload);
        Mask custom = Mask.Parse("10101010110011001111000010010001");
        string stego = Path.Combine(_tempDir, $"footer_corrupt_{Guid.NewGuid():N}.png");
        var result = LsbEncoder.Encode(cover, data, custom);
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }

        string corrupted = Path.Combine(_tempDir, $"footer_corrupted_{Guid.NewGuid():N}.png");
        using (Bitmap image = ImageUtil.Load32bppArgb(stego))
        {
            int x = image.Width - 1;
            int y = image.Height - 1;
            Color c = image.GetPixel(x, y);
            image.SetPixel(x, y, Color.FromArgb(c.A, c.R, c.G, c.B ^ 1));
            image.Save(corrupted, ImageFormat.Png);
        }

        DecodedPayload decoded = LsbDecoder.ExtractAuto(corrupted, custom, out _, out bool embedded);
        Check(decoded.Data.SequenceEqual(payload) && !embedded, "页脚损坏时回退到界面掩码");
    }

    private static void CheckTinyImageNoFooter()
    {
        foreach ((int width, int height) in new[] { (8, 8), (80, 1) })
        {
            string cover = CreateCover(width, height, "tiny", ImageFormat.Png);
            string data = WriteTemp("tiny.bin", MakePayload(20));
            var result = LsbEncoder.Encode(cover, data, Mask.Default);
            string stego = Path.Combine(_tempDir, $"tiny_{Guid.NewGuid():N}.png");
            using (result.StegoImage)
            {
                result.StegoImage.Save(stego, ImageFormat.Png);
            }

            Check(!result.FooterWritten, $"{width}x{height} 小图不写页脚");
            Check(!MaskFooter.TryReadFile(stego, out _, out _), $"{width}x{height} 小图无页脚");
            DecodedPayload decoded = LsbDecoder.ExtractAuto(stego, Mask.Default, out _, out bool embedded);
            Check(!embedded && decoded.Data.SequenceEqual(MakePayload(20)), $"{width}x{height} 小图回退界面掩码仍可解码");
        }
    }

    private static void CheckNoRoomFooterSkip()
    {
        string cover = CreateCover(64, 64, "noroom", ImageFormat.Png);
        byte[] payload = MakePayload(8050);
        string data = WriteTemp("big.bin", payload);
        var result = LsbEncoder.Encode(cover, data, Mask.Default);
        string stego = Path.Combine(_tempDir, $"noroom_{Guid.NewGuid():N}.png");
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }

        Check(!result.FooterWritten, "payload 进入预留区时不写页脚");
        Check(!MaskFooter.TryReadFile(stego, out _, out _), "跳过页脚后图片无有效页脚");
        DecodedPayload decoded = LsbDecoder.ExtractAuto(stego, Mask.Default, out _, out bool embedded);
        Check(!embedded && decoded.Data.SequenceEqual(payload), "跳过页脚时仍按界面掩码解码");
    }

    private static void CheckStaleFooterInvalidated()
    {
        Mask staleMask = Mask.Parse("10101010110011001111000010010001");
        string firstStego = Path.Combine(_tempDir, $"stale_first_{Guid.NewGuid():N}.png");
        var first = LsbEncoder.Encode(
            CreateCover(64, 64, "stale", ImageFormat.Png),
            WriteTemp("stale.bin", MakePayload(64)),
            staleMask);
        using (first.StegoImage)
        {
            first.StegoImage.Save(firstStego, ImageFormat.Png);
        }
        Check(first.FooterWritten, "旧页脚准备：已写入");

        byte[] payload = MakePayload(8050);
        string data = WriteTemp("big2.bin", payload);
        string secondStego = Path.Combine(_tempDir, $"stale_second_{Guid.NewGuid():N}.png");
        var second = LsbEncoder.Encode(firstStego, data, Mask.Default);
        using (second.StegoImage)
        {
            second.StegoImage.Save(secondStego, ImageFormat.Png);
        }

        Check(!second.FooterWritten, "以隐写图为载体时同样跳过页脚");
        Check(!MaskFooter.TryReadFile(secondStego, out _, out _), "旧页脚被清除/失效");
        DecodedPayload decoded = LsbDecoder.ExtractAuto(secondStego, Mask.Default, out _, out bool embedded);
        Check(!embedded && decoded.Data.SequenceEqual(payload), "旧页脚失效后按界面掩码正确解码");
    }

    private static void CheckNoRoomFooterSkipWithoutBlueLsb()
    {
        Mask mask = Mask.Parse("11111111111111111111111111111110");
        string cover = CreateCover(64, 64, "noblue", ImageFormat.Png);
        byte[] payload = MakePayload(15600);
        string data = WriteTemp("huge.bin", payload);
        var result = LsbEncoder.Encode(cover, data, mask);
        string stego = Path.Combine(_tempDir, $"noblue_{Guid.NewGuid():N}.png");
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }

        Check(!result.FooterWritten, "掩码不含 Blue bit0 且放不下预留区时跳过页脚");
        Check(!MaskFooter.TryReadFile(stego, out _, out _), "无 Blue bit0 掩码下无有效页脚");
        DecodedPayload decoded = LsbDecoder.ExtractAuto(stego, mask, out _, out bool embedded);
        Check(!embedded && decoded.Data.SequenceEqual(payload), "无 Blue bit0 掩码下显式解码正确");
    }

    private static void RunErrorChecks()
    {
        CheckThrows<StegoException>(() => Mask.Parse("0000000000000000000000000000000"),
            "31 位掩码被拒绝（合法性兜底）");

        string cover = CreateCover(16, 16, "err", ImageFormat.Png);
        string data = WriteTemp("foo.bin", MakePayload(500));

        CheckThrows<StegoException>(() =>
        {
            var r = LsbEncoder.Encode(cover, data, Mask.Default);
            r.StegoImage.Dispose();
        }, "容量不足时抛出异常");
        CheckThrows<StegoException>(() =>
        {
            var r = LsbEncoder.Encode(cover, data, Mask.FromBits(0));
            r.StegoImage.Dispose();
        }, "全零掩码时抛出异常");
        CheckThrows<StegoException>(() => LsbEncoder.Encode(cover, Path.Combine(_tempDir, "missing.bin"), Mask.Default),
            "数据文件不存在时抛出异常");
        CheckThrows<StegoException>(() => ImageUtil.Load32bppArgb(Path.Combine(_tempDir, "missing.png")),
            "图片不存在时抛出异常");

        string plainCover = CreateCover(64, 64, "plain", ImageFormat.Png);
        CheckThrows<StegoException>(() => LsbDecoder.Extract(plainCover, Mask.Default),
            "未嵌入数据的图片解码失败");

        // 用错误掩码解码已嵌入图片
        string stego = Path.Combine(_tempDir, "wrongmask_stego.png");
        string small = WriteTemp("small.bin", MakePayload(64));
        var result = LsbEncoder.Encode(CreateCover(64, 64, "wm", ImageFormat.Png), small, Mask.Default);
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }
        CheckThrows<StegoException>(() => LsbDecoder.Extract(stego, Mask.Parse("11111111000000001111111100000000")),
            "错误掩码解码失败");
    }

    private static void RoundTrip(int width, int height, Mask mask, byte[] payload, string fileName, string label, string? password = null)
    {
        string cover = CreateCover(width, height, "rt", ImageFormat.Png);
        string dataPath = WriteTemp(fileName, payload);
        string stego = Path.Combine(_tempDir, $"stego_{Guid.NewGuid():N}.png");

        var result = LsbEncoder.Encode(cover, dataPath, mask, password);
        using (result.StegoImage)
        {
            result.StegoImage.Save(stego, ImageFormat.Png);
        }

        Check(result.WasEncrypted == !string.IsNullOrEmpty(password), $"加密标志：{label}");
        DecodedPayload decoded = LsbDecoder.Extract(stego, mask, password);
        bool dataOk = decoded.Data.SequenceEqual(payload);
        if (!dataOk || decoded.FileName != fileName)
        {
            int diff = -1;
            int min = Math.Min(decoded.Data.Length, payload.Length);
            for (int i = 0; i < min; i++)
            {
                if (decoded.Data[i] != payload[i]) { diff = i; break; }
            }
            Console.WriteLine($"  详情: 期望长度={payload.Length}, 实际长度={decoded.Data.Length}, 期望文件名={fileName}, 实际文件名={decoded.FileName}, 首个差异={diff}");
        }
        Check(dataOk && decoded.FileName == fileName, $"往返：{label}");
    }

    private static string CreateCover(int width, int height, string name, ImageFormat format)
    {
        string ext = format.Guid == ImageFormat.Jpeg.Guid ? ".jpg" : ".png";
        string path = Path.Combine(_tempDir, $"{name}_{Guid.NewGuid():N}{ext}");
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.FromArgb(200, 12, 34, 56));
        }
        bitmap.Save(path, format);
        return path;
    }

    private static string WriteTemp(string fileName, byte[] data)
    {
        string directory = Path.Combine(_tempDir, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, fileName);
        File.WriteAllBytes(path, data);
        return path;
    }

    private static byte[] MakePayload(int length)
    {
        var random = new Random(2024 + length);
        byte[] payload = new byte[length];
        random.NextBytes(payload);
        return payload;
    }

    private static void Check(bool condition, string name)
    {
        if (condition)
        {
            Console.WriteLine($"[PASS] {name}");
        }
        else
        {
            _failures++;
            Console.WriteLine($"[FAIL] {name}");
        }
    }

    private static void CheckThrows<T>(Action action, string name) where T : Exception
    {
        try
        {
            action();
            _failures++;
            Console.WriteLine($"[FAIL] {name}（未抛出异常）");
        }
        catch (T)
        {
            Console.WriteLine($"[PASS] {name}");
        }
        catch (Exception ex)
        {
            _failures++;
            Console.WriteLine($"[FAIL] {name}（抛出 {ex.GetType().Name}: {ex.Message}）");
        }
    }

    private static void Paragraph(string title) => Console.WriteLine($"\n== {title} ==");
}
