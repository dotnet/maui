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
        var swipe = (SwipeView)App.WaitForElement("swipe");
        var button = (Button)App.WaitForElement("button");

        var buttonWasClicked = false;

        button.Clicked += (s, e) => buttonWasClicked = true;

        App.SwipeLeftToRight("swipe");

        App.Click("swipe");

        Assert.That(buttonWasClicked, Is.False);
    }

    [Test]
    [Category(UITestCategories.SwipeView)]
    public void SwipeViewTappedWhenClosed_PropagatesTap()
    {
        var swipe = (SwipeView)App.WaitForElement("swipe");
        var button = (Button)App.WaitForElement("button");

        var buttonWasClicked = false;

        button.Clicked += (s, e) => buttonWasClicked = true;

        App.SwipeLeftToRight("swipe");

        App.Click("swipe");

        Assert.That(buttonWasClicked, Is.True);
    }
}