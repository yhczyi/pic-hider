using System.Buffers.Binary;
using System.Drawing.Imaging;
using System.Text;

namespace PicHider.Core;

public sealed record DecodedPayload(string FileName, byte[] Data)
{
    public bool WasEncrypted { get; init; }
}

public sealed record DecodeResult(string OutputPath, string FileName, long SizeBytes, bool OverwroteExisting)
{
    public Mask? UsedMask { get; init; }
    public bool UsedEmbeddedMask { get; init; }
    public bool WasEncrypted { get; init; }
}

public static class LsbDecoder
{
    private const int PrefixBytes = 6; // 4B 魔数 + 1B 版本 + 1B 标志
    private const int ProfileLengthBytes = 4;
    private const int MaxProfileBytes = 128 * 1024;

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    /// <summary>解码并写入同目录的 output_原文件名。</summary>
    public static DecodeResult DecodeToFile(string stegoPath, Mask mask, string? password = null)
    {
        DecodedPayload payload = Extract(stegoPath, mask, password);
        return WritePayload(stegoPath, payload, mask, usedEmbeddedMask: false);
    }

    /// <summary>优先使用内嵌掩码；没有有效页脚时用 fallbackMask。</summary>
    public static DecodeResult DecodeToFileAuto(string stegoPath, Mask fallbackMask, string? password = null)
    {
        using Bitmap image = ImageUtil.Load32bppArgb(stegoPath);
        DecodedPayload payload = ExtractAuto(image, fallbackMask, out Mask usedMask, out bool usedEmbeddedMask, password);
        return WritePayload(stegoPath, payload, usedMask, usedEmbeddedMask);
    }

    private static DecodeResult WritePayload(string stegoPath, DecodedPayload payload, Mask usedMask, bool usedEmbeddedMask)
    {
        string fullPath = Path.GetFullPath(stegoPath);
        string directory = Path.GetDirectoryName(fullPath) ?? Environment.CurrentDirectory;
        string outputPath = Path.Combine(directory, "output_" + payload.FileName);
        bool existed = File.Exists(outputPath);

        try
        {
            File.WriteAllBytes(outputPath, payload.Data);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new StegoException(StegoError.OutputFailed, $"写入解码文件失败：{outputPath}", ex);
        }

        return new DecodeResult(outputPath, payload.FileName, payload.Data.LongLength, existed)
        {
            UsedMask = usedMask,
            UsedEmbeddedMask = usedEmbeddedMask,
            WasEncrypted = payload.WasEncrypted,
        };
    }

    public static DecodedPayload ExtractAuto(string stegoPath, Mask fallbackMask, out Mask usedMask, out bool usedEmbeddedMask, string? password = null)
    {
        using Bitmap image = ImageUtil.Load32bppArgb(stegoPath);
        return ExtractAuto(image, fallbackMask, out usedMask, out usedEmbeddedMask, password);
    }

    public static DecodedPayload ExtractAuto(Bitmap image, Mask fallbackMask, out Mask usedMask, out bool usedEmbeddedMask, string? password = null)
    {
        if (MaskFooter.TryRead(image, out Mask embeddedMask, out _) && !embeddedMask.IsEmpty)
        {
            try
            {
                DecodedPayload payload = Extract(image, embeddedMask, password);
                usedMask = embeddedMask;
                usedEmbeddedMask = true;
                return payload;
            }
            catch (StegoException ex) when (ex.Error == StegoError.NoHiddenData
                && !fallbackMask.IsEmpty
                && fallbackMask.Bits != embeddedMask.Bits)
            {
                // 页脚误判时回退到界面掩码；密码错误不回退
            }
        }

        usedMask = fallbackMask;
        usedEmbeddedMask = false;
        return Extract(image, fallbackMask, password);
    }

    public static DecodedPayload Extract(string stegoPath, Mask mask, string? password = null)
    {
        using Bitmap image = ImageUtil.Load32bppArgb(stegoPath);
        return Extract(image, mask, password);
    }

    /// <summary>调用方保留 image 所有权。</summary>
    public static unsafe DecodedPayload Extract(Bitmap image, Mask mask, string? password = null)
    {
        if (mask.IsEmpty)
        {
            throw new StegoException(StegoError.InvalidMask, "请至少勾选一个数据位。");
        }

        long availableBits = (long)image.Width * image.Height * mask.PopCount;
        if (availableBits < 8L * PrefixBytes)
        {
            throw NoHiddenData();
        }

        byte[] data = new byte[PrefixBytes];
        int dataLength = PrefixBytes;
        int dataByte = 0;
        int dataBit = 0x80;
        bool prefixParsed = false;
        bool headerParsed = false;
        bool profileParsed = false;
        bool complete = false;

        bool encrypted = false;
        int headerBytes = 0;
        int profileStart = 0;
        byte[]? key = null;
        string? fileName = null;
        int dataStart = -1;

        var rect = new Rectangle(0, 0, image.Width, image.Height);
        BitmapData bitmapData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int y = 0; y < image.Height && !complete; y++)
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
                            if (dataByte >= dataLength)
                            {
                                complete = true;
                                break;
                            }

                            if ((pixel & maskBit) != 0)
                            {
                                data[dataByte] |= (byte)dataBit;
                            }

                            dataBit >>= 1;
                            if (dataBit == 0)
                            {
                                dataBit = 0x80;
                                dataByte++;

                                if (!prefixParsed && dataByte == PrefixBytes)
                                {
                                    prefixParsed = true;
                                    encrypted = ParsePrefix(data, password);
                                    headerBytes = encrypted
                                        ? PrefixBytes + PayloadCrypto.SaltSize + PayloadCrypto.NonceSize * 2 + ProfileLengthBytes
                                        : PrefixBytes + ProfileLengthBytes;
                                    if (8L * headerBytes > availableBits)
                                    {
                                        throw NoHiddenData();
                                    }
                                    dataLength = headerBytes;
                                    Array.Resize(ref data, dataLength);
                                }
                                else if (!headerParsed && dataByte == headerBytes)
                                {
                                    headerParsed = true;
                                    long profileLength = BinaryPrimitives.ReadInt32BigEndian(
                                        data.AsSpan(headerBytes - ProfileLengthBytes, ProfileLengthBytes));
                                    if (profileLength <= 0
                                        || profileLength > MaxProfileBytes
                                        || (encrypted && profileLength <= PayloadCrypto.TagSize)
                                        || 8L * (headerBytes + profileLength) > availableBits
                                        || headerBytes + profileLength > Array.MaxLength)
                                    {
                                        throw NoHiddenData();
                                    }

                                    if (encrypted)
                                    {
                                        key = PayloadCrypto.DeriveKey(password!, data.AsSpan(PrefixBytes, PayloadCrypto.SaltSize));
                                    }

                                    profileStart = headerBytes;
                                    dataLength = (int)(headerBytes + profileLength);
                                    Array.Resize(ref data, dataLength);
                                }
                                else if (!profileParsed && dataByte == dataLength)
                                {
                                    profileParsed = true;
                                    DecodeProfile(
                                        data,
                                        encrypted,
                                        key,
                                        headerBytes,
                                        profileStart,
                                        dataLength - profileStart,
                                        out fileName,
                                        out long fileSize);

                                    long contentFieldBytes = fileSize + (encrypted ? PayloadCrypto.TagSize : 0);
                                    if (contentFieldBytes < 0 || contentFieldBytes > Array.MaxLength - dataLength)
                                    {
                                        throw NoHiddenData();
                                    }
                                    long totalBytes = dataLength + contentFieldBytes;
                                    if (8L * totalBytes > availableBits)
                                    {
                                        throw NoHiddenData();
                                    }

                                    dataStart = dataLength;
                                    dataLength = (int)totalBytes;
                                    Array.Resize(ref data, dataLength);
                                }

                                if (dataByte >= dataLength)
                                {
                                    complete = true;
                                    break;
                                }
                            }
                        }
                        maskBit >>= 1;
                    }

                    if (complete)
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

        if (!complete || fileName is null || dataStart < 0 || !profileParsed)
        {
            throw NoHiddenData();
        }

        byte[] stored = data[dataStart..];
        byte[] content;
        if (encrypted)
        {
            content = PayloadCrypto.Decrypt(
                key!,
                data.AsSpan(ContentNonceOffset(), PayloadCrypto.NonceSize),
                stored,
                data.AsSpan(0, headerBytes));
        }
        else
        {
            content = stored;
        }

        return new DecodedPayload(fileName, content)
        {
            WasEncrypted = encrypted,
        };
    }

    // 加密头部布局：magic(4)+version(1)+flags(1)+salt(16)+profileNonce(12)+contentNonce(12)
    private static int ProfileNonceOffset() => PrefixBytes + PayloadCrypto.SaltSize;
    private static int ContentNonceOffset() => PrefixBytes + PayloadCrypto.SaltSize + PayloadCrypto.NonceSize;

    private static bool ParsePrefix(byte[] data, string? password)
    {
        if (!data.AsSpan(0, 4).SequenceEqual(LsbEncoder.MagicBytes)
            || data[4] != LsbEncoder.FormatVersion
            || (data[5] & ~LsbEncoder.FlagEncrypted) != 0)
        {
            throw NoHiddenData();
        }

        bool encrypted = (data[5] & LsbEncoder.FlagEncrypted) != 0;
        if (encrypted && PayloadCrypto.IsEmptyPassword(password))
        {
            throw new StegoException(StegoError.WrongPassword, "该图片已加密，请输入密码。");
        }
        return encrypted;
    }

    private static void DecodeProfile(
        byte[] data,
        bool encrypted,
        byte[]? key,
        int headerBytes,
        int profileStart,
        int profileLength,
        out string fileName,
        out long fileSize)
    {
        ReadOnlySpan<byte> profileField = data.AsSpan(profileStart, profileLength);
        byte[] profilePlain;
        if (encrypted)
        {
            profilePlain = PayloadCrypto.Decrypt(
                key!,
                data.AsSpan(ProfileNonceOffset(), PayloadCrypto.NonceSize),
                profileField,
                data.AsSpan(0, headerBytes));
        }
        else
        {
            profilePlain = profileField.ToArray();
        }

        string profile;
        try
        {
            profile = StrictUtf8.GetString(profilePlain);
        }
        catch (DecoderFallbackException)
        {
            throw NoHiddenData();
        }

        (fileSize, fileName) = ProfileCodec.Parse(profile);
    }

    private static StegoException NoHiddenData() =>
        new(StegoError.NoHiddenData, "没有找到完整的隐藏数据，请确认图片与掩码是否正确。");
}
