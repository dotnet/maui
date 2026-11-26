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
    public void VerifyNavigatingToAndNavigatingFromArgsForPopAndPopToRoot()
    {
        // First page is shown — navigate to second
        App.WaitForElement("Issue35613_NavigateButton");
        App.Tap("Issue35613_NavigateButton");

        // Second page: verify Push events from First→Second
        App.WaitForElement("Issue35613_Second_LogEditor");
        // OnNavigatedFrom/OnNavigatedTo are raised after the push transition completes.
        App.WaitForTextToBePresentInElement("Issue35613_Second_LogEditor", "OnNavigatedFrom FirstPage [Push], DestinationPage=Issue35613SecondPage");
        App.WaitForTextToBePresentInElement("Issue35613_Second_LogEditor", "OnNavigatedTo SecondPage [Push], PreviousPage=Issue35613FirstPage");
        var secondLog = App.FindElement("Issue35613_Second_LogEditor").GetText();
        Assert.That(secondLog, Does.Contain("OnNavigatingFrom FirstPage [Push], DestinationPage=Issue35613SecondPage"));
        Assert.That(secondLog, Does.Contain("OnNavigatedFrom FirstPage [Push], DestinationPage=Issue35613SecondPage"));
        Assert.That(secondLog, Does.Contain("OnNavigatedTo SecondPage [Push], PreviousPage=Issue35613FirstPage"));

        // Navigate to third
        App.Tap("Issue35613_NavigateToThirdButton");

        // Third page: verify Push events from Second→Third
        App.WaitForElement("Issue35613_Third_LogEditor");
        App.WaitForTextToBePresentInElement("Issue35613_Third_LogEditor", "OnNavigatedFrom SecondPage [Push], DestinationPage=Issue35613ThirdPage");
        App.WaitForTextToBePresentInElement("Issue35613_Third_LogEditor", "OnNavigatedTo ThirdPage [Push], PreviousPage=Issue35613SecondPage");
        var thirdLog = App.FindElement("Issue35613_Third_LogEditor").GetText();
        Assert.That(thirdLog, Does.Contain("OnNavigatingFrom SecondPage [Push], DestinationPage=Issue35613ThirdPage"));
        Assert.That(thirdLog, Does.Contain("OnNavigatedFrom SecondPage [Push], DestinationPage=Issue35613ThirdPage"));
        Assert.That(thirdLog, Does.Contain("OnNavigatedTo ThirdPage [Push], PreviousPage=Issue35613SecondPage"));

        // PopToRoot back to first
        App.Tap("Issue35613_PopToRootButton");

        // First page: verify PopToRoot events
        App.WaitForElement("Issue35613_LogEditor");
        App.WaitForTextToBePresentInElement("Issue35613_LogEditor", "OnNavigatedFrom ThirdPage [PopToRoot], DestinationPage=Issue35613FirstPage");
        App.WaitForTextToBePresentInElement("Issue35613_LogEditor", "OnNavigatedTo FirstPage [PopToRoot], PreviousPage=Issue35613ThirdPage");
        var firstLogAfterPopToRoot = App.FindElement("Issue35613_LogEditor").GetText();
        Assert.That(firstLogAfterPopToRoot, Does.Contain("OnNavigatingFrom ThirdPage [PopToRoot], DestinationPage=Issue35613FirstPage"));
        Assert.That(firstLogAfterPopToRoot, Does.Contain("OnNavigatedFrom ThirdPage [PopToRoot], DestinationPage=Issue35613FirstPage"));
        Assert.That(firstLogAfterPopToRoot, Does.Contain("OnNavigatedTo FirstPage [PopToRoot], PreviousPage=Issue35613ThirdPage"));

        // Navigate to second again, then pop back
        App.Tap("Issue35613_NavigateButton");
        App.WaitForElement("Issue35613_Second_LogEditor");
        App.Tap("Issue35613_NavigateBackButton");

        // First page: verify Pop events from Second→First
        App.WaitForElement("Issue35613_LogEditor");
        App.WaitForTextToBePresentInElement("Issue35613_LogEditor", "OnNavigatedFrom SecondPage [Pop], DestinationPage=Issue35613FirstPage");
        App.WaitForTextToBePresentInElement("Issue35613_LogEditor", "OnNavigatedTo FirstPage [Pop], PreviousPage=Issue35613SecondPage");
        var firstLogAfterPop = App.FindElement("Issue35613_LogEditor").GetText();
        Assert.That(firstLogAfterPop, Does.Contain("OnNavigatingFrom SecondPage [Pop], DestinationPage=Issue35613FirstPage"));
        Assert.That(firstLogAfterPop, Does.Contain("OnNavigatedFrom SecondPage [Pop], DestinationPage=Issue35613FirstPage"));
        Assert.That(firstLogAfterPop, Does.Contain("OnNavigatedTo FirstPage [Pop], PreviousPage=Issue35613SecondPage"));
    }
}
