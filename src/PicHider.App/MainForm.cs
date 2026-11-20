using PicHider.Core;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace PicHider.App;

public partial class MainForm : Form
{
    private static readonly string[] ChannelNames = { "Alpha", "Red", "Green", "Blue" };
    private static readonly string ImageFilter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|所有文件|*.*";

    private readonly CheckBox[,] _bitChecks = new CheckBox[4, 8];
    private bool _suppressMaskEvents;
    private bool _busy;

    private Bitmap? _coverImage;
    private EncodeResult? _embedResult;

    private Bitmap? _decodeImage;
    private DecodeResult? _decodeResult;

    public MainForm()
    {
        InitializeComponent();

        _ratioRadios = new[]
        {
            _ratioRadio0, _ratioRadio1, _ratioRadio2, _ratioRadio3, _ratioRadio4,
            _ratioRadio5, _ratioRadio6, _ratioRadio7, _ratioRadio8,
        };

        Rectangle area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 800);
        MinimumSize = new Size(Math.Min(880, area.Width - 40), Math.Min(680, area.Height - 40));
        Size = new Size(Math.Min(1100, area.Width - 40), Math.Min(820, area.Height - 40));

        _dataPathBox.Leave += OnDataPathLeave;
        Disposed += (_, _) =>
        {
            _embedResult?.StegoImage.Dispose();
            _generateResult?.StegoImage.Dispose();
            _coverImage?.Dispose();
            _decodeImage?.Dispose();
        };
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        BuildMaskGrid();
        RestoreUiState();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        SaveUiState();
        base.OnFormClosing(e);
    }

    private void RestoreUiState()
    {
        UiState? state = UiStateStore.Load();
        if (state is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(state.Mask))
        {
            try
            {
                Mask mask = Mask.Parse(state.Mask);
                _suppressMaskEvents = true;
                try
                {
                    for (int r = 0; r < 4; r++)
                    {
                        for (int c = 0; c < 8; c++)
                        {
                            _bitChecks[r, c].Checked = (mask.Bits & (1u << (31 - (r * 8 + c)))) != 0;
                        }
                    }
                }
                finally
                {
                    _suppressMaskEvents = false;
                }
                OnMaskChanged(null, EventArgs.Empty);
            }
            catch (StegoException)
            {
                // 缓存里的掩码无效时忽略，保留默认掩码
            }
        }

        if (state.EmbedPassword is not null)
        {
            _embedPasswordBox.Text = state.EmbedPassword;
        }
        if (state.DecodePassword is not null)
        {
            _decodePasswordBox.Text = state.DecodePassword;
        }

        string coverPath = NormalizePath(state.CoverPath);
        if (coverPath.Length > 0)
        {
            _coverPathBox.Text = coverPath;
        }
        string dataPath = NormalizePath(state.DataPath);
        if (dataPath.Length > 0)
        {
            _dataPathBox.Text = dataPath;
        }
        string stegoPath = NormalizePath(state.StegoPath);
        if (stegoPath.Length > 0)
        {
            _stegoPathBox.Text = stegoPath;
        }
        string generatePath = NormalizePath(state.GeneratePath);
        if (generatePath.Length > 0)
        {
            _generateDataPathBox.Text = generatePath;
        }
        if (state.GeneratePassword is not null)
        {
            _generatePasswordBox.Text = state.GeneratePassword;
        }
        SelectRatio(state.GenerateRatio);

        LoadCoverPreview();
        LoadStegoPreview();
    }

    private void SaveUiState()
    {
        UiStateStore.Save(new UiState(
            ReadMask().ToBinaryString(),
            NormalizePath(_coverPathBox.Text),
            NormalizePath(_dataPathBox.Text),
            NormalizePath(_stegoPathBox.Text),
            _embedPasswordBox.Text,
            _decodePasswordBox.Text,
            NormalizePath(_generateDataPathBox.Text),
            _generatePasswordBox.Text,
            GetSelectedRatioText()));
    }

    /// <summary>去掉粘贴路径时携带的首尾引号（Windows 文件名不允许引号）。</summary>
    private static string NormalizePath(string? text)
    {
        string path = text?.Trim() ?? string.Empty;
        int start = 0;
        int end = path.Length - 1;
        while (start <= end && IsPathQuote(path[start]))
        {
            start++;
        }
        while (end >= start && IsPathQuote(path[end]))
        {
            end--;
        }
        return start <= end ? path[start..(end + 1)] : string.Empty;
    }

    private static bool IsPathQuote(char c) => c is '"' or '“' or '”';

    private static string NormalizePathBox(TextBox box)
    {
        string normalized = NormalizePath(box.Text);
        if (!string.Equals(normalized, box.Text, StringComparison.Ordinal))
        {
            box.Text = normalized;
        }
        return normalized;
    }

    /// <summary>掩码矩阵是 4×8 循环生成的动态控件，不适合放进设计器文件。</summary>
    private void BuildMaskGrid()
    {
        _maskTable.SuspendLayout();
        try
        {
            for (int c = 0; c < 8; c++)
            {
                var columnButton = new Button
                {
                    Text = "全选",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    Tag = c,
                };
                columnButton.Click += OnColumnSelectClick;
                _toolTip.SetToolTip(columnButton, $"全选该列 4 个通道的 bit{7 - c}");
                _maskTable.Controls.Add(columnButton, c + 1, 0);
            }

            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    var check = new CheckBox
                    {
                        Checked = c >= 4,
                        AutoSize = false,
                        Dock = DockStyle.Fill,
                        Text = string.Empty,
                        CheckAlign = ContentAlignment.MiddleCenter,
                        Margin = new Padding(4, 2, 4, 2),
                    };
                    check.CheckedChanged += OnMaskChanged;
                    _toolTip.SetToolTip(check, $"{ChannelNames[r]} bit{7 - c}");
                    _bitChecks[r, c] = check;
                    _maskTable.Controls.Add(check, c + 1, r + 1);
                }

                var rowButton = new Button
                {
                    Text = "整行全选",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(6, 2, 2, 2),
                    Tag = r,
                };
                rowButton.Click += OnRowSelectClick;
                _toolTip.SetToolTip(rowButton, $"全选 {ChannelNames[r]} 通道的 8 个 bit");
                _maskTable.Controls.Add(rowButton, 9, r + 1);
            }
        }
        finally
        {
            _maskTable.ResumeLayout(false);
            _maskTable.PerformLayout();
        }
    }

    private void BrowseFile(TextBox target, string filter)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = filter,
            CheckFileExists = true,
        };
        string current = NormalizePath(target.Text);
        if (File.Exists(current))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(current);
            dialog.FileName = Path.GetFileName(current);
        }
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            target.Text = dialog.FileName;
        }
    }

    private void OnBrowseCoverClick(object? sender, EventArgs e)
    {
        BrowseFile(_coverPathBox, ImageFilter);
        LoadCoverPreview();
    }

    private void OnBrowseDataClick(object? sender, EventArgs e) => BrowseFile(_dataPathBox, "所有文件|*.*");

    private void OnBrowseStegoClick(object? sender, EventArgs e)
    {
        BrowseFile(_stegoPathBox, ImageFilter);
        LoadStegoPreview();
    }

    private void OnPathKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }
        e.SuppressKeyPress = true;
        if (ReferenceEquals(sender, _coverPathBox))
        {
            LoadCoverPreview();
        }
        else if (ReferenceEquals(sender, _stegoPathBox))
        {
            LoadStegoPreview();
        }
    }

    private Mask ReadMask()
    {
        Span<char> chars = stackalloc char[Mask.BitWidth];
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                chars[r * 8 + c] = _bitChecks[r, c].Checked ? '1' : '0';
            }
        }
        return Mask.Parse(new string(chars));
    }

    /// <summary>同步勾选框但不触发 OnMaskChanged，避免作废刚完成的解码结果。</summary>
    private void ApplyMaskToChecks(Mask mask)
    {
        _suppressMaskEvents = true;
        try
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    _bitChecks[r, c].Checked = (mask.Bits & (1u << (31 - (r * 8 + c)))) != 0;
                }
            }
        }
        finally
        {
            _suppressMaskEvents = false;
        }
        InvalidateGenerateResult();
    }

    private void OnMaskChanged(object? sender, EventArgs e)
    {
        if (_suppressMaskEvents)
        {
            return;
        }
        InvalidateEmbedResult();
        InvalidateDecodeResult();
        InvalidateGenerateResult();
    }

    private void OnRowSelectClick(object? sender, EventArgs e)
    {
        int row = (int)((Button)sender!).Tag!;
        SetChecks(targetState: !RowFullyChecked(row), r => r == row, null);
    }

    private void OnColumnSelectClick(object? sender, EventArgs e)
    {
        int column = (int)((Button)sender!).Tag!;
        SetChecks(targetState: !ColumnFullyChecked(column), null, c => c == column);
    }

    private bool RowFullyChecked(int row)
    {
        for (int c = 0; c < 8; c++)
        {
            if (!_bitChecks[row, c].Checked)
            {
                return false;
            }
        }
        return true;
    }

    private bool ColumnFullyChecked(int column)
    {
        for (int r = 0; r < 4; r++)
        {
            if (!_bitChecks[r, column].Checked)
            {
                return false;
            }
        }
        return true;
    }

    private void SetChecks(bool targetState, Func<int, bool>? rowFilter, Func<int, bool>? columnFilter)
    {
        _suppressMaskEvents = true;
        try
        {
            for (int r = 0; r < 4; r++)
            {
                if (rowFilter is not null && !rowFilter(r))
                {
                    continue;
                }
                for (int c = 0; c < 8; c++)
                {
                    if (columnFilter is not null && !columnFilter(c))
                    {
                        continue;
                    }
                    _bitChecks[r, c].Checked = targetState;
                }
            }
        }
        finally
        {
            _suppressMaskEvents = false;
        }
        OnMaskChanged(null, EventArgs.Empty);
    }

    private void OnCoverPathChanged(object? sender, EventArgs e) => InvalidateEmbedResult();

    private void OnDataPathChanged(object? sender, EventArgs e) => InvalidateEmbedResult();

    private void OnEmbedPasswordChanged(object? sender, EventArgs e) => InvalidateEmbedResult();

    private void OnStegoPathChanged(object? sender, EventArgs e) => InvalidateDecodeResult();

    private void OnDecodePasswordChanged(object? sender, EventArgs e) => InvalidateDecodeResult();

    private void OnViewOriginalChanged(object? sender, EventArgs e) => RefreshEmbedPreview();

    private void OnCoverPathLeave(object? sender, EventArgs e) => LoadCoverPreview();

    private void OnDataPathLeave(object? sender, EventArgs e) => NormalizePathBox(_dataPathBox);

    private void OnStegoPathLeave(object? sender, EventArgs e) => LoadStegoPreview();

    private void LoadCoverPreview()
    {
        _coverImage?.Dispose();
        _coverImage = null;
        string path = NormalizePathBox(_coverPathBox);
        if (path.Length > 0 && File.Exists(path))
        {
            try
            {
                _coverImage = ImageUtil.Load32bppArgb(path);
            }
            catch
            {
                _coverImage = null;
            }
        }
        RefreshEmbedPreview();
    }

    private void LoadStegoPreview()
    {
        _decodeImage?.Dispose();
        _decodeImage = null;
        string path = NormalizePathBox(_stegoPathBox);
        if (path.Length > 0 && File.Exists(path))
        {
            try
            {
                _decodeImage = ImageUtil.Load32bppArgb(path);
            }
            catch
            {
                _decodeImage = null;
            }
        }
        _decodePicture.Image = null;
        _decodePicture.Image = _decodeImage;
    }

    private void RefreshEmbedPreview()
    {
        Bitmap? target = _embedResult is not null && !_viewOriginalCheck.Checked
            ? _embedResult.StegoImage
            : _coverImage;
        _embedPicture.Image = null;
        _embedPicture.Image = target;
    }

    private void InvalidateEmbedResult()
    {
        _embedResult?.StegoImage.Dispose();
        _embedResult = null;
        _saveEmbedButton.Enabled = false;
        RefreshEmbedPreview();
    }

    private void InvalidateDecodeResult()
    {
        _decodeResult = null;
        _saveDecodedButton.Enabled = false;
        _openFolderButton.Enabled = false;
        _decodedPathBox.Clear();
    }

    private async void OnEmbedClick(object? sender, EventArgs e)
    {
        if (_busy)
        {
            return;
        }
        string coverPath = NormalizePathBox(_coverPathBox);
        string dataPath = NormalizePathBox(_dataPathBox);
        string password = _embedPasswordBox.Text;
        Mask mask = ReadMask();
        SaveUiState();

        SetBusy(true, "正在嵌入...");
        try
        {
            EncodeResult result = await Task.Run(() => LsbEncoder.Encode(coverPath, dataPath, mask, password));

            _embedResult?.StegoImage.Dispose();
            _embedResult = result;
            _viewOriginalCheck.Checked = false;
            RefreshEmbedPreview();
            _saveEmbedButton.Enabled = true;
            SetStatus(
                $"嵌入完成：文件 {ByteSize.Format(result.PayloadBytes)}，图片可隐藏 {ByteSize.Format(result.CapacityBytes)}，掩码有效位 {result.MaskBitCount}"
                + (result.WasEncrypted ? "，已使用密码加密" : "，未加密")
                + (result.FooterWritten ? "，已写入图片内嵌掩码。" : "，未写入内嵌掩码（空间不足或图片过小）。"),
                false);
        }
        catch (StegoException ex)
        {
            InvalidateEmbedResult();
            SetStatus(ex.Message, true);
        }
        catch (Exception ex)
        {
            InvalidateEmbedResult();
            SetStatus($"嵌入失败：{ex.Message}", true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnSaveEmbedClick(object? sender, EventArgs e)
    {
        if (_embedResult is null)
        {
            return;
        }

        string coverPath = NormalizePath(_coverPathBox.Text);
        string directory = Path.GetDirectoryName(Path.GetFullPath(coverPath)) ?? Environment.CurrentDirectory;
        string baseName = Path.GetFileNameWithoutExtension(coverPath);
        using var dialog = new SaveFileDialog
        {
            Filter = "PNG 图片 (*.png)|*.png",
            DefaultExt = "png",
            FileName = $"{baseName}-嵌入后.png",
            InitialDirectory = directory,
            OverwritePrompt = true,
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            if (string.Equals(Path.GetFullPath(dialog.FileName), Path.GetFullPath(coverPath), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "不能覆盖载体原图，请选择其他文件名。", "保存", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _embedResult.StegoImage.Save(dialog.FileName, ImageFormat.Png);
            SetStatus($"已保存：{dialog.FileName}", false);
        }
        catch (Exception ex)
        {
            SetStatus($"保存失败：{ex.Message}", true);
        }
    }

    private async void OnExtractClick(object? sender, EventArgs e)
    {
        if (_busy)
        {
            return;
        }
        string stegoPath = NormalizePathBox(_stegoPathBox);
        string password = _decodePasswordBox.Text;
        Mask mask = ReadMask();
        SaveUiState();

        SetBusy(true, "正在解码...");
        try
        {
            DecodeResult result = await Task.Run(() => LsbDecoder.DecodeToFileAuto(stegoPath, mask, password));

            _decodeResult = result;
            _decodedPathBox.Text = result.OutputPath;
            _saveDecodedButton.Enabled = true;
            _openFolderButton.Enabled = true;
            if (result.UsedEmbeddedMask && result.UsedMask is Mask usedMask)
            {
                ApplyMaskToChecks(usedMask);
                SaveUiState();
            }
            SetStatus(
                (result.OverwroteExisting ? "解码完成（已覆盖同名文件）：" : "解码完成：")
                + $"{result.FileName}（{ByteSize.Format(result.SizeBytes)}）→ {result.OutputPath}"
                + (result.UsedEmbeddedMask ? "（已使用图片内嵌掩码）" : "")
                + (result.WasEncrypted ? "（已使用密码解密）" : ""),
                false);
        }
        catch (StegoException ex)
        {
            InvalidateDecodeResult();
            SetStatus(ex.Message, true);
        }
        catch (Exception ex)
        {
            InvalidateDecodeResult();
            SetStatus($"解码失败：{ex.Message}", true);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnSaveDecodedClick(object? sender, EventArgs e)
    {
        if (_decodeResult is null || !File.Exists(_decodeResult.OutputPath))
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "所有文件|*.*",
            FileName = _decodeResult.FileName,
            InitialDirectory = Path.GetDirectoryName(_decodeResult.OutputPath),
            OverwritePrompt = true,
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            File.Copy(_decodeResult.OutputPath, dialog.FileName, overwrite: true);
            SetStatus($"已另存为：{dialog.FileName}", false);
        }
        catch (Exception ex)
        {
            SetStatus($"保存失败：{ex.Message}", true);
        }
    }

    private void OnOpenFolderClick(object? sender, EventArgs e)
    {
        if (_decodeResult is null || !File.Exists(_decodeResult.OutputPath))
        {
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{_decodeResult.OutputPath}\"")
            {
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            SetStatus($"打开目录失败：{ex.Message}", true);
        }
    }

    private void SetBusy(bool busy, string? status = null)
    {
        _busy = busy;
        _maskGroup.Enabled = !busy;
        _embedButton.Enabled = !busy;
        _extractButton.Enabled = !busy;
        _generateButton.Enabled = !busy;
        _ratioPanel.Enabled = !busy;
        _embedPasswordBox.Enabled = !busy;
        _decodePasswordBox.Enabled = !busy;
        _generatePasswordBox.Enabled = !busy;
        _saveEmbedButton.Enabled = !busy && _embedResult is not null;
        _saveDecodedButton.Enabled = !busy && _decodeResult is not null;
        _saveGeneratedButton.Enabled = !busy && _generateResult is not null;
        _openFolderButton.Enabled = !busy && _decodeResult is not null;
        UseWaitCursor = busy;
        if (status is not null)
        {
            SetStatus(status, false);
        }
    }

    private void SetStatus(string text, bool isError)
    {
        _statusLabel.ForeColor = isError ? Color.Firebrick : SystemColors.ControlText;
        _statusLabel.Text = text;
    }
}
