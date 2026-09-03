using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37635 : _IssuesUITest
{
    const string ExpectedResult = "Control: 0/30; Command: 0/30";

    public Issue37635(TestDevice device) : base(device)
    {
    }

    public override string Issue => "BackButtonBehavior.Command retains discarded behaviors";

    [Test]
    [Category(UITestCategories.Shell)]
    public void BackButtonBehaviorCommandDoesNotRetainDiscardedBehavior()
    {
        App.WaitForElement("RunTestButton");
        App.Tap("RunTestButton");

        bool collected = App.WaitForTextToBePresentInElement("ResultLabel", ExpectedResult);
        Assert.That(collected, Is.True, "Expected the control and shared-command cohorts to be fully collected.");
    }
}