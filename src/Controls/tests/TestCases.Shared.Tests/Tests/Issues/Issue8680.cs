#if TEST_FAILS_ON_IOS || TEST_FAILS_ON_CATALYST // https://github.com/dotnet/maui/issues/8296
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
		App.TapBackArrow();

		// The page should still be visible because OnBackButtonPressed returned true
		App.WaitForElement("StatusLabel");
		Assert.That(
		 App.FindElement("StatusLabel").GetText(),
		 Does.Contain("Back intercepted"),
		 "OnBackButtonPressed should have been called and returned true, intercepting the back press.");

		// The intercept page should still be displayed (not popped)
		App.WaitForElement("InterceptPageLabel");
	}
}
#endif