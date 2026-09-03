using System.Globalization;
using System.Text;
using Rubujo.YouTube.Utility.Models.LiveChat;
using YTLiveChatCatcher.Common.Utils;
using YTLiveChatCatcher.Common.Controls;

namespace YTLiveChatCatcher;

/// <summary>進階篩選、無損匯出、分析與問題回報工具。</summary>
public sealed class FDataTools : Form
{
    private readonly FMain _main;
    private readonly IReadOnlyList<RendererData> _messages;
    private readonly TextBox _start = new() { PlaceholderText = "開始時間，例如 2026-09-03 12:00" };
    private readonly TextBox _end = new() { PlaceholderText = "結束時間，例如 2026-09-03 13:00" };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _author = new() { PlaceholderText = "作者名稱（包含）" };
    private readonly NumericUpDown _minimum = new() { DecimalPlaces = 2, Maximum = 1000000000, ThousandsSeparator = true };
    private readonly NumericUpDown _maximum = new() { DecimalPlaces = 2, Maximum = 1000000000, ThousandsSeparator = true };
    private readonly CheckBox _useMinimum = new() { Text = "最低金額" };
    private readonly CheckBox _useMaximum = new() { Text = "最高金額" };
    private readonly NumericUpDown _revenuePercent = new() { DecimalPlaces = 1, Minimum = 0, Maximum = 100, Increment = 1 };
    private readonly TextBox _analysis = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
    private readonly AnalyticsChartPanel _chart = new() { Dock = DockStyle.Fill };

    public FDataTools(FMain main)
    {
        _main = main;
        _messages = main.GetRawMessagesSnapshot();

        Text = $"資料工具 - {main.Text}";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(760, 560);
        Size = new Size(900, 650);

        _type.Items.Add("全部類型");
        _type.Items.AddRange(_messages.Select(message => message.Type)
            .Where(type => !string.IsNullOrWhiteSpace(type))
            .Distinct(StringComparer.Ordinal)
            .Order()
            .Cast<object>()
            .ToArray());
        _type.SelectedIndex = 0;
        _revenuePercent.Value = main.GetRevenueEstimateRate() * 100m;

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 4,
            RowCount = 8
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        AddLabeled(layout, "開始時間（留空不限）", _start, 0, 0);
        AddLabeled(layout, "結束時間（留空不限）", _end, 2, 0);
        AddLabeled(layout, "訊息類型", _type, 0, 2);
        AddLabeled(layout, "作者", _author, 2, 2);
        layout.Controls.Add(_useMinimum, 0, 4);
        layout.Controls.Add(_minimum, 1, 4);
        layout.Controls.Add(_useMaximum, 2, 4);
        layout.Controls.Add(_maximum, 3, 4);

        FlowLayoutPanel actions = new() { Dock = DockStyle.Fill, AutoSize = true };
        actions.Controls.Add(CreateButton("套用並分析", (_, _) => RefreshAnalysis()));
        actions.Controls.Add(CreateButton("匯出 JSONL", (_, _) => Export("JSON Lines|*.jsonl", ChatDataTools.ExportJsonLines)));
        actions.Controls.Add(CreateButton("匯出 CSV", (_, _) => Export("CSV|*.csv", ChatDataTools.ExportCsv)));
        actions.Controls.Add(CreateButton("匯入 JSONL/CSV", (_, _) => Import()));
        actions.Controls.Add(CreateButton("設定持續 JSONL", (_, _) => ConfigureStreamingJsonLines()));
        actions.Controls.Add(CreateButton("產生診斷包", (_, _) => CreateDiagnosticBundle()));
        layout.Controls.Add(actions, 0, 5);
        layout.SetColumnSpan(actions, 4);

        FlowLayoutPanel ratePanel = new() { Dock = DockStyle.Fill, AutoSize = true };
        ratePanel.Controls.Add(new Label { Text = "粗估收益比例（%）：", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        ratePanel.Controls.Add(_revenuePercent);
        ratePanel.Controls.Add(CreateButton("儲存比例", (_, _) =>
        {
            _main.SetRevenueEstimateRate(_revenuePercent.Value / 100m);
            MessageBox.Show("已儲存。此比例僅為粗略估算，不代表實際結算。", Text);
        }));
        layout.Controls.Add(ratePanel, 0, 6);
        layout.SetColumnSpan(ratePanel, 4);

        SplitContainer analysisArea = new() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 210 };
        analysisArea.Panel1.Controls.Add(_chart);
        _analysis.Dock = DockStyle.Fill;
        analysisArea.Panel2.Controls.Add(_analysis);
        layout.Controls.Add(analysisArea, 0, 7);
        layout.SetColumnSpan(analysisArea, 4);
        Controls.Add(layout);

        RefreshAnalysis();
    }

    private static void AddLabeled(TableLayoutPanel panel, string label, Control control, int column, int row)
    {
        panel.Controls.Add(new Label { Text = label, AutoSize = true }, column, row);
        control.Dock = DockStyle.Fill;
        panel.Controls.Add(control, column, row + 1);
        panel.SetColumnSpan(control, 2);
    }

    private static Button CreateButton(string text, EventHandler click)
    {
        Button button = new() { Text = text, AutoSize = true };
        button.Click += click;
        return button;
    }

    private IReadOnlyList<RendererData> GetFiltered()
    {
        DateTimeOffset? start = ParseOptionalTime(_start.Text, "開始時間");
        DateTimeOffset? end = ParseOptionalTime(_end.Text, "結束時間");

        return ChatDataTools.Filter(_messages, new ChatFilterOptions(
            start,
            end,
            _type.SelectedIndex > 0 ? _type.SelectedItem?.ToString() : null,
            _author.Text,
            _useMinimum.Checked ? _minimum.Value : null,
            _useMaximum.Checked ? _maximum.Value : null));
    }

    private void RefreshAnalysis()
    {
        try
        {
            ChatAnalytics analytics = ChatDataTools.Analyze(GetFiltered());
            _chart.SetAnalytics(analytics);
            StringBuilder output = new();
            output.AppendLine($"符合條件：{analytics.MessageCount} 筆");
            output.AppendLine();
            output.AppendLine("幣別分布：");
            output.AppendLine(analytics.AmountsByCurrency.Count == 0 ? "（無付費事件）" :
                string.Join(Environment.NewLine, analytics.AmountsByCurrency.Select(item => $"{item.Key}{item.Value}")));
            output.AppendLine();
            output.AppendLine("活躍作者（前 20 名）：");
            output.AppendLine(string.Join(Environment.NewLine, analytics.ActiveAuthors.Take(20).Select(item => $"{item.Key}：{item.Value}")));
            output.AppendLine();
            output.AppendLine("訊息密度（每分鐘，前 100 筆）：");
            output.AppendLine(string.Join(Environment.NewLine, analytics.MessagesByMinute.Take(100).Select(item => $"{item.Key}：{item.Value}")));
            output.AppendLine();
            output.AppendLine("付費事件時間軸：");
            output.AppendLine(string.Join(Environment.NewLine, analytics.PaidTimeline.Select(item =>
                $"{ChatDataTools.ParseTimestamp(item.TimestampUsec)?.ToLocalTime():yyyy-MM-dd HH:mm:ss}  {item.AuthorName}  {item.PurchaseAmountText}")));
            _analysis.Text = output.ToString();
        }
        catch (FormatException ex)
        {
            MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void Export(string filter, Action<string, IEnumerable<RendererData>> exporter)
    {
        IReadOnlyList<RendererData> data = GetFiltered();
        using SaveFileDialog dialog = new() { Filter = filter, FileName = $"聊天室資料_{DateTime.Now:yyyyMMdd_HHmmss}" };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            exporter(dialog.FileName, data);
            MessageBox.Show($"已匯出 {data.Count} 筆資料。", Text);
        }
    }

    private void CreateDiagnosticBundle()
    {
        using SaveFileDialog dialog = new() { Filter = "ZIP 診斷包|*.zip", FileName = $"YTLiveChatCatcher_診斷包_{DateTime.Now:yyyyMMdd_HHmmss}.zip" };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            DiagnosticBundleBuilder.Create(
                dialog.FileName,
                _main.GetCaptureSessionSnapshot(),
                GetFiltered(),
                Path.Combine(AppContext.BaseDirectory, "Logs", "log.txt"),
                _main.GetSanitizedRawResponsesSnapshot());
            MessageBox.Show("診斷包已產生；傳送前仍請自行檢查內容。", Text);
        }
    }

    private void Import()
    {
        using OpenFileDialog dialog = new()
        {
            Filter = "JSON Lines 或 CSV|*.jsonl;*.csv|JSON Lines|*.jsonl|CSV|*.csv",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            IReadOnlyList<RendererData> messages = string.Equals(Path.GetExtension(dialog.FileName), ".csv", StringComparison.OrdinalIgnoreCase) ?
                ChatDataTools.ImportCsv(dialog.FileName) :
                ChatDataTools.ImportJsonLines(dialog.FileName);
            int imported = _main.ImportRawMessages(messages);
            MessageBox.Show($"已匯入 {imported} 筆新資料；重複事件已略過。請重新開啟資料工具以分析完整資料。", Text);
            Close();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or System.Text.Json.JsonException)
        {
            MessageBox.Show($"匯入失敗：{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ConfigureStreamingJsonLines()
    {
        using SaveFileDialog dialog = new()
        {
            Filter = "JSON Lines|*.jsonl",
            FileName = $"聊天室串流_{DateTime.Now:yyyyMMdd_HHmmss}.jsonl",
            OverwritePrompt = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                File.WriteAllText(dialog.FileName, string.Empty, new UTF8Encoding(false));
                _main.ConfigureStreamingJsonLines(dialog.FileName);
                MessageBox.Show("已啟用持續 JSONL；之後擷取到的新批次會同步附加寫入，寫入失敗不會中止擷取。", Text);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show($"無法建立 JSONL：{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private static DateTimeOffset? ParseOptionalTime(string text, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTimeOffset value))
        {
            return value;
        }

        throw new FormatException($"{fieldName}格式無法辨識。");
    }
}
