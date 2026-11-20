namespace PicHider.App;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    private System.Windows.Forms.ToolTip _toolTip = null!;
    private System.Windows.Forms.TableLayoutPanel _rootLayout = null!;
    private System.Windows.Forms.GroupBox _maskGroup = null!;
    private System.Windows.Forms.TableLayoutPanel _maskTable = null!;
    private System.Windows.Forms.Label _channelHeaderLabel = null!;
    private System.Windows.Forms.Label _rowHeaderLabel = null!;
    private System.Windows.Forms.Label _alphaChannelLabel = null!;
    private System.Windows.Forms.Label _redChannelLabel = null!;
    private System.Windows.Forms.Label _greenChannelLabel = null!;
    private System.Windows.Forms.Label _blueChannelLabel = null!;
    private System.Windows.Forms.TabControl _tabs = null!;
    private System.Windows.Forms.TabPage _embedPage = null!;
    private System.Windows.Forms.TabPage _decodePage = null!;

    private System.Windows.Forms.TableLayoutPanel _embedLayout = null!;
    private System.Windows.Forms.Label _coverLabel = null!;
    private System.Windows.Forms.TextBox _coverPathBox = null!;
    private System.Windows.Forms.Button _browseCoverButton = null!;
    private System.Windows.Forms.Label _dataLabel = null!;
    private System.Windows.Forms.TextBox _dataPathBox = null!;
    private System.Windows.Forms.Button _browseDataButton = null!;
    private System.Windows.Forms.Label _embedPasswordLabel = null!;
    private System.Windows.Forms.TextBox _embedPasswordBox = null!;
    private System.Windows.Forms.FlowLayoutPanel _embedActions = null!;
    private System.Windows.Forms.CheckBox _viewOriginalCheck = null!;
    private System.Windows.Forms.Button _embedButton = null!;
    private System.Windows.Forms.Button _saveEmbedButton = null!;
    private System.Windows.Forms.PictureBox _embedPicture = null!;

    private System.Windows.Forms.TableLayoutPanel _decodeLayout = null!;
    private System.Windows.Forms.Label _stegoLabel = null!;
    private System.Windows.Forms.TextBox _stegoPathBox = null!;
    private System.Windows.Forms.Button _browseStegoButton = null!;
    private System.Windows.Forms.Label _decodePasswordLabel = null!;
    private System.Windows.Forms.TextBox _decodePasswordBox = null!;
    private System.Windows.Forms.FlowLayoutPanel _decodeActions = null!;
    private System.Windows.Forms.Button _extractButton = null!;
    private System.Windows.Forms.Button _saveDecodedButton = null!;
    private System.Windows.Forms.Button _openFolderButton = null!;
    private System.Windows.Forms.Label _decodedLabel = null!;
    private System.Windows.Forms.TextBox _decodedPathBox = null!;
    private System.Windows.Forms.PictureBox _decodePicture = null!;
    private System.Windows.Forms.TabPage _generatePage = null!;
    private System.Windows.Forms.TableLayoutPanel _generateLayout = null!;
    private System.Windows.Forms.Label _generateDataLabel = null!;
    private System.Windows.Forms.TextBox _generateDataPathBox = null!;
    private System.Windows.Forms.Button _generateBrowseButton = null!;
    private System.Windows.Forms.Label _ratioLabel = null!;
    private System.Windows.Forms.FlowLayoutPanel _ratioPanel = null!;
    private System.Windows.Forms.RadioButton _ratioRadio0 = null!;
    private System.Windows.Forms.RadioButton _ratioRadio1 = null!;
    private System.Windows.Forms.RadioButton _ratioRadio2 = null!;
    private System.Windows.Forms.RadioButton _ratioRadio3 = null!;
    private System.Windows.Forms.RadioButton _ratioRadio4 = null!;
    private System.Windows.Forms.RadioButton _ratioRadio5 = null!;
    private System.Windows.Forms.RadioButton _ratioRadio6 = null!;
    private System.Windows.Forms.RadioButton _ratioRadio7 = null!;
    private System.Windows.Forms.RadioButton _ratioRadio8 = null!;
    private System.Windows.Forms.RadioButton _customRatioRadio = null!;
    private System.Windows.Forms.TextBox _customRatioBox = null!;
    private System.Windows.Forms.Label _generatePasswordLabel = null!;
    private System.Windows.Forms.TextBox _generatePasswordBox = null!;
    private System.Windows.Forms.FlowLayoutPanel _generateActions = null!;
    private System.Windows.Forms.Button _generateButton = null!;
    private System.Windows.Forms.Button _saveGeneratedButton = null!;
    private System.Windows.Forms.PictureBox _generatePicture = null!;

    private System.Windows.Forms.Label _statusLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _toolTip = new ToolTip(components);
        _ratioPanel = new FlowLayoutPanel();
        _ratioRadio0 = new RadioButton();
        _ratioRadio1 = new RadioButton();
        _ratioRadio2 = new RadioButton();
        _ratioRadio3 = new RadioButton();
        _ratioRadio4 = new RadioButton();
        _ratioRadio5 = new RadioButton();
        _ratioRadio6 = new RadioButton();
        _ratioRadio7 = new RadioButton();
        _ratioRadio8 = new RadioButton();
        _customRatioRadio = new RadioButton();
        _customRatioBox = new TextBox();
        _rootLayout = new TableLayoutPanel();
        _maskGroup = new GroupBox();
        _maskTable = new TableLayoutPanel();
        _channelHeaderLabel = new Label();
        _rowHeaderLabel = new Label();
        _alphaChannelLabel = new Label();
        _redChannelLabel = new Label();
        _greenChannelLabel = new Label();
        _blueChannelLabel = new Label();
        _tabs = new TabControl();
        _embedPage = new TabPage();
        _embedLayout = new TableLayoutPanel();
        _dataLabel = new Label();
        _dataPathBox = new TextBox();
        _browseDataButton = new Button();
        _coverLabel = new Label();
        _coverPathBox = new TextBox();
        _browseCoverButton = new Button();
        _embedPasswordLabel = new Label();
        _embedPasswordBox = new TextBox();
        _embedActions = new FlowLayoutPanel();
        _viewOriginalCheck = new CheckBox();
        _embedButton = new Button();
        _saveEmbedButton = new Button();
        _embedPicture = new PictureBox();
        _generatePage = new TabPage();
        _generateLayout = new TableLayoutPanel();
        _generateDataLabel = new Label();
        _generateDataPathBox = new TextBox();
        _generateBrowseButton = new Button();
        _ratioLabel = new Label();
        _generatePasswordLabel = new Label();
        _generatePasswordBox = new TextBox();
        _generateActions = new FlowLayoutPanel();
        _generateButton = new Button();
        _saveGeneratedButton = new Button();
        _generatePicture = new PictureBox();
        _decodePage = new TabPage();
        _decodeLayout = new TableLayoutPanel();
        _stegoLabel = new Label();
        _stegoPathBox = new TextBox();
        _browseStegoButton = new Button();
        _decodePasswordLabel = new Label();
        _decodePasswordBox = new TextBox();
        _decodeActions = new FlowLayoutPanel();
        _extractButton = new Button();
        _openFolderButton = new Button();
        _saveDecodedButton = new Button();
        _decodedLabel = new Label();
        _decodedPathBox = new TextBox();
        _decodePicture = new PictureBox();
        _statusLabel = new Label();
        _ratioPanel.SuspendLayout();
        _rootLayout.SuspendLayout();
        _maskGroup.SuspendLayout();
        _maskTable.SuspendLayout();
        _tabs.SuspendLayout();
        _embedPage.SuspendLayout();
        _embedLayout.SuspendLayout();
        _embedActions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_embedPicture).BeginInit();
        _generatePage.SuspendLayout();
        _generateLayout.SuspendLayout();
        _generateActions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_generatePicture).BeginInit();
        _decodePage.SuspendLayout();
        _decodeLayout.SuspendLayout();
        _decodeActions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_decodePicture).BeginInit();
        SuspendLayout();
        // 
        // _ratioPanel
        // 
        _generateLayout.SetColumnSpan(_ratioPanel, 2);
        _ratioPanel.Controls.Add(_ratioRadio0);
        _ratioPanel.Controls.Add(_ratioRadio1);
        _ratioPanel.Controls.Add(_ratioRadio2);
        _ratioPanel.Controls.Add(_ratioRadio3);
        _ratioPanel.Controls.Add(_ratioRadio4);
        _ratioPanel.Controls.Add(_ratioRadio5);
        _ratioPanel.Controls.Add(_ratioRadio6);
        _ratioPanel.Controls.Add(_ratioRadio7);
        _ratioPanel.Controls.Add(_ratioRadio8);
        _ratioPanel.Controls.Add(_customRatioRadio);
        _ratioPanel.Controls.Add(_customRatioBox);
        _ratioPanel.Dock = DockStyle.Fill;
        _ratioPanel.Location = new Point(99, 44);
        _ratioPanel.Margin = new Padding(3, 6, 3, 6);
        _ratioPanel.Name = "_ratioPanel";
        _ratioPanel.Padding = new Padding(0, 2, 0, 0);
        _ratioPanel.Size = new Size(940, 26);
        _ratioPanel.TabIndex = 4;
        _toolTip.SetToolTip(_ratioPanel, "单选常见比例；选择“自定义”后可输入如 5:4、1920x1080");
        // 
        // _ratioRadio0
        // 
        _ratioRadio0.AutoSize = true;
        _ratioRadio0.Location = new Point(0, 2);
        _ratioRadio0.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio0.Name = "_ratioRadio0";
        _ratioRadio0.Size = new Size(43, 21);
        _ratioRadio0.TabIndex = 0;
        _ratioRadio0.Text = "1:1";
        _ratioRadio0.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _ratioRadio1
        // 
        _ratioRadio1.AutoSize = true;
        _ratioRadio1.Location = new Point(51, 2);
        _ratioRadio1.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio1.Name = "_ratioRadio1";
        _ratioRadio1.Size = new Size(43, 21);
        _ratioRadio1.TabIndex = 1;
        _ratioRadio1.Text = "4:3";
        _ratioRadio1.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _ratioRadio2
        // 
        _ratioRadio2.AutoSize = true;
        _ratioRadio2.Location = new Point(102, 2);
        _ratioRadio2.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio2.Name = "_ratioRadio2";
        _ratioRadio2.Size = new Size(43, 21);
        _ratioRadio2.TabIndex = 2;
        _ratioRadio2.Text = "3:2";
        _ratioRadio2.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _ratioRadio3
        // 
        _ratioRadio3.AutoSize = true;
        _ratioRadio3.Checked = true;
        _ratioRadio3.Location = new Point(153, 2);
        _ratioRadio3.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio3.Name = "_ratioRadio3";
        _ratioRadio3.Size = new Size(50, 21);
        _ratioRadio3.TabIndex = 3;
        _ratioRadio3.TabStop = true;
        _ratioRadio3.Text = "16:9";
        _ratioRadio3.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _ratioRadio4
        // 
        _ratioRadio4.AutoSize = true;
        _ratioRadio4.Location = new Point(211, 2);
        _ratioRadio4.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio4.Name = "_ratioRadio4";
        _ratioRadio4.Size = new Size(50, 21);
        _ratioRadio4.TabIndex = 4;
        _ratioRadio4.Text = "9:16";
        _ratioRadio4.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _ratioRadio5
        // 
        _ratioRadio5.AutoSize = true;
        _ratioRadio5.Location = new Point(269, 2);
        _ratioRadio5.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio5.Name = "_ratioRadio5";
        _ratioRadio5.Size = new Size(43, 21);
        _ratioRadio5.TabIndex = 5;
        _ratioRadio5.Text = "3:4";
        _ratioRadio5.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _ratioRadio6
        // 
        _ratioRadio6.AutoSize = true;
        _ratioRadio6.Location = new Point(320, 2);
        _ratioRadio6.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio6.Name = "_ratioRadio6";
        _ratioRadio6.Size = new Size(43, 21);
        _ratioRadio6.TabIndex = 6;
        _ratioRadio6.Text = "2:3";
        _ratioRadio6.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _ratioRadio7
        // 
        _ratioRadio7.AutoSize = true;
        _ratioRadio7.Location = new Point(371, 2);
        _ratioRadio7.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio7.Name = "_ratioRadio7";
        _ratioRadio7.Size = new Size(43, 21);
        _ratioRadio7.TabIndex = 7;
        _ratioRadio7.Text = "5:4";
        _ratioRadio7.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _ratioRadio8
        // 
        _ratioRadio8.AutoSize = true;
        _ratioRadio8.Location = new Point(422, 2);
        _ratioRadio8.Margin = new Padding(0, 0, 8, 0);
        _ratioRadio8.Name = "_ratioRadio8";
        _ratioRadio8.Size = new Size(50, 21);
        _ratioRadio8.TabIndex = 8;
        _ratioRadio8.Text = "21:9";
        _ratioRadio8.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _customRatioRadio
        // 
        _customRatioRadio.AutoSize = true;
        _customRatioRadio.Location = new Point(486, 2);
        _customRatioRadio.Margin = new Padding(6, 0, 6, 0);
        _customRatioRadio.Name = "_customRatioRadio";
        _customRatioRadio.Size = new Size(62, 21);
        _customRatioRadio.TabIndex = 9;
        _customRatioRadio.Text = "自定义";
        _customRatioRadio.CheckedChanged += OnRatioCheckedChanged;
        // 
        // _customRatioBox
        // 
        _customRatioBox.Enabled = false;
        _customRatioBox.Location = new Point(554, 2);
        _customRatioBox.Margin = new Padding(0);
        _customRatioBox.Name = "_customRatioBox";
        _customRatioBox.PlaceholderText = "如 5:4";
        _customRatioBox.Size = new Size(90, 23);
        _customRatioBox.TabIndex = 10;
        _customRatioBox.TextChanged += OnGenerateInputChanged;
        // 
        // _rootLayout
        // 
        _rootLayout.ColumnCount = 1;
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(_maskGroup, 0, 0);
        _rootLayout.Controls.Add(_tabs, 0, 1);
        _rootLayout.Controls.Add(_statusLabel, 0, 2);
        _rootLayout.Dock = DockStyle.Fill;
        _rootLayout.Location = new Point(0, 0);
        _rootLayout.Name = "_rootLayout";
        _rootLayout.Padding = new Padding(8);
        _rootLayout.RowCount = 3;
        _rootLayout.RowStyles.Add(new RowStyle());
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        _rootLayout.Size = new Size(1084, 781);
        _rootLayout.TabIndex = 0;
        // 
        // _maskGroup
        // 
        _maskGroup.AutoSize = true;
        _maskGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _maskGroup.Controls.Add(_maskTable);
        _maskGroup.Location = new Point(8, 8);
        _maskGroup.Margin = new Padding(0, 0, 0, 6);
        _maskGroup.Name = "_maskGroup";
        _maskGroup.Padding = new Padding(8);
        _maskGroup.Size = new Size(576, 187);
        _maskGroup.TabIndex = 0;
        _maskGroup.TabStop = false;
        _maskGroup.Text = "掩码配置（勾选的 bit 用于隐藏数据，顺序 Alpha / Red / Green / Blue）";
        // 
        // _maskTable
        // 
        _maskTable.AutoSize = true;
        _maskTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _maskTable.ColumnCount = 10;
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
        _maskTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        _maskTable.Controls.Add(_channelHeaderLabel, 0, 0);
        _maskTable.Controls.Add(_rowHeaderLabel, 9, 0);
        _maskTable.Controls.Add(_alphaChannelLabel, 0, 1);
        _maskTable.Controls.Add(_redChannelLabel, 0, 2);
        _maskTable.Controls.Add(_greenChannelLabel, 0, 3);
        _maskTable.Controls.Add(_blueChannelLabel, 0, 4);
        _maskTable.Dock = DockStyle.Fill;
        _maskTable.Location = new Point(8, 24);
        _maskTable.Name = "_maskTable";
        _maskTable.RowCount = 5;
        _maskTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        _maskTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        _maskTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        _maskTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        _maskTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        _maskTable.Size = new Size(560, 155);
        _maskTable.TabIndex = 0;
        // 
        // _channelHeaderLabel
        // 
        _channelHeaderLabel.Dock = DockStyle.Fill;
        _channelHeaderLabel.Location = new Point(3, 0);
        _channelHeaderLabel.Name = "_channelHeaderLabel";
        _channelHeaderLabel.Size = new Size(58, 31);
        _channelHeaderLabel.TabIndex = 0;
        _channelHeaderLabel.Text = "通道";
        _channelHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _rowHeaderLabel
        // 
        _rowHeaderLabel.Dock = DockStyle.Fill;
        _rowHeaderLabel.Location = new Point(467, 0);
        _rowHeaderLabel.Name = "_rowHeaderLabel";
        _rowHeaderLabel.Padding = new Padding(6, 0, 0, 0);
        _rowHeaderLabel.Size = new Size(90, 31);
        _rowHeaderLabel.TabIndex = 1;
        _rowHeaderLabel.Text = "整行";
        _rowHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _alphaChannelLabel
        // 
        _alphaChannelLabel.Dock = DockStyle.Fill;
        _alphaChannelLabel.Location = new Point(3, 31);
        _alphaChannelLabel.Name = "_alphaChannelLabel";
        _alphaChannelLabel.Size = new Size(58, 31);
        _alphaChannelLabel.TabIndex = 2;
        _alphaChannelLabel.Text = "Alpha";
        _alphaChannelLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _redChannelLabel
        // 
        _redChannelLabel.Dock = DockStyle.Fill;
        _redChannelLabel.Location = new Point(3, 62);
        _redChannelLabel.Name = "_redChannelLabel";
        _redChannelLabel.Size = new Size(58, 31);
        _redChannelLabel.TabIndex = 3;
        _redChannelLabel.Text = "Red";
        _redChannelLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _greenChannelLabel
        // 
        _greenChannelLabel.Dock = DockStyle.Fill;
        _greenChannelLabel.Location = new Point(3, 93);
        _greenChannelLabel.Name = "_greenChannelLabel";
        _greenChannelLabel.Size = new Size(58, 31);
        _greenChannelLabel.TabIndex = 4;
        _greenChannelLabel.Text = "Green";
        _greenChannelLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _blueChannelLabel
        // 
        _blueChannelLabel.Dock = DockStyle.Fill;
        _blueChannelLabel.Location = new Point(3, 124);
        _blueChannelLabel.Name = "_blueChannelLabel";
        _blueChannelLabel.Size = new Size(58, 31);
        _blueChannelLabel.TabIndex = 5;
        _blueChannelLabel.Text = "Blue";
        _blueChannelLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _tabs
        // 
        _tabs.Controls.Add(_embedPage);
        _tabs.Controls.Add(_generatePage);
        _tabs.Controls.Add(_decodePage);
        _tabs.Dock = DockStyle.Fill;
        _tabs.Location = new Point(11, 204);
        _tabs.Name = "_tabs";
        _tabs.Padding = new Point(14, 6);
        _tabs.SelectedIndex = 0;
        _tabs.Size = new Size(1062, 538);
        _tabs.TabIndex = 1;
        // 
        // _embedPage
        // 
        _embedPage.Controls.Add(_embedLayout);
        _embedPage.Location = new Point(4, 32);
        _embedPage.Name = "_embedPage";
        _embedPage.Padding = new Padding(6);
        _embedPage.Size = new Size(1054, 502);
        _embedPage.TabIndex = 0;
        _embedPage.Text = "嵌入";
        // 
        // _embedLayout
        // 
        _embedLayout.ColumnCount = 3;
        _embedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        _embedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _embedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        _embedLayout.Controls.Add(_dataLabel, 0, 0);
        _embedLayout.Controls.Add(_dataPathBox, 1, 0);
        _embedLayout.Controls.Add(_browseDataButton, 2, 0);
        _embedLayout.Controls.Add(_coverLabel, 0, 1);
        _embedLayout.Controls.Add(_coverPathBox, 1, 1);
        _embedLayout.Controls.Add(_browseCoverButton, 2, 1);
        _embedLayout.Controls.Add(_embedPasswordLabel, 0, 2);
        _embedLayout.Controls.Add(_embedPasswordBox, 1, 2);
        _embedLayout.Controls.Add(_embedActions, 0, 3);
        _embedLayout.Controls.Add(_embedPicture, 0, 4);
        _embedLayout.Dock = DockStyle.Fill;
        _embedLayout.Location = new Point(6, 6);
        _embedLayout.Name = "_embedLayout";
        _embedLayout.RowCount = 5;
        _embedLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _embedLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _embedLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _embedLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        _embedLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _embedLayout.Size = new Size(1042, 490);
        _embedLayout.TabIndex = 0;
        // 
        // _dataLabel
        // 
        _dataLabel.Dock = DockStyle.Fill;
        _dataLabel.Location = new Point(3, 0);
        _dataLabel.Name = "_dataLabel";
        _dataLabel.Size = new Size(90, 38);
        _dataLabel.TabIndex = 0;
        _dataLabel.Text = "待隐藏文件";
        _dataLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _dataPathBox
        // 
        _dataPathBox.Dock = DockStyle.Fill;
        _dataPathBox.Location = new Point(99, 6);
        _dataPathBox.Margin = new Padding(3, 6, 3, 6);
        _dataPathBox.Name = "_dataPathBox";
        _dataPathBox.Size = new Size(852, 23);
        _dataPathBox.TabIndex = 1;
        _dataPathBox.TextChanged += OnDataPathChanged;
        // 
        // _browseDataButton
        // 
        _browseDataButton.Dock = DockStyle.Fill;
        _browseDataButton.Location = new Point(957, 4);
        _browseDataButton.Margin = new Padding(3, 4, 0, 4);
        _browseDataButton.Name = "_browseDataButton";
        _browseDataButton.Size = new Size(85, 30);
        _browseDataButton.TabIndex = 2;
        _browseDataButton.Text = "选择...";
        _browseDataButton.Click += OnBrowseDataClick;
        // 
        // _coverLabel
        // 
        _coverLabel.Dock = DockStyle.Fill;
        _coverLabel.Location = new Point(3, 38);
        _coverLabel.Name = "_coverLabel";
        _coverLabel.Size = new Size(90, 38);
        _coverLabel.TabIndex = 3;
        _coverLabel.Text = "载体图片";
        _coverLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _coverPathBox
        // 
        _coverPathBox.Dock = DockStyle.Fill;
        _coverPathBox.Location = new Point(99, 44);
        _coverPathBox.Margin = new Padding(3, 6, 3, 6);
        _coverPathBox.Name = "_coverPathBox";
        _coverPathBox.Size = new Size(852, 23);
        _coverPathBox.TabIndex = 4;
        _coverPathBox.TextChanged += OnCoverPathChanged;
        _coverPathBox.KeyDown += OnPathKeyDown;
        _coverPathBox.Leave += OnCoverPathLeave;
        // 
        // _browseCoverButton
        // 
        _browseCoverButton.Dock = DockStyle.Fill;
        _browseCoverButton.Location = new Point(957, 42);
        _browseCoverButton.Margin = new Padding(3, 4, 0, 4);
        _browseCoverButton.Name = "_browseCoverButton";
        _browseCoverButton.Size = new Size(85, 30);
        _browseCoverButton.TabIndex = 5;
        _browseCoverButton.Text = "选择...";
        _browseCoverButton.Click += OnBrowseCoverClick;
        // 
        // _embedPasswordLabel
        // 
        _embedPasswordLabel.Dock = DockStyle.Fill;
        _embedPasswordLabel.Location = new Point(3, 76);
        _embedPasswordLabel.Name = "_embedPasswordLabel";
        _embedPasswordLabel.Size = new Size(90, 38);
        _embedPasswordLabel.TabIndex = 6;
        _embedPasswordLabel.Text = "密码";
        _embedPasswordLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _embedPasswordBox
        // 
        _embedLayout.SetColumnSpan(_embedPasswordBox, 2);
        _embedPasswordBox.Dock = DockStyle.Fill;
        _embedPasswordBox.Location = new Point(99, 82);
        _embedPasswordBox.Margin = new Padding(3, 6, 3, 6);
        _embedPasswordBox.Name = "_embedPasswordBox";
        _embedPasswordBox.PlaceholderText = "留空则不加密";
        _embedPasswordBox.Size = new Size(940, 23);
        _embedPasswordBox.TabIndex = 7;
        _embedPasswordBox.TextChanged += OnEmbedPasswordChanged;
        // 
        // _embedActions
        // 
        _embedLayout.SetColumnSpan(_embedActions, 3);
        _embedActions.Controls.Add(_viewOriginalCheck);
        _embedActions.Controls.Add(_embedButton);
        _embedActions.Controls.Add(_saveEmbedButton);
        _embedActions.Dock = DockStyle.Fill;
        _embedActions.Location = new Point(3, 117);
        _embedActions.Name = "_embedActions";
        _embedActions.Padding = new Padding(0, 6, 0, 0);
        _embedActions.Size = new Size(1036, 40);
        _embedActions.TabIndex = 8;
        _embedActions.WrapContents = false;
        // 
        // _viewOriginalCheck
        // 
        _viewOriginalCheck.AutoSize = true;
        _viewOriginalCheck.Location = new Point(0, 10);
        _viewOriginalCheck.Margin = new Padding(0, 4, 16, 0);
        _viewOriginalCheck.Name = "_viewOriginalCheck";
        _viewOriginalCheck.Size = new Size(75, 21);
        _viewOriginalCheck.TabIndex = 0;
        _viewOriginalCheck.Text = "查看原图";
        _viewOriginalCheck.CheckedChanged += OnViewOriginalChanged;
        // 
        // _embedButton
        // 
        _embedButton.Location = new Point(91, 6);
        _embedButton.Margin = new Padding(0, 0, 12, 0);
        _embedButton.Name = "_embedButton";
        _embedButton.Size = new Size(96, 30);
        _embedButton.TabIndex = 1;
        _embedButton.Text = "嵌入";
        _embedButton.Click += OnEmbedClick;
        // 
        // _saveEmbedButton
        // 
        _saveEmbedButton.Enabled = false;
        _saveEmbedButton.Location = new Point(202, 9);
        _saveEmbedButton.Name = "_saveEmbedButton";
        _saveEmbedButton.Size = new Size(96, 30);
        _saveEmbedButton.TabIndex = 2;
        _saveEmbedButton.Text = "另存为";
        _saveEmbedButton.Click += OnSaveEmbedClick;
        // 
        // _embedPicture
        // 
        _embedPicture.BackColor = Color.FromArgb(32, 32, 32);
        _embedPicture.BorderStyle = BorderStyle.FixedSingle;
        _embedLayout.SetColumnSpan(_embedPicture, 3);
        _embedPicture.Dock = DockStyle.Fill;
        _embedPicture.Location = new Point(0, 164);
        _embedPicture.Margin = new Padding(0, 4, 0, 0);
        _embedPicture.Name = "_embedPicture";
        _embedPicture.Size = new Size(1042, 326);
        _embedPicture.SizeMode = PictureBoxSizeMode.Zoom;
        _embedPicture.TabIndex = 9;
        _embedPicture.TabStop = false;
        // 
        // _generatePage
        // 
        _generatePage.Controls.Add(_generateLayout);
        _generatePage.Location = new Point(4, 32);
        _generatePage.Name = "_generatePage";
        _generatePage.Padding = new Padding(6);
        _generatePage.Size = new Size(1054, 502);
        _generatePage.TabIndex = 1;
        _generatePage.Text = "免图嵌入";
        // 
        // _generateLayout
        // 
        _generateLayout.ColumnCount = 3;
        _generateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        _generateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _generateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        _generateLayout.Controls.Add(_generateDataLabel, 0, 0);
        _generateLayout.Controls.Add(_generateDataPathBox, 1, 0);
        _generateLayout.Controls.Add(_generateBrowseButton, 2, 0);
        _generateLayout.Controls.Add(_ratioLabel, 0, 1);
        _generateLayout.Controls.Add(_ratioPanel, 1, 1);
        _generateLayout.Controls.Add(_generatePasswordLabel, 0, 2);
        _generateLayout.Controls.Add(_generatePasswordBox, 1, 2);
        _generateLayout.Controls.Add(_generateActions, 0, 3);
        _generateLayout.Controls.Add(_generatePicture, 0, 4);
        _generateLayout.Dock = DockStyle.Fill;
        _generateLayout.Location = new Point(6, 6);
        _generateLayout.Name = "_generateLayout";
        _generateLayout.RowCount = 5;
        _generateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _generateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _generateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _generateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        _generateLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _generateLayout.Size = new Size(1042, 490);
        _generateLayout.TabIndex = 0;
        // 
        // _generateDataLabel
        // 
        _generateDataLabel.Dock = DockStyle.Fill;
        _generateDataLabel.Location = new Point(3, 0);
        _generateDataLabel.Name = "_generateDataLabel";
        _generateDataLabel.Size = new Size(90, 38);
        _generateDataLabel.TabIndex = 0;
        _generateDataLabel.Text = "待隐藏文件";
        _generateDataLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _generateDataPathBox
        // 
        _generateDataPathBox.Dock = DockStyle.Fill;
        _generateDataPathBox.Location = new Point(99, 6);
        _generateDataPathBox.Margin = new Padding(3, 6, 3, 6);
        _generateDataPathBox.Name = "_generateDataPathBox";
        _generateDataPathBox.Size = new Size(852, 23);
        _generateDataPathBox.TabIndex = 1;
        _generateDataPathBox.TextChanged += OnGenerateInputChanged;
        _generateDataPathBox.Leave += OnGenerateDataPathLeave;
        // 
        // _generateBrowseButton
        // 
        _generateBrowseButton.Dock = DockStyle.Fill;
        _generateBrowseButton.Location = new Point(957, 4);
        _generateBrowseButton.Margin = new Padding(3, 4, 0, 4);
        _generateBrowseButton.Name = "_generateBrowseButton";
        _generateBrowseButton.Size = new Size(85, 30);
        _generateBrowseButton.TabIndex = 2;
        _generateBrowseButton.Text = "选择...";
        _generateBrowseButton.Click += OnBrowseGenerateDataClick;
        // 
        // _ratioLabel
        // 
        _ratioLabel.Dock = DockStyle.Fill;
        _ratioLabel.Location = new Point(3, 38);
        _ratioLabel.Name = "_ratioLabel";
        _ratioLabel.Size = new Size(90, 38);
        _ratioLabel.TabIndex = 3;
        _ratioLabel.Text = "图片比例";
        _ratioLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _generatePasswordLabel
        // 
        _generatePasswordLabel.Dock = DockStyle.Fill;
        _generatePasswordLabel.Location = new Point(3, 76);
        _generatePasswordLabel.Name = "_generatePasswordLabel";
        _generatePasswordLabel.Size = new Size(90, 38);
        _generatePasswordLabel.TabIndex = 5;
        _generatePasswordLabel.Text = "密码";
        _generatePasswordLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _generatePasswordBox
        // 
        _generateLayout.SetColumnSpan(_generatePasswordBox, 2);
        _generatePasswordBox.Dock = DockStyle.Fill;
        _generatePasswordBox.Location = new Point(99, 82);
        _generatePasswordBox.Margin = new Padding(3, 6, 3, 6);
        _generatePasswordBox.Name = "_generatePasswordBox";
        _generatePasswordBox.PlaceholderText = "留空则不加密";
        _generatePasswordBox.Size = new Size(940, 23);
        _generatePasswordBox.TabIndex = 6;
        _generatePasswordBox.TextChanged += OnGenerateInputChanged;
        // 
        // _generateActions
        // 
        _generateLayout.SetColumnSpan(_generateActions, 3);
        _generateActions.Controls.Add(_generateButton);
        _generateActions.Controls.Add(_saveGeneratedButton);
        _generateActions.Dock = DockStyle.Fill;
        _generateActions.Location = new Point(3, 117);
        _generateActions.Name = "_generateActions";
        _generateActions.Padding = new Padding(0, 6, 0, 0);
        _generateActions.Size = new Size(1036, 40);
        _generateActions.TabIndex = 7;
        _generateActions.WrapContents = false;
        // 
        // _generateButton
        // 
        _generateButton.Location = new Point(0, 6);
        _generateButton.Margin = new Padding(0, 0, 12, 0);
        _generateButton.Name = "_generateButton";
        _generateButton.Size = new Size(110, 30);
        _generateButton.TabIndex = 0;
        _generateButton.Text = "生成并嵌入";
        _generateButton.Click += OnGenerateClick;
        // 
        // _saveGeneratedButton
        // 
        _saveGeneratedButton.Enabled = false;
        _saveGeneratedButton.Location = new Point(122, 6);
        _saveGeneratedButton.Margin = new Padding(0);
        _saveGeneratedButton.Name = "_saveGeneratedButton";
        _saveGeneratedButton.Size = new Size(96, 30);
        _saveGeneratedButton.TabIndex = 1;
        _saveGeneratedButton.Text = "另存为";
        _saveGeneratedButton.Click += OnSaveGeneratedClick;
        // 
        // _generatePicture
        // 
        _generatePicture.BackColor = Color.FromArgb(32, 32, 32);
        _generatePicture.BorderStyle = BorderStyle.FixedSingle;
        _generateLayout.SetColumnSpan(_generatePicture, 3);
        _generatePicture.Dock = DockStyle.Fill;
        _generatePicture.Location = new Point(0, 164);
        _generatePicture.Margin = new Padding(0, 4, 0, 0);
        _generatePicture.Name = "_generatePicture";
        _generatePicture.Size = new Size(1042, 326);
        _generatePicture.SizeMode = PictureBoxSizeMode.Zoom;
        _generatePicture.TabIndex = 8;
        _generatePicture.TabStop = false;
        // 
        // _decodePage
        // 
        _decodePage.Controls.Add(_decodeLayout);
        _decodePage.Location = new Point(4, 32);
        _decodePage.Name = "_decodePage";
        _decodePage.Padding = new Padding(6);
        _decodePage.Size = new Size(1054, 502);
        _decodePage.TabIndex = 2;
        _decodePage.Text = "提取";
        // 
        // _decodeLayout
        // 
        _decodeLayout.ColumnCount = 3;
        _decodeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        _decodeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _decodeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
        _decodeLayout.Controls.Add(_stegoLabel, 0, 0);
        _decodeLayout.Controls.Add(_stegoPathBox, 1, 0);
        _decodeLayout.Controls.Add(_browseStegoButton, 2, 0);
        _decodeLayout.Controls.Add(_decodePasswordLabel, 0, 1);
        _decodeLayout.Controls.Add(_decodePasswordBox, 1, 1);
        _decodeLayout.Controls.Add(_decodeActions, 0, 2);
        _decodeLayout.Controls.Add(_decodedLabel, 0, 3);
        _decodeLayout.Controls.Add(_decodedPathBox, 1, 3);
        _decodeLayout.Controls.Add(_decodePicture, 0, 4);
        _decodeLayout.Dock = DockStyle.Fill;
        _decodeLayout.Location = new Point(6, 6);
        _decodeLayout.Name = "_decodeLayout";
        _decodeLayout.RowCount = 5;
        _decodeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _decodeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _decodeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        _decodeLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        _decodeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _decodeLayout.Size = new Size(1042, 490);
        _decodeLayout.TabIndex = 0;
        // 
        // _stegoLabel
        // 
        _stegoLabel.Dock = DockStyle.Fill;
        _stegoLabel.Location = new Point(3, 0);
        _stegoLabel.Name = "_stegoLabel";
        _stegoLabel.Size = new Size(90, 38);
        _stegoLabel.TabIndex = 0;
        _stegoLabel.Text = "隐写图片";
        _stegoLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _stegoPathBox
        // 
        _stegoPathBox.Dock = DockStyle.Fill;
        _stegoPathBox.Location = new Point(99, 6);
        _stegoPathBox.Margin = new Padding(3, 6, 3, 6);
        _stegoPathBox.Name = "_stegoPathBox";
        _stegoPathBox.Size = new Size(852, 23);
        _stegoPathBox.TabIndex = 1;
        _stegoPathBox.TextChanged += OnStegoPathChanged;
        _stegoPathBox.KeyDown += OnPathKeyDown;
        _stegoPathBox.Leave += OnStegoPathLeave;
        // 
        // _browseStegoButton
        // 
        _browseStegoButton.Dock = DockStyle.Fill;
        _browseStegoButton.Location = new Point(957, 4);
        _browseStegoButton.Margin = new Padding(3, 4, 0, 4);
        _browseStegoButton.Name = "_browseStegoButton";
        _browseStegoButton.Size = new Size(85, 30);
        _browseStegoButton.TabIndex = 2;
        _browseStegoButton.Text = "选择...";
        _browseStegoButton.Click += OnBrowseStegoClick;
        // 
        // _decodePasswordLabel
        // 
        _decodePasswordLabel.Dock = DockStyle.Fill;
        _decodePasswordLabel.Location = new Point(3, 38);
        _decodePasswordLabel.Name = "_decodePasswordLabel";
        _decodePasswordLabel.Size = new Size(90, 38);
        _decodePasswordLabel.TabIndex = 3;
        _decodePasswordLabel.Text = "密码";
        _decodePasswordLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _decodePasswordBox
        // 
        _decodeLayout.SetColumnSpan(_decodePasswordBox, 2);
        _decodePasswordBox.Dock = DockStyle.Fill;
        _decodePasswordBox.Location = new Point(99, 44);
        _decodePasswordBox.Margin = new Padding(3, 6, 3, 6);
        _decodePasswordBox.Name = "_decodePasswordBox";
        _decodePasswordBox.PlaceholderText = "图片加密时请输入密码";
        _decodePasswordBox.Size = new Size(940, 23);
        _decodePasswordBox.TabIndex = 4;
        _decodePasswordBox.TextChanged += OnDecodePasswordChanged;
        // 
        // _decodeActions
        // 
        _decodeLayout.SetColumnSpan(_decodeActions, 3);
        _decodeActions.Controls.Add(_extractButton);
        _decodeActions.Controls.Add(_openFolderButton);
        _decodeActions.Controls.Add(_saveDecodedButton);
        _decodeActions.Dock = DockStyle.Fill;
        _decodeActions.Location = new Point(3, 79);
        _decodeActions.Name = "_decodeActions";
        _decodeActions.Padding = new Padding(0, 6, 0, 0);
        _decodeActions.Size = new Size(1036, 40);
        _decodeActions.TabIndex = 5;
        _decodeActions.WrapContents = false;
        // 
        // _extractButton
        // 
        _extractButton.Location = new Point(0, 6);
        _extractButton.Margin = new Padding(0, 0, 12, 0);
        _extractButton.Name = "_extractButton";
        _extractButton.Size = new Size(96, 30);
        _extractButton.TabIndex = 0;
        _extractButton.Text = "提取";
        _extractButton.Click += OnExtractClick;
        // 
        // _openFolderButton
        // 
        _openFolderButton.Enabled = false;
        _openFolderButton.Location = new Point(108, 6);
        _openFolderButton.Margin = new Padding(0, 0, 12, 0);
        _openFolderButton.Name = "_openFolderButton";
        _openFolderButton.Size = new Size(96, 30);
        _openFolderButton.TabIndex = 1;
        _openFolderButton.Text = "打开目录";
        _openFolderButton.Click += OnOpenFolderClick;
        // 
        // _saveDecodedButton
        // 
        _saveDecodedButton.Enabled = false;
        _saveDecodedButton.Location = new Point(216, 6);
        _saveDecodedButton.Margin = new Padding(0);
        _saveDecodedButton.Name = "_saveDecodedButton";
        _saveDecodedButton.Size = new Size(96, 30);
        _saveDecodedButton.TabIndex = 2;
        _saveDecodedButton.Text = "另存为";
        _saveDecodedButton.Click += OnSaveDecodedClick;
        // 
        // _decodedLabel
        // 
        _decodedLabel.Dock = DockStyle.Fill;
        _decodedLabel.Location = new Point(3, 122);
        _decodedLabel.Name = "_decodedLabel";
        _decodedLabel.Size = new Size(90, 38);
        _decodedLabel.TabIndex = 6;
        _decodedLabel.Text = "解码后路径";
        _decodedLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _decodedPathBox
        // 
        _decodedPathBox.BackColor = SystemColors.Window;
        _decodeLayout.SetColumnSpan(_decodedPathBox, 2);
        _decodedPathBox.Dock = DockStyle.Fill;
        _decodedPathBox.Location = new Point(99, 128);
        _decodedPathBox.Margin = new Padding(3, 6, 3, 6);
        _decodedPathBox.Name = "_decodedPathBox";
        _decodedPathBox.ReadOnly = true;
        _decodedPathBox.Size = new Size(940, 23);
        _decodedPathBox.TabIndex = 7;
        // 
        // _decodePicture
        // 
        _decodePicture.BackColor = Color.FromArgb(32, 32, 32);
        _decodePicture.BorderStyle = BorderStyle.FixedSingle;
        _decodeLayout.SetColumnSpan(_decodePicture, 3);
        _decodePicture.Dock = DockStyle.Fill;
        _decodePicture.Location = new Point(0, 164);
        _decodePicture.Margin = new Padding(0, 4, 0, 0);
        _decodePicture.Name = "_decodePicture";
        _decodePicture.Size = new Size(1042, 326);
        _decodePicture.SizeMode = PictureBoxSizeMode.Zoom;
        _decodePicture.TabIndex = 8;
        _decodePicture.TabStop = false;
        // 
        // _statusLabel
        // 
        _statusLabel.AutoEllipsis = true;
        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.Location = new Point(11, 745);
        _statusLabel.Name = "_statusLabel";
        _statusLabel.Size = new Size(1062, 28);
        _statusLabel.TabIndex = 2;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // MainForm
        // 
        ClientSize = new Size(1084, 781);
        Controls.Add(_rootLayout);
        MinimumSize = new Size(880, 680);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PicHider 图片隐写工具";
        _ratioPanel.ResumeLayout(false);
        _ratioPanel.PerformLayout();
        _rootLayout.ResumeLayout(false);
        _rootLayout.PerformLayout();
        _maskGroup.ResumeLayout(false);
        _maskGroup.PerformLayout();
        _maskTable.ResumeLayout(false);
        _tabs.ResumeLayout(false);
        _embedPage.ResumeLayout(false);
        _embedLayout.ResumeLayout(false);
        _embedLayout.PerformLayout();
        _embedActions.ResumeLayout(false);
        _embedActions.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_embedPicture).EndInit();
        _generatePage.ResumeLayout(false);
        _generateLayout.ResumeLayout(false);
        _generateLayout.PerformLayout();
        _generateActions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_generatePicture).EndInit();
        _decodePage.ResumeLayout(false);
        _decodeLayout.ResumeLayout(false);
        _decodeLayout.PerformLayout();
        _decodeActions.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_decodePicture).EndInit();
        ResumeLayout(false);
    }

    #endregion
}
