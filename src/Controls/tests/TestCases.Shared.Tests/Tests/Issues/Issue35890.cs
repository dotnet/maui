#if IOS || ANDROID // Keyboard is only available on mobile platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35890 : _IssuesUITest
{
	public Issue35890(TestDevice device) : base(device) { }

	public override string Issue => "HideSoftInputOnTapped=True on one page permanently affects all other pages if NavigatedFrom is never fired";

	[Test]
	[Category(UITestCategories.SoftInput)]
	public void HideSoftInputOnTappedDoesNotPersistAfterShellContentHiddenWithoutNavigatedFrom()
	{
		// Dismiss any keyboard that may already be shown before starting.
		if (App.IsKeyboardShown())
			App.DismissKeyboard();

		// Step 1: Log in from the LoginPage (HideSoftInputOnTapped = True).
		// This triggers NavigatedTo on the LoginPage, which registers it in
		// HideSoftInputOnTappedChangedManager._contentPages.
		// The login action hides the ShellContent (IsVisible=false) and navigates
		// via absolute GoToAsync — which does NOT fire NavigatedFrom on the LoginPage.
		App.WaitForElement("LoginButton");
		App.Tap("LoginButton");

		// Step 2: Verify we are on the HomePage (HideSoftInputOnTapped = False).
		App.WaitForElement("HomePageLabel");

		// Step 3: Tap the Entry on the HomePage to show the keyboard.
		App.WaitForElement("HomeEntry");
		App.Tap("HomeEntry");
		Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping the Entry on the HomePage.");

		// Step 4: Tap the Button on the HomePage.
		// Without the fix, the stale TapWindowTracker from the LoginPage would dismiss the
		// keyboard before the button's tap action executes, and the button action would
		// NOT fire (or would fire after keyboard dismissal).
		// With the fix, the keyboard must stay visible and the button action must execute.
		App.WaitForElement("HomeButton");
		App.Tap("HomeButton");

		// Step 5: Verify keyboard is still visible.
		// On a page with HideSoftInputOnTapped=False the keyboard must NOT be dismissed.
		Assert.That(App.IsKeyboardShown(), Is.True,
			"Keyboard should remain visible when HideSoftInputOnTapped=False is set on the current page. " +
			"The stale LoginPage registration in HideSoftInputOnTappedChangedManager must have been cleaned up.");

		// Step 6: Verify the button tap actually executed (not swallowed by keyboard dismissal).
		Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Button Tapped"),
			"The button's tap action should have executed.");

		App.DismissKeyboard();
	}
}
#endif
