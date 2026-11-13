#if ANDROID // Android-specific: OnBackButtonPressed uses onBackPressedDispatcher
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8680 : _IssuesUITest
{
	public Issue8680(TestDevice device) : base(device) { }

	public override string Issue => "Rework OnBackButtonPressed to use onBackPressedDispatcher";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void BackButtonPressIsInterceptedByOnBackButtonPressed()
	{
		// Navigate to the intercept page
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		// Confirm we are on the intercept page
		App.WaitForElement("InterceptPageLabel");

		// Press the device back button — should be intercepted (page stays)
		App.Back();

		// The page should still be visible because OnBackButtonPressed returned true
		App.WaitForElement("StatusLabel");
		Assert.That(
		 App.FindElement("StatusLabel").GetText(),
		 Is.EqualTo("Back intercepted: 1"),
		 "OnBackButtonPressed should have been called exactly once per back press (detects dual-fire regression on API 33+).");

		// The intercept page should still be displayed (not popped)
		App.WaitForElement("InterceptPageLabel");
	}
}
#endif