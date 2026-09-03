using YTLiveChatCatcher.Common.Utils;

namespace YTLiveChatCatcher.Common.Controls;

/// <summary>不依賴第三方繪圖套件的輕量聊天室分析圖表。</summary>
public sealed class AnalyticsChartPanel : Control
{
    private ChatAnalytics? _analytics;

    public AnalyticsChartPanel()
    {
        DoubleBuffered = true;
        BackColor = SystemColors.Window;
        AccessibleName = "聊天室分析圖表";
        AccessibleDescription = "顯示每分鐘訊息密度與各幣別付費金額分布";
    }

    public void SetAnalytics(ChatAnalytics analytics)
    {
        _analytics = analytics;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);

        if (_analytics == null || _analytics.MessageCount == 0)
        {
            e.Graphics.DrawString("沒有符合條件的資料", Font, SystemBrushes.GrayText, 12, 12);
            return;
        }

        Rectangle densityArea = new(12, 28, Math.Max(100, Width - 24), Math.Max(90, Height / 2 - 36));
        Rectangle currencyArea = new(12, Height / 2 + 20, Math.Max(100, Width - 24), Math.Max(70, Height / 2 - 32));
        DrawBars(e.Graphics, "每分鐘訊息密度（最近 40 分鐘）",
            _analytics.MessagesByMinute.TakeLast(40).Select(item => (item.Key[^5..], (decimal)item.Value)).ToList(), densityArea, Color.SteelBlue);
        DrawBars(e.Graphics, "幣別付費金額分布",
            _analytics.AmountsByCurrency.Select(item => (item.Key, item.Value)).ToList(), currencyArea, Color.DarkOrange);
    }

    private void DrawBars(Graphics graphics, string title, IReadOnlyList<(string Label, decimal Value)> values, Rectangle area, Color color)
    {
        graphics.DrawString(title, Font, SystemBrushes.ControlText, area.Left, area.Top - 20);

        if (values.Count == 0)
        {
            graphics.DrawString("無資料", Font, SystemBrushes.GrayText, area.Left, area.Top);
            return;
        }

        decimal maximum = values.Max(item => item.Value);
        float barWidth = Math.Max(2f, area.Width / (float)values.Count - 2f);

        using SolidBrush brush = new(color);
        using Font labelFont = new(Font.FontFamily, 7f);

        for (int index = 0; index < values.Count; index++)
        {
            float height = maximum == 0 ? 0 : (float)(values[index].Value / maximum) * (area.Height - 24);
            float x = area.Left + index * (area.Width / (float)values.Count);
            graphics.FillRectangle(brush, x, area.Bottom - height - 18, barWidth, height);

            if (values.Count <= 12 || index % Math.Max(1, values.Count / 8) == 0)
            {
                graphics.DrawString(values[index].Label, labelFont, SystemBrushes.ControlText, x, area.Bottom - 16);
            }
        }
    }
}
