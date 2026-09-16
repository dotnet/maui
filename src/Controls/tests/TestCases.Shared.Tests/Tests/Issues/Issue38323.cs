#if TEST_FAILS_ON_WINDOWS // More tab is not displayed on Windows, so MoreNavigationController navigation cannot be tested on this platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38323 : _IssuesUITest
{
    public Issue38323(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "[iOS] Shell navigation operations fail under the native More tab";

    [Test]
    [Category(UITestCategories.Shell)]
    public void NavigationOperationsWorkUnderMoreTab()
    {
        OpenTestPageUnderMoreTab();

        App.Tap("InsertPageButton");
        var status = App.WaitForElement("OperationStatusLabel");
        Assert.That(status.GetText(), Is.EqualTo("Insert succeeded"));

        App.Tap("RemovePageButton");
        App.WaitForElementTillPageNavigationSettled("InsertedPageLabel");

        App.Tap("PopToRootButton");
        App.WaitForElementTillPageNavigationSettled("RootPageLabel");
    }

    void OpenTestPageUnderMoreTab()
    {
        App.WaitForElement("More");
        App.Tap("More");
        App.WaitForElement("More 1");
        App.Tap("More 1");
        App.WaitForElementTillPageNavigationSettled("PushPageButton");
        App.Tap("PushPageButton");
        App.WaitForElementTillPageNavigationSettled("InsertPageButton");
    }
}
#endif
