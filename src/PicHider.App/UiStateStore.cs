using System.Text.Json;

namespace PicHider.App;

internal sealed record UiState(
    string? Mask,
    string? CoverPath,
    string? DataPath,
    string? StegoPath,
    string? EmbedPassword,
    string? DecodePassword,
    string? GeneratePath = null,
    string? GeneratePassword = null,
    string? GenerateRatio = null);

internal static class UiStateStore
{
    private const string CacheFolderName = ".pic-hider";
    private const string StateFileName = "state.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>状态文件放在含 PicHider.sln 的目录下的 .pic-hider；找不到时用程序目录。</summary>
    public static string CacheDirectory
    {
        get
        {
            for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            {
                if (File.Exists(Path.Combine(dir.FullName, "PicHider.sln")))
                {
                    return Path.Combine(dir.FullName, CacheFolderName);
                }
            }
            return Path.Combine(AppContext.BaseDirectory, CacheFolderName);
        }
    }

    public static UiState? Load()
    {
        try
        {
            string path = Path.Combine(CacheDirectory, StateFileName);
            if (!File.Exists(path))
            {
                return null;
            }
            return JsonSerializer.Deserialize<UiState>(File.ReadAllText(path));
        }
        catch
        {
            return null;
        }
    }

    public static void Save(UiState state)
    {
        try
        {
            string directory = CacheDirectory;
            Directory.CreateDirectory(directory);
            new DirectoryInfo(directory).Attributes |= FileAttributes.Hidden;

            // 隐藏目录即可；文件本身不能带 Hidden 属性，否则 Win32 CREATE_ALWAYS 会拒绝写入
            string path = Path.Combine(directory, StateFileName);
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
            File.WriteAllText(path, JsonSerializer.Serialize(state, JsonOptions));
        }
        catch
        {
            // 缓存失败不影响主流程
        }
    }
}
