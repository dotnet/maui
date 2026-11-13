#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
// Fix is Android-only (MauiSwipeView.cs). SwipeView automation doesn't work on Windows (#14777).
// On iOS/Catalyst the underlying platform bug hasn't been fixed.
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
		App.WaitForElement("buttonOne");

		App.SwipeRightToLeft("swipeOne");

		// Verify the SwipeView actually opened before proceeding
		App.WaitForElement("swipeItemOne");

		App.Click("swipeOne");

		Assert.That(App.FindElement("buttonOne").GetText(), Is.Not.EqualTo("tapped"));
	}

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeViewTappedWhenClosed_PropagatesTap()
	{
		App.WaitForElement("buttonTwo");

		App.Click("swipeTwo");

		Assert.That(App.FindElement("buttonTwo").GetText(), Is.EqualTo("tapped"));
	}
}
#endif
