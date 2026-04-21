using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9095 : _IssuesUITest
{
	public Issue9095(TestDevice device) : base(device) { }

	public override string Issue => "Shell toolbar back button doesn't fire Shell.OnBackButtonPressed on Android and iOS";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellOnBackButtonPressedShouldBeInvokedWhenPressingNavigationBarBackButton()
	{
		// Navigate to the second page
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		// Wait for the second page to appear
		App.WaitForElement("BackButtonPressedLabel");

		// Verify the initial state
		var initialText = App.FindElement("BackButtonPressedLabel").GetText();
		Assert.That(initialText, Is.EqualTo("OnBackButtonPressed Not Called"),
			"Label should show 'Not Called' before pressing back.");

		// Tap the Shell toolbar back button
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			App.TapBackArrow(); // iOS 26+ doesn't show the previous page title in the back button
		else
			App.TapBackArrow(Device is TestDevice.iOS or TestDevice.Mac ? "HomePage" : "");

		// Shell.OnBackButtonPressed should have been called, updating the label.
		// The second page remains visible because Shell.OnBackButtonPressed returns true.
		var updatedText = App.WaitForElement("BackButtonPressedLabel").GetText();
		Assert.That(updatedText, Is.EqualTo("OnBackButtonPressed Called"),
			"Shell.OnBackButtonPressed should be invoked when pressing the Shell toolbar back button.");
	}
}
