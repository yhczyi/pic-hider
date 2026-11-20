using PicHider.Core;
using System.Drawing.Imaging;
using System.Globalization;

namespace PicHider.App;

public partial class MainForm
{
    private static readonly string[] RatioPresets =
    {
        "1:1", "4:3", "3:2", "16:9", "9:16", "3:4", "2:3", "5:4", "21:9",
    };

    private RadioButton[] _ratioRadios = null!;
    private EncodeResult? _generateResult;

    private void OnBrowseGenerateDataClick(object? sender, EventArgs e) =>
        BrowseFile(_generateDataPathBox, "所有文件|*.*");

    private void OnGenerateDataPathLeave(object? sender, EventArgs e) =>
        NormalizePathBox(_generateDataPathBox);

    private void OnGenerateInputChanged(object? sender, EventArgs e) =>
        InvalidateGenerateResult();

    private void OnRatioCheckedChanged(object? sender, EventArgs e)
    {
        _customRatioBox.Enabled = _customRatioRadio.Checked;
        InvalidateGenerateResult();
    }

    private bool TryGetSelectedRatio(out int width, out int height)
    {
        if (_customRatioRadio.Checked)
        {
            return TryParseRatio(_customRatioBox.Text, out width, out height);
        }
        foreach (RadioButton radio in _ratioRadios)
        {
            if (radio.Checked)
            {
                return TryParseRatio(radio.Text, out width, out height);
            }
        }
        width = 0;
        height = 0;
        return false;
    }

    private string GetSelectedRatioText()
    {
        if (_customRatioRadio.Checked)
        {
            return _customRatioBox.Text.Trim();
        }
        foreach (RadioButton radio in _ratioRadios)
        {
            if (radio.Checked)
            {
                return radio.Text;
            }
        }
        return RatioPresets[3];
    }

    private void SelectRatio(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        string value = text.Trim();
        foreach (RadioButton radio in _ratioRadios)
        {
            if (string.Equals(radio.Text, value, StringComparison.OrdinalIgnoreCase))
            {
                radio.Checked = true;
                return;
            }
        }
        _customRatioRadio.Checked = true;
        _customRatioBox.Text = value;
    }

    private void InvalidateGenerateResult()
    {
        _generatePicture.Image = null;
        _generateResult?.StegoImage.Dispose();
        _generateResult = null;
        _saveGeneratedButton.Enabled = false;
    }

    private async void OnGenerateClick(object? sender, EventArgs e)
    {
        if (_busy)
        {
            return;
        }

        string dataPath = NormalizePathBox(_generateDataPathBox);
        string password = _generatePasswordBox.Text;
        if (!TryGetSelectedRatio(out int ratioWidth, out int ratioHeight))
        {
            SetStatus("请选择图片比例，或选择“自定义”后输入如 5:4、1920x1080。", true);
            return;
        }
        if (string.IsNullOrWhiteSpace(dataPath) || !File.Exists(dataPath))
        {
            SetStatus("请选择存在的待隐藏文件。", true);
            return;
        }
        Mask mask = ReadMask();
        if (mask.IsEmpty)
        {
            SetStatus("请至少勾选一个数据位。", true);
            return;
        }
        SaveUiState();

        SetBusy(true, "正在生成载体并嵌入...");
        try
        {
            EncodeResult result = await Task.Run(() =>
            {
                var fileInfo = new FileInfo(dataPath);
                long requiredBytes = LsbEncoder.ComputePayloadBytes(
                    fileInfo.Length,
                    Path.GetFileName(dataPath),
                    !PayloadCrypto.IsEmptyPassword(password));
                (int width, int height) = CoverGenerator.ComputeSize(
                    ratioWidth,
                    ratioHeight,
                    requiredBytes,
                    mask.PopCount);

                Bitmap cover = CoverGenerator.Create(width, height);
                try
                {
                    return LsbEncoder.Encode(cover, dataPath, mask, password);
                }
                catch
                {
                    cover.Dispose();
                    throw;
                }
            });

            _generatePicture.Image = null;
            _generateResult?.StegoImage.Dispose();
            _generateResult = result;
            _generatePicture.Image = result.StegoImage;
            _saveGeneratedButton.Enabled = true;
            SetStatus(
                $"已生成 {result.StegoImage.Width}×{result.StegoImage.Height} 载体并嵌入：文件 {ByteSize.Format(result.PayloadBytes)}，可隐藏 {ByteSize.Format(result.CapacityBytes)}，掩码有效位 {result.MaskBitCount}"
                + (result.WasEncrypted ? "，已使用密码加密" : "，未加密")
                + (result.FooterWritten ? "，已写入图片内嵌掩码。" : "，未写入内嵌掩码。"),
                false);
        }
        catch (StegoException ex)
        {
            InvalidateGenerateResult();
            SetStatus(ex.Message, true);
        }
        catch (Exception ex)
        {
            InvalidateGenerateResult();
            SetStatus($"生成失败：{ex.Message}", true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnSaveGeneratedClick(object? sender, EventArgs e)
    {
        if (_generateResult is null)
        {
            return;
        }

        string dataPath = NormalizePath(_generateDataPathBox.Text);
        string directory = dataPath.Length > 0
            ? Path.GetDirectoryName(Path.GetFullPath(dataPath)) ?? Environment.CurrentDirectory
            : Environment.CurrentDirectory;
        string baseName = dataPath.Length > 0 ? Path.GetFileNameWithoutExtension(dataPath) : "生成";
        using var dialog = new SaveFileDialog
        {
            Filter = "PNG 图片 (*.png)|*.png",
            DefaultExt = "png",
            FileName = $"{baseName}-隐写.png",
            InitialDirectory = directory,
            OverwritePrompt = true,
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            if (dataPath.Length > 0
                && string.Equals(Path.GetFullPath(dialog.FileName), Path.GetFullPath(dataPath), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "不能覆盖待隐藏文件，请选择其他文件名。", "保存", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _generateResult.StegoImage.Save(dialog.FileName, ImageFormat.Png);
            SetStatus($"已保存：{dialog.FileName}", false);
        }
        catch (Exception ex)
        {
            SetStatus($"保存失败：{ex.Message}", true);
        }
    }

    /// <summary>支持 16:9、16：9、1920x1080、1920×1080、16/9 等写法，并约分。</summary>
    private static bool TryParseRatio(string? text, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] parts = text.Split(
            new[] { ':', '：', 'x', 'X', '×', '/' },
            StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }
        if (!int.TryParse(parts[0].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out width)
            || !int.TryParse(parts[1].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out height))
        {
            return false;
        }
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        int divisor = Gcd(width, height);
        width /= divisor;
        height /= divisor;
        return width <= CoverGenerator.MaxDimension && height <= CoverGenerator.MaxDimension;
    }

    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        return a;
    }
}
