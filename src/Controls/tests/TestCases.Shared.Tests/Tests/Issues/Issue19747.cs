using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19747 : _IssuesUITest
	{
		public Issue19747(TestDevice device) : base(device) { }

		public override string Issue => "Shell BackButtonBehavior TextOverride property not working as expected";

		[Test]
		[Category(UITestCategories.Shell)]
		public void BackButtonTextOverrideShouldNotBeTruncated()
		{
			// Wait for main page to load
			App.WaitForElement("MainPageLabel");
			
			// Navigate to detail page
			App.Tap("NavigateButton");
			
			// Wait for detail page to load
			App.WaitForElement("DetailLabel");
			
			// On Android, we need to verify the back button displays "Cancel" without truncation
			// The fix ensures text size is dynamically adjusted to fit within the available width
			// Note: Visual verification of the back button text is platform-specific and requires
			// checking the toolbar/navigation bar rendering
			
			// For now, we verify the page navigation works correctly with TextOverride set
			// More comprehensive verification would require:
			// 1. Screenshot comparison (golden test)
			// 2. Accessibility tree inspection to read the back button text
			// 3. Platform-specific handler verification
			
			VerifyScreenshot();
		}
	}
}
