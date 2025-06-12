#if TEST_FAILS_ON_CATALYST // Swipe Action not support on with Appium on Maccatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue16203 : _IssuesUITest
{
	public Issue16203(TestDevice device) : base(device)
	{
	}

	public override string Issue => "SwipeGestureRecognizer does not raise Swiped event";

    [Test]
	[Category(UITestCategories.Button)]
	public void ButtonWithSwipeGestureShouldWork()
	{
		App.WaitForElement("MauiButton");
		App.Tap("MauiButton");
		App.SwipeRightToLeft("MauiButton");

        var text1 = App.WaitForElement("ClickedLabel").GetText();
        var text2 = App.WaitForElement("SwipedLabel").GetText();

        Assert.That(text1,Is.EqualTo("Clicked"));
        Assert.That(text2,Is.EqualTo("Swiped"));
	}
}
#endif