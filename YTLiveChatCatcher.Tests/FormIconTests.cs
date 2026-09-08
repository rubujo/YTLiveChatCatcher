using System.Reflection;
using System.Windows.Forms;
using Xunit;

namespace YTLiveChatCatcher.Tests;

public class FormIconTests
{
    [Fact]
    public void 所有應用程式視窗皆由統一圖示基底類別衍生()
    {
        Type[] forms = typeof(FMain).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Form).IsAssignableFrom(type))
            .ToArray();

        Assert.NotEmpty(forms);
        Assert.All(forms, type => Assert.True(
            typeof(AppForm).IsAssignableFrom(type),
            $"{type.FullName} 未使用 {nameof(AppForm)}。"));
    }

    [Fact]
    public void 統一視窗基底類別套用應用程式圖示()
    {
        using TestForm form = new();
        using MemoryStream actual = new();
        using MemoryStream expected = new();

        Assert.NotNull(form.Icon);
        form.Icon.Save(actual);
        Properties.Resources.app_icon.Save(expected);
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    private sealed class TestForm : AppForm;
}
