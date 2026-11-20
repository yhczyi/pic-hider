using System.Globalization;
using System.Text;

namespace PicHider.Core;

public static class ProfileCodec
{
    public static byte[] Build(long fileSize, string fileName) =>
        Encoding.UTF8.GetBytes($"{fileSize},{fileName}");

    public static (long FileSize, string FileName) Parse(string plainText)
    {
        int comma = plainText.IndexOf(',');
        if (comma <= 0
            || !long.TryParse(plainText.AsSpan(0, comma), NumberStyles.None, CultureInfo.InvariantCulture, out long fileSize)
            || fileSize < 0)
        {
            throw new StegoException(StegoError.NoHiddenData, "隐藏数据的描述信息无效。");
        }

        string fileName = Path.GetFileName(plainText[(comma + 1)..]);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new StegoException(StegoError.NoHiddenData, "隐藏数据的文件名无效。");
        }
        return (fileSize, fileName);
    }
}
