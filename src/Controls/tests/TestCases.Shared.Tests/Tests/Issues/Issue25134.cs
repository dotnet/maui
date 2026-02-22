using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25134 : _IssuesUITest
{
    public Issue25134(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Shell retains page references when replacing a page";

    [Test]
    [Category(UITestCategories.Shell)]
    public void ShellReplaceDisposesPage()
    {
        App.WaitForElement("goToChildPage");
        App.Tap("goToChildPage");
        App.WaitForElement("replace");
        App.Tap("replace");
        var button = App.WaitForElement("checkReference");
        App.Tap("checkReference");
        Assert.That(button.GetText(), Is.EqualTo("gone"));
    }
}