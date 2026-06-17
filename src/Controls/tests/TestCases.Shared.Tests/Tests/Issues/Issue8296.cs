using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8296 : _IssuesUITest
	{
		public Issue8296(TestDevice device) : base(device) { }

		public override string Issue => "ContentPage.OnBackButtonPressed is not invoked on iOS and MacCatalyst with NavigationPage";

		protected override bool ResetAfterEachTest => true;

		[Test]
		[Category(UITestCategories.Navigation)]
		public void OnBackButtonPressedShouldBeInvokedOnIOSWithNavigationPage()
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

			// Press the native back button
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				App.TapBackArrow(); // In iOS 26, the previous page title is not shown along with the back arrow, so we use the default back arrow
			}
			else
			{
				App.TapBackArrow(Device is TestDevice.iOS or TestDevice.Mac ? "HomePage" : "");
			}

			// OnBackButtonPressed should have been called, updating the label
			// The second page stays visible because OnBackButtonPressed returns true
			var updatedText = App.WaitForElement("BackButtonPressedLabel").GetText();
			Assert.That(updatedText, Is.EqualTo("OnBackButtonPressed Called"),
				"OnBackButtonPressed should have been invoked when pressing the back button.");
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public void OnBackButtonPressedReturnFalseShouldNavigateBack()
		{
			// Navigate to the return-false page
			App.WaitForElement("NavigateReturnFalseButton");
			App.Tap("NavigateReturnFalseButton");

			// Wait for the return-false page to appear
			App.WaitForElement("ReturnFalsePageLabel");

			// Press the native back button
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				App.TapBackArrow();
			}
			else
			{
				App.TapBackArrow(Device is TestDevice.iOS or TestDevice.Mac ? "HomePage" : "");
			}

			// OnBackButtonPressed returned false, so navigation should proceed back to MainPage.
			// The label on MainPage confirms OnBackButtonPressed was still called.
			App.WaitForElement("ReturnFalseStatusLabel");
			var statusText = App.FindElement("ReturnFalseStatusLabel").GetText();
			Assert.That(statusText, Is.EqualTo("OnBackButtonPressed Called And Returned False"),
				"OnBackButtonPressed should have been called even when returning false.");
		}
	}
}
