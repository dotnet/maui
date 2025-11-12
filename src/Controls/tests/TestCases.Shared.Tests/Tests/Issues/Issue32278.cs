using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32278 : _IssuesUITest
	{
		public Issue32278(TestDevice device) : base(device) { }

		public override string Issue => "Shell content page title position incorrect/clipped";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellNavigationPageTitleNotClipped()
		{
			// Wait for the main page to load
			App.WaitForElement("NavigateButton");
			
			// Navigate to the new page
			App.Tap("NavigateButton");
			
			// Wait for the new page to appear
			App.WaitForElement("TopLabel");
			App.WaitForElement("ContentLabel");
			
			// Verify the top label is visible (not clipped by toolbar)
			var topLabel = App.FindElement("TopLabel");
			Assert.IsNotNull(topLabel, "Top label should be present");
			
			// Get the bounding box of the top label
			var rect = topLabel.GetRect();
			
			// On Android API 28, the top label should be visible below the toolbar
			// If SafeAreaEdges is working correctly, the Y position should be > 0
			// and the label should be visible (not positioned at negative Y or clipped)
			Assert.That(rect.Y, Is.GreaterThan(0), 
				"Top label should be positioned below the toolbar, not clipped");
			
			// Verify content label is also visible
			var contentLabel = App.FindElement("ContentLabel");
			Assert.IsNotNull(contentLabel, "Content label should be present");
			
			// Visual verification screenshot
			VerifyScreenshot();
		}
	}
}
