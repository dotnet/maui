#if IOS || ANDROID // Keyboard is only available on mobile platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35890NavigationPage : _IssuesUITest
{
	public Issue35890NavigationPage(TestDevice device) : base(device) { }

	public override string Issue => "HideSoftInputOnTapped should be evaluated per-page when pushing through a NavigationPage";

	[Test]
	[Category(UITestCategories.SoftInput)]
	public void HideSoftInputOnTappedDoesNotLeakToPushedPageWithoutSetting()
	{
		// Dismiss any keyboard that may already be shown before starting.
		if (App.IsKeyboardShown())
			App.DismissKeyboard();

		// Step 1: On Page A (HideSoftInputOnTapped = True), push Page B
		// (HideSoftInputOnTapped not set / defaults to false) via NavigationPage.PushAsync.
		App.WaitForElement("PushPageBButton");
		App.Tap("PushPageBButton");

		// Step 2: Verify we are on Page B.
		App.WaitForElement("PageBLabel");

		// Step 3: Focus the Entry on Page B to show the keyboard.
		App.WaitForElement("PageBEntry");
		App.Tap("PageBEntry");
		Assert.That(App.IsKeyboardShown(), Is.True, "Keyboard should be visible after tapping the Entry on Page B.");

		// Step 4: Tap outside the Entry (on the page background/button) on Page B.
		// Per-page semantics: since Page B has HideSoftInputOnTapped = false, the tap
		// should NOT dismiss the keyboard, even though Page A (still on the navigation
		// stack) has HideSoftInputOnTapped = true.
		App.WaitForElement("PageBButton");
		App.Tap("PageBButton");

		// Step 5: Verify the keyboard is still visible, confirming the setting from
		// Page A did not leak into Page B.
		Assert.That(App.IsKeyboardShown(), Is.True,
			"Keyboard should remain visible on Page B (HideSoftInputOnTapped = false) even though " +
			"Page A (HideSoftInputOnTapped = true) is still present on the navigation stack. " +
			"HideSoftInputOnTapped must be evaluated per-page, not globally.");

		// Step 6: Verify the button tap actually executed (not swallowed by keyboard dismissal).
		Assert.That(App.FindElement("ResultLabel").GetText(), Is.EqualTo("Button Tapped"),
			"The button's tap action should have executed.");

		App.DismissKeyboard();
	}
}
#endif
