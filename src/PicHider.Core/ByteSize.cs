namespace PicHider.Core;

public static class ByteSize
{
    private static readonly string[] Units = { "KB", "MB", "GB", "TB", "PB" };

    public static string Format(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes:N0} 字节";
        }

        double value = bytes;
        int unit = -1;
        while (value >= 1024 && unit < Units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return $"{bytes:N0} 字节（{value:0.##} {Units[unit]}）";
    }
}
