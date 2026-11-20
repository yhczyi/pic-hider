using PicHider.Core;
using System.Drawing.Imaging;
using System.Reflection;

internal static class Program
{
    /// <summary>当前进程的可执行文件名，用于帮助与示例；发布后重命名 exe 也能正确显示。</summary>
    private static readonly string ProgramName = GetProgramName();

    private static string GetProgramName()
    {
        string name = Path.GetFileName(Environment.ProcessPath ?? "");
        if (string.IsNullOrWhiteSpace(name) ||
            name.Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            name = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location ?? "");
        }
        return string.IsNullOrWhiteSpace(name) ? "PicHider.Cli" : name;
    }

    private static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch (IOException)
        {
            // 输出被重定向时可能没有控制台，忽略即可。
        }

        if (args.Length == 0)
        {
            PrintUsage(Console.Error);
            return 2;
        }
        if (IsHelp(args[0]))
        {
            PrintUsage(Console.Out);
            return 0;
        }

        try
        {
            return args[0] switch
            {
                "--make-fixture" => RunMakeFixture(args),
                "--encode" => RunEncode(args),
                "--decode" => RunDecode(args),
                "--decode-auto" => RunDecodeAuto(args),
                _ => UnknownCommand(args[0]),
            };
        }
        catch (StegoException ex)
        {
            Console.Error.WriteLine($"错误：{ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"失败：{ex.Message}");
            return 2;
        }
    }

    private static bool IsHelp(string arg) => arg is "--help" or "-h" or "/?";

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"未知命令：{command}");
        Console.Error.WriteLine();
        PrintUsage(Console.Error);
        return 2;
    }

    /// <summary>生成测试素材：一张 256×256 测试图与一个 32 KiB 随机二进制文件。</summary>
    private static int RunMakeFixture(string[] args)
    {
        string dir = args.Length > 1 ? args[1] : Environment.CurrentDirectory;
        Directory.CreateDirectory(dir);
        MakeFixture(dir);
        return 0;
    }

    /// <summary>嵌入命令：把待隐藏文件写入载体图片，输出 PNG；掩码与密码均可选。</summary>
    private static int RunEncode(string[] args)
    {
        if (args.Length < 4)
        {
            PrintCommandUsage(
                Console.Error,
                "用法: --encode <cover> <data> <out.png> [mask] [password]",
                $"示例: {ProgramName} --encode cover.png secret.bin stego.png 00001111000011110000111100001111 \"口令\"");
            return 2;
        }
        // 参数按位置解析：第 5 个为掩码，第 6 个为密码；要指定密码必须同时给出掩码。
        Mask mask = args.Length > 4 ? Mask.Parse(args[4]) : Mask.Default;
        string? password = args.Length > 5 ? args[5] : null;
        var result = LsbEncoder.Encode(args[1], args[2], mask, password);

        // 输出路径的父级目录不存在时级联创建，避免保存 PNG 时因目录缺失失败。
        string? outputDir = Path.GetDirectoryName(Path.GetFullPath(args[3]));
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        using (result.StegoImage)
        {
            result.StegoImage.Save(args[3], ImageFormat.Png);
        }
        Console.WriteLine($"encoded -> {args[3]} (capacity={result.CapacityBytes}, payload={result.PayloadBytes}, encrypted={result.WasEncrypted})");
        return 0;
    }

    /// <summary>解码命令：按指定掩码从隐写图片还原原始文件；输出参数可为文件或已存在的目录。</summary>
    private static int RunDecode(string[] args)
    {
        if (args.Length < 3)
        {
            PrintCommandUsage(
                Console.Error,
                "用法: --decode <stego> <outFile|outDir> [mask] [password]",
                $"示例: {ProgramName} --decode stego.png out.bin 00001111000011110000111100001111 \"口令\"");
            return 2;
        }
        Mask mask = args.Length > 3 ? Mask.Parse(args[3]) : Mask.Default;
        string? password = args.Length > 4 ? args[4] : null;
        DecodedPayload decoded = LsbDecoder.Extract(args[1], mask, password);
        string outputPath = ResolveOutputPath(args[2], decoded.FileName);
        File.WriteAllBytes(outputPath, decoded.Data);
        Console.WriteLine($"decoded {decoded.FileName} ({decoded.Data.Length} bytes, encrypted={decoded.WasEncrypted}) -> {outputPath}");
        return 0;
    }

    /// <summary>自动解码命令：优先读取图片内嵌掩码，缺失或损坏时回退到指定掩码；输出参数可为文件或已存在的目录。</summary>
    private static int RunDecodeAuto(string[] args)
    {
        if (args.Length < 3)
        {
            PrintCommandUsage(
                Console.Error,
                "用法: --decode-auto <stego> <outFile|outDir> [fallbackMask] [password]",
                $"示例: {ProgramName} --decode-auto stego.png out.bin");
            return 2;
        }
        Mask fallback = args.Length > 3 ? Mask.Parse(args[3]) : Mask.Default;
        string? password = args.Length > 4 ? args[4] : null;
        DecodedPayload decoded = LsbDecoder.ExtractAuto(args[1], fallback, out Mask used, out bool embedded, password);
        string outputPath = ResolveOutputPath(args[2], decoded.FileName);
        File.WriteAllBytes(outputPath, decoded.Data);
        Console.WriteLine($"decoded {decoded.FileName} ({decoded.Data.Length} bytes, encrypted={decoded.WasEncrypted}) mask={used} embedded={embedded} -> {outputPath}");
        return 0;
    }

    /// <summary>
    /// 输出参数是已存在的目录，或以 "/"、"\" 结尾时按目录处理：目录不存在则创建，
    /// 并按载荷内文件名拼出实际输出路径；否则按文件路径使用。
    /// </summary>
    private static string ResolveOutputPath(string output, string fileName)
    {
        bool endsWithSeparator =
            output.EndsWith(Path.DirectorySeparatorChar) ||
            output.EndsWith(Path.AltDirectorySeparatorChar);
        bool isDirectory = Directory.Exists(output) || endsWithSeparator;
        if (!isDirectory)
        {
            return output;
        }

        Directory.CreateDirectory(output);

        string name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "decoded.bin";
        }
        return Path.Combine(output, name);
    }

    /// <summary>输出单条命令的用法与示例，供参数不足时提示。</summary>
    private static void PrintCommandUsage(TextWriter writer, string usage, string example)
    {
        writer.WriteLine(usage);
        writer.WriteLine(example);
    }

    /// <summary>生成渐变测试图 fixture.png 与固定种子随机数据 secret.bin。</summary>
    private static void MakeFixture(string dir)
    {
        string imagePath = Path.Combine(dir, "fixture.png");
        using (var bitmap = new Bitmap(256, 256, PixelFormat.Format32bppArgb))
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    int alpha = 128 + (x * 7 + y * 3) % 128;
                    int red = (x * 5) % 256;
                    int green = (y * 3) % 256;
                    int blue = (x + y * 2) % 256;
                    bitmap.SetPixel(x, y, Color.FromArgb(alpha, red, green, blue));
                }
            }
            bitmap.Save(imagePath, ImageFormat.Png);
        }

        string dataPath = Path.Combine(dir, "secret.bin");
        var random = new Random(42);
        byte[] payload = new byte[32 * 1024];
        random.NextBytes(payload);
        File.WriteAllBytes(dataPath, payload);

        Console.WriteLine(imagePath);
        Console.WriteLine(dataPath);
    }

    /// <summary>输出总帮助，包含全部命令的说明、用法与示例。</summary>
    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("PicHider CLI — 图片 LSB 隐写命令行工具");
        writer.WriteLine();
        writer.WriteLine("用法:");
        writer.WriteLine($"  {ProgramName} --make-fixture [目录]");
        writer.WriteLine($"  {ProgramName} --encode <cover> <data> <out.png> [mask] [password]");
        writer.WriteLine($"  {ProgramName} --decode <stego> <outFile|outDir> [mask] [password]");
        writer.WriteLine($"  {ProgramName} --decode-auto <stego> <outFile|outDir> [fallbackMask] [password]");
        writer.WriteLine($"  {ProgramName} --help");
        writer.WriteLine();
        writer.WriteLine("说明:");
        writer.WriteLine("  cover 为载体图片路径，支持 png / jpg / jpeg / bmp / gif / tif / tiff。");
        writer.WriteLine("  data 为待隐藏文件路径，支持任意类型文件。");
        writer.WriteLine("  stego 为隐写图片路径：编码时为输出 PNG，解码时为输入图片。");
        writer.WriteLine($"  mask 为 32 位（Alpha / Red / Green / Blue 各 8 位）二进制串，省略时使用默认掩码 {Mask.DefaultBinary}。");
        writer.WriteLine("  要指定密码，必须同时显式给出 mask。");
        writer.WriteLine("  隐写结果保存为 PNG；解码输出为原始文件字节。");
        writer.WriteLine("  嵌入的输出路径父级目录不存在时会自动创建。");
        writer.WriteLine("  解码的输出参数可以是文件路径，也可以是目录（已存在，或以 / 或 \\ 结尾；不存在会自动创建）；为目录时按载荷内文件名保存。");
        writer.WriteLine();
        writer.WriteLine("示例:");
        writer.WriteLine("  # 生成测试素材：256×256 图片 + 32 KiB 随机文件");
        writer.WriteLine($"  {ProgramName} --make-fixture output");
        writer.WriteLine();
        writer.WriteLine("  # 未加密嵌入：省略 mask 即使用默认掩码");
        writer.WriteLine($"  {ProgramName} --encode cover.png secret.bin stego.png");
        writer.WriteLine();
        writer.WriteLine("  # 加密嵌入：指定 mask 与密码");
        writer.WriteLine($"  {ProgramName} --encode cover.png secret.bin stego.png 00001111000011110000111100001111 \"口令\"");
        writer.WriteLine();
        writer.WriteLine("  # 按指定掩码解码");
        writer.WriteLine($"  {ProgramName} --decode stego.png out.bin 00001111000011110000111100001111 \"口令\"");
        writer.WriteLine();
        writer.WriteLine("  # 自动解码：优先使用图片内嵌掩码");
        writer.WriteLine($"  {ProgramName} --decode-auto stego.png out.bin");
        writer.WriteLine();
        writer.WriteLine("  # 输出到目录：按载荷内文件名保存；目录不存在会自动创建");
        writer.WriteLine($"  {ProgramName} --decode-auto stego.png output/");
    }
}
