using System.Buffers.Binary;
using System.Drawing.Imaging;

namespace PicHider.Core;

/// <summary>
/// 末尾 80 个像素（行优先）Blue bit0 中的自描述掩码：4B "PHMF" + 1B 版本 + 4B 掩码 + 1B XOR 校验。
/// 读取不依赖 payload 掩码，解码时可先恢复掩码再提取。
/// </summary>
public static class MaskFooter
{
    public const uint Magic = 0x5048_4D46; // "PHMF" = PicHider Mask Footer
    public const byte CurrentVersion = 1;
    public const int FooterBytes = 10;
    public const int FooterBits = FooterBytes * 8; // 每像素 1 bit，即 80 个像素

    private const uint BlueLsb = 0x0000_0001u;

    public static bool HasRoom(Bitmap image) => PixelCount(image) > FooterBits;

    public static void Write(Bitmap image, Mask mask)
    {
        if (!HasRoom(image))
        {
            throw new StegoException(StegoError.CapacityExceeded, "图片太小，无法写入内嵌掩码。");
        }
        if (mask.IsEmpty)
        {
            throw new StegoException(StegoError.InvalidMask, "掩码不能为空。");
        }

        byte[] footer = Build(mask);
        WriteBits(image, PixelCount(image) - FooterBits, footer);
    }

    public static bool TryRead(Bitmap image, out Mask mask, out byte version)
    {
        mask = default;
        version = 0;
        if (PixelCount(image) <= FooterBits)
        {
            return false;
        }

        byte[] footer = ReadBits(image, PixelCount(image) - FooterBits, FooterBytes);
        return TryValidate(footer, out mask, out version);
    }

    public static bool TryReadFile(string path, out Mask mask, out byte version)
    {
        using Bitmap image = ImageUtil.Load32bppArgb(path);
        return TryRead(image, out mask, out version);
    }

    /// <summary>
    /// 未写新页脚时调用：清零 payload 没占用的 Blue bit0，避免读到上一次嵌入的旧页脚。
    /// <paramref name="firstPayloadPixel"/> 之前的像素可能属于 payload，不能动；掩码不含 Blue bit0 时传 0。
    /// </summary>
    internal static void InvalidateStale(Bitmap image, long firstPayloadPixel)
    {
        long count = PixelCount(image);
        if (count <= FooterBits)
        {
            return;
        }

        long start = Math.Max(count - FooterBits, firstPayloadPixel);
        if (start >= count)
        {
            // payload 覆盖了所有页脚位，却仍能读出有效页脚
            if (TryRead(image, out _, out _))
            {
                throw new StegoException(StegoError.CapacityExceeded, "图片可用空间不足，无法清除旧的内嵌掩码。");
            }
            return;
        }

        SetBlueLsbRange(image, start, count, value: false);
        if (TryRead(image, out _, out _))
        {
            // 翻转一个空闲位，破坏魔数/版本/校验之一
            ToggleBlueLsb(image, start);
        }
    }

    internal static byte[] Build(Mask mask)
    {
        var footer = new byte[FooterBytes];
        BinaryPrimitives.WriteUInt32BigEndian(footer.AsSpan(0, 4), Magic);
        footer[4] = CurrentVersion;
        BinaryPrimitives.WriteUInt32BigEndian(footer.AsSpan(5, 4), mask.Bits);
        footer[9] = Checksum(footer);
        return footer;
    }

    internal static bool TryValidate(ReadOnlySpan<byte> footer, out Mask mask, out byte version)
    {
        mask = default;
        version = 0;
        if (footer.Length < FooterBytes
            || BinaryPrimitives.ReadUInt32BigEndian(footer[..4]) != Magic)
        {
            return false;
        }

        version = footer[4];
        if (version != CurrentVersion || footer[9] != Checksum(footer))
        {
            return false;
        }

        uint bits = BinaryPrimitives.ReadUInt32BigEndian(footer.Slice(5, 4));
        if (bits == 0)
        {
            return false;
        }

        mask = Mask.FromBits(bits);
        return true;
    }

    private static byte Checksum(ReadOnlySpan<byte> footer)
    {
        byte xor = 0;
        for (int i = 0; i < FooterBytes - 1; i++)
        {
            xor ^= footer[i];
        }
        return xor;
    }

    private static long PixelCount(Bitmap image) => (long)image.Width * image.Height;

    private static unsafe byte[] ReadBits(Bitmap image, long startPixel, int byteCount)
    {
        var bytes = new byte[byteCount];
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        BitmapData data = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int k = 0; k < byteCount * 8; k++)
            {
                long pixel = startPixel + k;
                int y = (int)(pixel / image.Width);
                int x = (int)(pixel % image.Width);
                uint value = (uint)((int*)((byte*)data.Scan0 + (long)y * data.Stride))[x];
                if ((value & BlueLsb) != 0)
                {
                    bytes[k >> 3] |= (byte)(0x80 >> (k & 7));
                }
            }
        }
        finally
        {
            image.UnlockBits(data);
        }
        return bytes;
    }

    private static unsafe void WriteBits(Bitmap image, long startPixel, ReadOnlySpan<byte> bytes)
    {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        BitmapData data = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            for (int k = 0; k < bytes.Length * 8; k++)
            {
                long pixel = startPixel + k;
                int y = (int)(pixel / image.Width);
                int x = (int)(pixel % image.Width);
                int* row = (int*)((byte*)data.Scan0 + (long)y * data.Stride);
                uint value = (uint)row[x];
                if ((bytes[k >> 3] & (0x80 >> (k & 7))) != 0)
                {
                    value |= BlueLsb;
                }
                else
                {
                    value &= ~BlueLsb;
                }
                row[x] = (int)value;
            }
        }
        finally
        {
            image.UnlockBits(data);
        }
    }

    private static unsafe void SetBlueLsbRange(Bitmap image, long startPixel, long endPixel, bool value)
    {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        BitmapData data = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            for (long pixel = startPixel; pixel < endPixel; pixel++)
            {
                int y = (int)(pixel / image.Width);
                int x = (int)(pixel % image.Width);
                int* row = (int*)((byte*)data.Scan0 + (long)y * data.Stride);
                uint current = (uint)row[x];
                row[x] = (int)(value ? (current | BlueLsb) : (current & ~BlueLsb));
            }
        }
        finally
        {
            image.UnlockBits(data);
        }
    }

    private static unsafe void ToggleBlueLsb(Bitmap image, long pixel)
    {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        BitmapData data = image.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            int y = (int)(pixel / image.Width);
            int x = (int)(pixel % image.Width);
            int* row = (int*)((byte*)data.Scan0 + (long)y * data.Stride);
            row[x] ^= (int)BlueLsb;
        }
        finally
        {
            image.UnlockBits(data);
        }
    }
}
