using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33523 : _IssuesUITest
	{
		public override string Issue => "OnBackButtonPressed not firing for Shell Navigation Bar button in .NET 10 SR2";

		public Issue33523(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Shell)]
		public void OnBackButtonPressedShouldFireForShellNavigationBarButton()
		{
			// Verify we're on the main page
			App.WaitForElement("MainPageLabel");

			// Navigate to TestPage
			App.Tap("NavigateButton");
			App.WaitForElement("StatusLabel");

			// Verify initial state
			var statusLabel = App.WaitForElement("StatusLabel");
			Assert.That(statusLabel.GetText(), Is.EqualTo("OnBackButtonPressed not called"));

			// Tap the navigation bar back button
			// Note: This uses the Shell's navigation bar back button, not the system back button
			App.TapBackArrow();

			// Wait a moment for the event to fire
			App.WaitForElement("StatusLabel");

			// Verify OnBackButtonPressed was called
			statusLabel = App.FindElement("StatusLabel");
			Assert.That(statusLabel.GetText(), Is.EqualTo("OnBackButtonPressed was called"), 
				"OnBackButtonPressed should be called when tapping the Shell Navigation Bar back button");
		}
	}
}
