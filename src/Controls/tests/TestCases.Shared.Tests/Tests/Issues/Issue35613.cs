using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35613 : _IssuesUITest
{
    public override string Issue => "OnNavigatingFrom with a NavigationPage always has an incorrect DestinationPage parameter";

    public Issue35613(TestDevice device) : base(device)
    {
    }

    [Test]
    [Category(UITestCategories.Navigation)]
    public void VerifyNavigatingToAndNavigatingFromArgs()
    {
        App.WaitForElement("Issue35613_NavigateButton");

        App.WaitForElement("Issue35613_First_OnNavigatingToLabel");
        var firstNavigatingToBeforePush = App.FindElement("Issue35613_First_OnNavigatingToLabel").GetText();
        Assert.That(firstNavigatingToBeforePush, Is.EqualTo("PreviousPage: Null, NavigationType: Push"));

        var firstNavigatingFromBeforePush = App.FindElement("Issue35613_First_OnNavigatingFromLabel").GetText();
        Assert.That(firstNavigatingFromBeforePush, Is.EqualTo("-"));

        App.Tap("Issue35613_NavigateButton");

        App.WaitForElement("Issue35613_Second_OnNavigatingToLabel");
        var secondNavigatingToAfterPush = App.FindElement("Issue35613_Second_OnNavigatingToLabel").GetText();
        Assert.That(secondNavigatingToAfterPush, Is.EqualTo("PreviousPage: Issue35613FirstPage, NavigationType: Push"));

        var secondNavigatingFromBeforePop = App.FindElement("Issue35613_Second_OnNavigatingFromLabel").GetText();
        Assert.That(secondNavigatingFromBeforePop, Is.EqualTo("-"));

        App.Tap("Issue35613_NavigateBackButton");

        App.WaitForElement("Issue35613_First_OnNavigatingToLabel");
        var firstNavigatingToAfterPop = App.FindElement("Issue35613_First_OnNavigatingToLabel").GetText();
        Assert.That(firstNavigatingToAfterPop, Is.EqualTo("PreviousPage: Issue35613SecondPage, NavigationType: Pop"));

        var firstNavigatingFromAfterPop = App.FindElement("Issue35613_First_OnNavigatingFromLabel").GetText();
        Assert.That(firstNavigatingFromAfterPop, Is.EqualTo("DestinationPage: Issue35613SecondPage, NavigationType: Push"));
    }
}
