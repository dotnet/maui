using Microsoft.Maui.Controls;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23921(TestDevice device) : _IssuesUITest(device)
{
	public override string Issue => "If a tap closes a SwipeView, the tap should not reach the children";

    [Test]
    [Category(UITestCategories.SwipeView)]
    public void SwipeViewTappedWhenOpen_ClosesAndDoesNotPropagateTap()
    {
        var button = App.WaitForElement("buttonOne");

        App.SwipeRightToLeft("swipeOne");

		App.Click("swipeOne");

		var buttonWasClicked = button.GetText() == "tapped";

        Assert.That(buttonWasClicked, Is.False);
    }

    [Test]
    [Category(UITestCategories.SwipeView)]
    public void SwipeViewTappedWhenClosed_PropagatesTap()
    {
        var button = App.WaitForElement("buttonTwo");

		App.Click("swipeTwo");

		var buttonWasClicked = button.GetText() == "tapped";

		Assert.That(buttonWasClicked, Is.True);
    }
}