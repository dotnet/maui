#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37781 : _IssuesUITest
{
	public Issue37781(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Interactive pop gesture ignores OnBackButtonPressed when the navigation bar is hidden";

	protected override bool ResetAfterEachTest => true;

	[Test]
	[Category(UITestCategories.Navigation)]
	public void EdgeSwipeRemainsOnPageWhenBackButtonPressedReturnsTrue()
	{
		App.WaitForElement("NavigateHandledButton");
		App.Tap("NavigateHandledButton");
		App.WaitForElement("HandledStatusLabel");

		App.SwipeBackNavigation();

		var statusText = App.WaitForElement("HandledStatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("Handled back invoked"),
			"The edge swipe should invoke OnBackButtonPressed and remain on the page when it returns true.");
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public void EdgeSwipePopsPageWhenBackButtonPressedReturnsFalse()
	{
		App.WaitForElement("NavigateUnhandledButton");
		App.Tap("NavigateUnhandledButton");
		App.WaitForElement("UnhandledPageLabel");

		App.SwipeBackNavigation();

		var statusText = App.WaitForElement("UnhandledStatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("Unhandled back invoked"),
			"The edge swipe should invoke OnBackButtonPressed and pop the page when it returns false.");
	}
}
#endif