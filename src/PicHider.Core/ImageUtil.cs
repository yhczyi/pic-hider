using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PicHider.Core;

public static class ImageUtil
{
    /// <summary>读取为独立的 32bppArgb 位图，并立即释放文件句柄。</summary>
    public static Bitmap Load32bppArgb(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new StegoException(StegoError.FileNotFound, "请先选择图片文件。");
        }
        if (!File.Exists(path))
        {
            throw new StegoException(StegoError.FileNotFound, $"图片文件不存在：{path}");
        }

        try
        {
            using var source = Image.FromFile(path);
            return To32bppArgb(source);
        }
        catch (Exception ex) when (ex is not StegoException)
        {
            throw new StegoException(StegoError.ImageLoadFailed, "无法读取图片文件，请确认图片格式是否受支持。", ex);
        }
    }

    public static Bitmap To32bppArgb(Image source)
    {
        if (source is Bitmap bitmap && bitmap.PixelFormat == PixelFormat.Format32bppArgb)
        {
            return (Bitmap)bitmap.Clone();
        }

        var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        try
        {
            using var graphics = Graphics.FromImage(result);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height));
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }
}
