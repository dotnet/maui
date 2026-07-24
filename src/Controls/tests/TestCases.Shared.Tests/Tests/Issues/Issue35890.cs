#if IOS || ANDROID // Keyboard is only available on mobile platforms
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35890 : _IssuesUITest
{
	public Issue35890(TestDevice device) : base(device) { }

	public override string Issue => "HideSoftInputOnTapped=True on one page permanently affects all other pages if NavigatedFrom is never fired";

	// FixtureSetup (and therefore TryToResetTestState/NavigateToIssue) runs once per fixture,
	// not before each [Test]. All tests in this class share the same Shell instance, and
	// logging in hides the Login ShellContent permanently (IsVisible = false). So whichever
	// test runs first can use "LoginButton", but a test running afterwards must detect that
	// the app is already on the HomePage instead of trying to log in again.
	void EnsureOnHomePage()
	{
		if (App.IsKeyboardShown())
			App.DismissKeyboard();

		try
		{
			App.WaitForElement("HomePageLabel", timeout: TimeSpan.FromSeconds(3));
			return;
		}
		catch (TimeoutException)
		{
			// Not on the HomePage yet - fall through and log in.
		}

		App.WaitForElement("LoginButton");
		App.Tap("LoginButton");
		App.WaitForElement("HomePageLabel");
	}

	[Test]
	[Category(UITestCategories.SoftInput)]
	public void HideSoftInputOnTappedDoesNotPersistAfterShellContentHiddenWithoutNavigatedFrom()
	{
		// Step 1: Log in from the LoginPage (HideSoftInputOnTapped = True), unless a previous
		// test in this fixture has already done so.
		// This triggers NavigatedTo on the LoginPage, which registers it in
		// HideSoftInputOnTappedChangedManager._contentPages.
		// The login action hides the ShellContent (IsVisible=false) and navigates
		// via absolute GoToAsync — which does NOT fire NavigatedFrom on the LoginPage.
		// Step 2: Verify we are on the HomePage (HideSoftInputOnTapped = False).
		EnsureOnHomePage();

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

	[Test]
	[Category(UITestCategories.SoftInput)]
	public void HideSoftInputOnTappedRespectsPerPageSemanticsWithClassicPushNavigation()
	{
		// Navigate to the HomePage (HideSoftInputOnTapped = False), logging in first only if
		// a previous test in this fixture hasn't already done so.
		EnsureOnHomePage();

		try
		{
			// Push NavPageA (HideSoftInputOnTapped = True) via classic Navigation.PushAsync,
			// then push NavPageB (HideSoftInputOnTapped = False) on top of it.
			// Unlike the Shell absolute-navigation scenario above, NavigatedFrom fires correctly
			// for both pages here — this locks in that the fix respects per-page semantics
			// (rather than relying on global/stale tracking) even in the common push/pop case.
			App.WaitForElement("PushNavPageAButton");
			App.Tap("PushNavPageAButton");

			App.WaitForElement("PushNavPageBButton");
			App.Tap("PushNavPageBButton");

			App.WaitForElement("NavPageBEntry");
			App.Tap("NavPageBEntry");
			Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping the Entry on NavPageB.");

			// Tap outside the Entry on NavPageB (HideSoftInputOnTapped = False).
			App.WaitForElement("NavPageBEmptySpace");
			App.Tap("NavPageBEmptySpace");

			// The keyboard must NOT be dismissed: NavPageB has HideSoftInputOnTapped = False,
			// and that per-page setting must be respected regardless of NavPageA's setting.
			Assert.That(App.IsKeyboardShown(), Is.True,
				"Keyboard should remain visible when tapping outside the Entry on a page with " +
				"HideSoftInputOnTapped = False, even though the previous page in the navigation " +
				"stack had HideSoftInputOnTapped = True.");
		}
		finally
		{
			// Pop back to the HomePage so the shared Shell instance is left in a consistent
			// state for any other test that runs afterwards in this fixture.
			App.DismissKeyboard();
			this.Back();
			this.Back();
			App.WaitForElement("HomePageLabel");
		}
	}
}
#endif
