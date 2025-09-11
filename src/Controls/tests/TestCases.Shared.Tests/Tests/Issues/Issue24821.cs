using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24821 : _IssuesUITest
	{
		public Issue24821(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Support navigating with URLs for HybridWebView";

		[Test]
		[Category(UITestCategories.HybridWebView)]
		public void HybridWebViewSourcePropertyNavigatesCorrectly()
		{
			// Wait for the page to load
			App.WaitForElement("HybridWebView");
			
			// Wait a bit for the HybridWebView to initialize
			System.Threading.Thread.Sleep(2000);
			
			// Click the navigate button to test the Source property
			App.Tap("NavigateButton");
			
			// Wait for navigation
			System.Threading.Thread.Sleep(1000);
			
			// Get the current location to verify navigation worked
			App.Tap("GetLocationButton");
			
			// Wait for the location to be retrieved
			System.Threading.Thread.Sleep(1000);
			
			// Check that the status shows navigation occurred
			var statusLabel = App.WaitForElement("StatusLabel");
			var statusText = statusLabel.GetText();
			
			// Verify that the status contains information about the current location
			Assert.That(statusText, Does.Contain("Current location"), 
				"Expected status to show current location information");
			
			// Verify that the location contains our test route
			Assert.That(statusText, Does.Contain("test-route"), 
				"Expected location to contain the test route we navigated to");
		}

		[Test]
		[Category(UITestCategories.HybridWebView)]
		public void HybridWebViewSourcePropertyHandlesSpecialCharacters()
		{
			// Wait for the page to load
			App.WaitForElement("HybridWebView");
			
			// Wait for HybridWebView to initialize
			System.Threading.Thread.Sleep(2000);
			
			// Clear the entry and enter a route with special characters
			App.ClearText("EntryForSource"); // This might be auto-generated ID
			App.EnterText("EntryForSource", "#/test's-route");
			
			// Navigate
			App.Tap("NavigateButton");
			
			// Wait for navigation
			System.Threading.Thread.Sleep(1000);
			
			// Get current location
			App.Tap("GetLocationButton");
			
			// Wait for location retrieval
			System.Threading.Thread.Sleep(1000);
			
			// Verify the navigation worked despite special characters
			var statusLabel = App.WaitForElement("StatusLabel");
			var statusText = statusLabel.GetText();
			
			Assert.That(statusText, Does.Contain("Current location"), 
				"Expected status to show current location information");
		}
	}
}