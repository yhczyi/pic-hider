using System.Drawing.Imaging;

namespace PicHider.Core;

/// <summary>按宽高比生成渐变载体图；尺寸自动放大到能容纳 payload 并预留页脚。</summary>
public static class CoverGenerator
{
    public const int MinLongEdge = 1920;
    public const int MaxDimension = 8192;
    public const long MaxPixels = 40_000_000;

    /// <summary>满足容量且长边不小于 <see cref="MinLongEdge"/> 的最小整数倍尺寸。</summary>
    public static (int Width, int Height) ComputeSize(int ratioWidth, int ratioHeight, long requiredBytes, int bitsPerPixel)
    {
        if (ratioWidth <= 0 || ratioHeight <= 0 || ratioWidth > MaxDimension || ratioHeight > MaxDimension)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ratioWidth),
                $"图片比例必须是 1 到 {MaxDimension} 之间的正整数。");
        }
        if (bitsPerPixel <= 0)
        {
            throw new StegoException(StegoError.InvalidMask, "请至少勾选一个数据位。");
        }
        if (requiredBytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredBytes));
        }

        long requiredBits;
        long requiredPixels;
        try
        {
            requiredBits = checked(requiredBytes * 8);
            requiredPixels = (requiredBits + bitsPerPixel - 1) / bitsPerPixel + MaskFooter.FooterBits;
        }
        catch (OverflowException ex)
        {
            throw new StegoException(StegoError.CapacityExceeded, "待隐藏文件过大。", ex);
        }

        long basePixels = (long)ratioWidth * ratioHeight;
        int longEdge = Math.Max(ratioWidth, ratioHeight);
        long scaleForLongEdge = (MinLongEdge + longEdge - 1L) / longEdge;
        long scaleForCapacity = (long)Math.Ceiling(Math.Sqrt((double)requiredPixels / basePixels));
        long scale = Math.Max(1L, Math.Max(scaleForLongEdge, scaleForCapacity));
        while ((long)ratioWidth * scale * ratioHeight * scale < requiredPixels)
        {
            scale++;
        }

        long width = ratioWidth * scale;
        long height = ratioHeight * scale;
        if (width > MaxDimension || height > MaxDimension || width * height > MaxPixels)
        {
            throw new StegoException(
                StegoError.CapacityExceeded,
                $"文件过大：按当前比例需要 {width:N0}×{height:N0} 的载体图片，超过允许的最大尺寸 {MaxDimension:N0}×{MaxDimension:N0}。");
        }

        return ((int)width, (int)height);
    }

    /// <summary>生成四角随机颜色的双线性渐变图，并叠加一处柔和光晕。</summary>
    public static unsafe Bitmap Create(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "图片尺寸必须为正数。");
        }

        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        try
        {
            Random random = Random.Shared;
            Color c00 = RandomColor(random);
            Color c10 = RandomColor(random);
            Color c01 = RandomColor(random);
            Color c11 = RandomColor(random);
            double glowX = 0.2 + random.NextDouble() * 0.6;
            double glowY = 0.2 + random.NextDouble() * 0.6;

            double invW = 1.0 / Math.Max(1, width - 1);
            double invH = 1.0 / Math.Max(1, height - 1);
            var rect = new Rectangle(0, 0, width, height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    double fy = y * invH;
                    int* row = (int*)((byte*)data.Scan0 + y * data.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        double fx = x * invW;
                        int r = Bilinear(c00.R, c10.R, c01.R, c11.R, fx, fy);
                        int g = Bilinear(c00.G, c10.G, c01.G, c11.G, fx, fy);
                        int b = Bilinear(c00.B, c10.B, c01.B, c11.B, fx, fy);

                        double dx = fx - glowX;
                        double dy = fy - glowY;
                        double glow = Math.Max(0, 1 - (dx * dx + dy * dy) * 3.0) * 0.22;
                        r += (int)((255 - r) * glow);
                        g += (int)((255 - g) * glow);
                        b += (int)((255 - b) * glow);

                        row[x] = (int)(0xFF000000u | (uint)(r << 16) | (uint)(g << 8) | (uint)b);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    private static int Bilinear(int c00, int c10, int c01, int c11, double fx, double fy)
    {
        double top = c00 + (c10 - c00) * fx;
        double bottom = c01 + (c11 - c01) * fx;
        return (int)Math.Round(top + (bottom - top) * fy);
    }

    private static Color RandomColor(Random random) =>
        Color.FromArgb(40 + random.Next(176), 40 + random.Next(176), 40 + random.Next(176));
}
