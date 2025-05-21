#if MACCATALYST || WINDOWS // MenuBarItems are only supported on desktop platforms
using NUnit.Framework;
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

    [Test]
    public void MenuFlyoutItem_ClickedEventWorks()
    {
        App.WaitForElement(MenuItems);
        App.Tap(MenuItems);
        App.WaitForElement(ClickedEventItem);
        App.Tap(ClickedEventItem);

        Assert.That(App.WaitForElement(ResultLabel)?.GetText(), Is.EqualTo("Clicked event handler executed"));
    }

    [Test]
    public void MenuFlyoutItem_CommandWorks()
    {
        App.WaitForElement(MenuItems);
        App.Tap(MenuItems);
        App.WaitForElement(CommandItem);
        App.Tap(CommandItem);

        Assert.That(App.WaitForElement(ResultLabel)?.GetText(), Is.EqualTo("Command executed"));
    }

    [Test]
    public void MenuFlyoutItem_CommandWithParameterWorks()
    {
        App.WaitForElement(MenuItems);
        App.Tap(MenuItems);
        App.WaitForElement(CommandWithParamItem);
        App.Tap(CommandWithParamItem);

        Assert.That(App.WaitForElement(ResultLabel)?.GetText(), Is.EqualTo("Command executed with parameter: Test Parameter"));
    }
}

#endif
