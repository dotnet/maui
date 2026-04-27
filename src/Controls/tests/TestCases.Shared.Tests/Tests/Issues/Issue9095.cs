#if TEST_FAILS_ON_IOS || TEST_FAILS_ON_CATALYST // PR link: https://github.com/dotnet/maui/pull/35072
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9095 : _IssuesUITest
{
	public Issue9095(TestDevice device) : base(device) { }

	public override string Issue => "Shell toolbar back button doesn't fire Shell.OnBackButtonPressed on Android and iOS";

	protected override bool ResetAfterEachTest => true;

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

		var initialContentPageText = App.FindElement("ContentPageBackButtonLabel").GetText();
		Assert.That(initialContentPageText, Is.EqualTo("ContentPage OnBackButtonPressed Not Called"),
		 "ContentPage label should show 'Not Called' before pressing back.");

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

		// ContentPage.OnBackButtonPressed should also have been called.
		var contentPageText = App.FindElement("ContentPageBackButtonLabel").GetText();
		Assert.That(contentPageText, Is.EqualTo("ContentPage OnBackButtonPressed Called"),
		 "ContentPage.OnBackButtonPressed should be invoked when pressing the Shell toolbar back button.");
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellOnBackButtonPressedReturnFalseShouldNavigateBack()
	{
		// Navigate to the return-false page
		App.WaitForElement("NavigateReturnFalseButton");
		App.Tap("NavigateReturnFalseButton");

		// Wait for the return-false page to appear
		App.WaitForElement("ReturnFalsePageLabel");

		// Tap the Shell toolbar back button
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			App.TapBackArrow();
		else
			App.TapBackArrow(Device is TestDevice.iOS or TestDevice.Mac ? "HomePage" : "");

		// Shell.OnBackButtonPressed returned false, so navigation should proceed back to root.
		// The labels on the root page confirm both Shell and ContentPage OnBackButtonPressed were called.
		App.WaitForElement("ReturnFalseStatusLabel");
		var shellStatusText = App.FindElement("ReturnFalseStatusLabel").GetText();
		Assert.That(shellStatusText, Is.EqualTo("OnBackButtonPressed Called And Returned False"),
		 "Shell.OnBackButtonPressed should have been called even when returning false.");

		var contentPageStatusText = App.FindElement("ContentPageReturnFalseStatusLabel").GetText();
		Assert.That(contentPageStatusText, Is.EqualTo("ContentPage OnBackButtonPressed Called And Returned False"),
		 "ContentPage.OnBackButtonPressed should have been called even when returning false.");
	}
}
#endif