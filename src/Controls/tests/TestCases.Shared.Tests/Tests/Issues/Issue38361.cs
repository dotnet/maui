using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38361 : _IssuesUITest
{
    public Issue38361(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue =>
        "A singleton modal page with a manual disconnect policy renders blank when presented a second time on iOS";

    [Test]
    [Category(UITestCategories.Shell)]
    public void SingletonModalPageContentRemainsVisibleAfterSecondPresentation()
    {
        AssertModalContentIsVisible();

        App.Tap("Issue38361DismissModalButton");
        App.WaitForElement("Issue38361ShowModalButton");

        AssertModalContentIsVisible();
    }

    void AssertModalContentIsVisible()
    {
        App.WaitForElement("Issue38361ShowModalButton");
        App.Tap("Issue38361ShowModalButton");

        App.WaitForElement("Issue38361ModalContent");
    }
}
