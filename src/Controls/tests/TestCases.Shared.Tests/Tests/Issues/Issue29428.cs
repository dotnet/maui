using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29428 : _IssuesUITest
{
    public override string Issue => "Shell flyout navigation fires NavigatedTo before Loaded event";

    public Issue29428(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Shell)]
    public void ShellFlyoutNavigationEventOrderShouldBeCorrect()
    {
        App.WaitForElement("MainPageLabel");
        App.TapShellFlyoutIcon();
        App.Tap("Page 2");
        var eventOrderText = App.WaitForElement("EventOrderLabel").GetText();
        Assert.That(eventOrderText, Is.EqualTo("Loaded called first then NavigatedTo called"));
    }
}
