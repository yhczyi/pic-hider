using System.Buffers.Binary;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace PicHider.Core;

public sealed record EncodeResult(
    Bitmap StegoImage,
    long CapacityBytes,
    long PayloadBytes,
    int HeaderBytes,
    int MaskBitCount,
    bool FooterWritten)
{
    public bool WasEncrypted { get; init; }
}

public static class LsbEncoder
{
    public static ReadOnlySpan<byte> MagicBytes => "PHPL"u8; // "PHPL" = PicHider PayLoad
    public const byte FormatVersion = 1;
    public const byte FlagEncrypted = 0x01;

    private const int PrefixBytes = 6; // 4B 魔数 + 1B 版本 + 1B 标志
    private const int ProfileLengthBytes = 4;

    public static EncodeResult Encode(string coverPath, string dataPath, Mask mask, string? password = null)
    {
        ValidateInputs(dataPath, mask);
        Bitmap cover = ImageUtil.Load32bppArgb(coverPath);
        try
        {
            return Encode(cover, dataPath, mask, password);
        }
        catch
        {
            cover.Dispose();
            throw;
        }
    }

    /// <summary>成功后 StegoImage 即 cover（所有权转移）；失败时不释放 cover。</summary>
    public static EncodeResult Encode(Bitmap cover, string dataPath, Mask mask, string? password = null)
    {
        ValidateInputs(dataPath, mask);
        if (cover.PixelFormat != PixelFormat.Format32bppArgb)
        {
            throw new StegoException(StegoError.ImageLoadFailed, "载体位图必须是 32bppArgb 格式。");
        }

        var fileInfo = new FileInfo(dataPath);
        long fileSize = fileInfo.Length;
        string fileName = Path.GetFileName(dataPath);
        byte[] profilePlain = ProfileCodec.Build(fileSize, fileName);
        bool encrypted = !PayloadCrypto.IsEmptyPassword(password);

        int profileFieldBytes = profilePlain.Length + (encrypted ? PayloadCrypto.TagSize : 0);
        int saltNonceBytes = encrypted
            ? PayloadCrypto.SaltSize + PayloadCrypto.NonceSize * 2
            : 0;

        long requiredBytes = ComputePayloadBytes(fileSize, fileName, encrypted);
        long requiredBits;
        try
        {
            requiredBits = checked(requiredBytes * 8);
        }
        catch (OverflowException)
        {
            throw new StegoException(StegoError.CapacityExceeded, "待隐藏文件过大。");
        }

        long pixelCount = (long)cover.Width * cover.Height;
        long popCount = mask.PopCount;
        long availableBits = pixelCount * popCount;
        long reservedBits = pixelCount > MaskFooter.FooterBits
            ? (pixelCount - MaskFooter.FooterBits) * popCount
            : 0;
        long availableBytes = availableBits / 8;

        if (requiredBits > availableBits || requiredBytes > Array.MaxLength)
        {
            throw new StegoException(
                StegoError.CapacityExceeded,
                $"容量不足：待隐藏文件需要 {ByteSize.Format(requiredBytes)}，当前图片在所选掩码下最多可隐藏 {ByteSize.Format(availableBytes)}。");
        }

        bool writeFooter = requiredBits <= reservedBits;

        byte[] payload = new byte[requiredBytes];
        MagicBytes.CopyTo(payload);
        payload[4] = FormatVersion;
        payload[5] = encrypted ? FlagEncrypted : (byte)0;

        int cursor = PrefixBytes;
        byte[]? key = null;
        ReadOnlySpan<byte> profileNonce = default;
        ReadOnlySpan<byte> contentNonce = default;
        if (encrypted)
        {
            RandomNumberGenerator.Fill(payload.AsSpan(cursor, PayloadCrypto.SaltSize));
            RandomNumberGenerator.Fill(payload.AsSpan(cursor + PayloadCrypto.SaltSize, PayloadCrypto.NonceSize));
            RandomNumberGenerator.Fill(payload.AsSpan(cursor + PayloadCrypto.SaltSize + PayloadCrypto.NonceSize, PayloadCrypto.NonceSize));
            key = PayloadCrypto.DeriveKey(password!, payload.AsSpan(cursor, PayloadCrypto.SaltSize));
            profileNonce = payload.AsSpan(cursor + PayloadCrypto.SaltSize, PayloadCrypto.NonceSize);
            contentNonce = payload.AsSpan(cursor + PayloadCrypto.SaltSize + PayloadCrypto.NonceSize, PayloadCrypto.NonceSize);
            cursor += saltNonceBytes;
        }

        BinaryPrimitives.WriteInt32BigEndian(payload.AsSpan(cursor, ProfileLengthBytes), profileFieldBytes);
        cursor += ProfileLengthBytes;
        ReadOnlySpan<byte> aad = payload.AsSpan(0, cursor);

        if (encrypted)
        {
            byte[] profileCipher = PayloadCrypto.Encrypt(key!, profileNonce, profilePlain, aad);
            profileCipher.CopyTo(payload.AsSpan(cursor));
        }
        else
        {
            profilePlain.CopyTo(payload.AsSpan(cursor));
        }

        int contentStart = cursor + profileFieldBytes;
        byte[] contentPlain = new byte[fileSize];
        using (FileStream input = File.OpenRead(dataPath))
        {
            input.ReadExactly(contentPlain);
        }
        byte[] contentField = encrypted
            ? PayloadCrypto.Encrypt(key!, contentNonce, contentPlain, aad)
            : contentPlain;
        contentField.CopyTo(payload.AsSpan(contentStart));

        EmbedPayload(cover, payload, mask);
        if (writeFooter)
        {
            MaskFooter.Write(cover, mask);
        }
        else
        {
            long firstPayloadPixel = (mask.Bits & 1u) != 0
                ? (requiredBits + popCount - 1) / popCount
                : 0;
            MaskFooter.InvalidateStale(cover, firstPayloadPixel);
        }

        long capacityBytes = (writeFooter ? reservedBits : availableBits) / 8;
        return new EncodeResult(cover, capacityBytes, fileSize, contentStart, mask.PopCount, writeFooter)
        {
            WasEncrypted = encrypted,
        };
    }

    public static long ComputePayloadBytes(long fileSize, string fileName, bool encrypted)
    {
        try
        {
            long profilePlainBytes = ProfileCodec.Build(fileSize, fileName).LongLength;
            long saltNonceBytes = encrypted ? PayloadCrypto.SaltSize + PayloadCrypto.NonceSize * 2 : 0;
            long profileFieldBytes = profilePlainBytes + (encrypted ? PayloadCrypto.TagSize : 0);
            long contentBytes = fileSize + (encrypted ? PayloadCrypto.TagSize : 0);
            return checked(PrefixBytes + saltNonceBytes + ProfileLengthBytes + profileFieldBytes + contentBytes);
        }
        catch (OverflowException ex)
        {
            throw new StegoException(StegoError.CapacityExceeded, "待隐藏文件过大。", ex);
        }
    }

    private static void ValidateInputs(string dataPath, Mask mask)
    {
        if (mask.IsEmpty)
        {
            throw new StegoException(StegoError.InvalidMask, "请至少勾选一个数据位。");
        }
        if (string.IsNullOrWhiteSpace(dataPath) || !File.Exists(dataPath))
        {
            throw new StegoException(StegoError.FileNotFound, $"待隐藏文件不存在：{dataPath}");
        }
    }

    private static unsafe void EmbedPayload(Bitmap image, byte[] data, Mask mask)
    {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        BitmapData bitmapData = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            int dataByte = 0;
            int dataBit = 0x80;
            bool exhausted = false;

            for (int y = 0; y < image.Height && !exhausted; y++)
            {
                int* row = (int*)((byte*)bitmapData.Scan0 + y * bitmapData.Stride);
                for (int x = 0; x < image.Width; x++)
                {
                    uint pixel = (uint)row[x];
                    uint maskBit = 0x8000_0000u;
                    for (int i = 0; i < Mask.BitWidth; i++)
                    {
                        if ((mask.Bits & maskBit) != 0)
                        {
                            if (dataByte >= data.Length)
                            {
                                exhausted = true;
                                break;
                            }

                            if ((data[dataByte] & dataBit) != 0)
                            {
                                pixel |= maskBit;
                            }
                            else
                            {
                                pixel &= ~maskBit;
                            }

                            dataBit >>= 1;
                            if (dataBit == 0)
                            {
                                dataBit = 0x80;
                                dataByte++;
                            }
                        }
                        maskBit >>= 1;
                    }

                    row[x] = (int)pixel;
                    if (exhausted)
                    {
                        break;
                    }
                }
            }
        }
        finally
        {
            image.UnlockBits(bitmapData);
        }
    }
}
