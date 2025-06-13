#if MACCATALYST || WINDOWS // MenuBarItems are only supported on desktop platforms
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class Issue20685 : _IssuesUITest
{

    public const string ResultLabel = "ResultLabel";
    public const string MenuItems = "MenuItems";
    public const string ClickedEventItem = "Test Clicked Event";
    public const string CommandItem = "Test Command";
    public const string CommandWithParamItem = "Test Command Parameter";

    public Issue20685(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "MenuBarItem Commands not working on Mac Catalyst";

    private void OpenMenuAndTapItem(string menuItem)
    {
        App.WaitForElement(MenuItems);
        App.Tap(MenuItems);
        App.WaitForElement(menuItem);
        App.Tap(menuItem);
    }

    [Fact]
    public void MenuFlyoutItem_ClickedEventWorks()
    {
        OpenMenuAndTapItem(ClickedEventItem);
        Assert.Equal("Clicked event handler executed", App.WaitForElement(ResultLabel)?.GetText());
    }

    [Fact]
    public void MenuFlyoutItem_CommandWorks()
    {
        OpenMenuAndTapItem(CommandItem);
        Assert.Equal("Command executed", App.WaitForElement(ResultLabel)?.GetText());
    }

    [Fact]
    public void MenuFlyoutItem_CommandWithParameterWorks()
    {
        OpenMenuAndTapItem(CommandWithParamItem);
        Assert.Equal("Command executed with parameter: Test Parameter", App.WaitForElement(ResultLabel)?.GetText());
    }
}

#endif
