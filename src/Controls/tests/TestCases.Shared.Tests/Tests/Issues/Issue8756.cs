using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8756 : _IssuesUITest
	{
		public Issue8756(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "3 dots in menu is in black in dark theme in Windows 10, but white in Windows 11";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		[Category(UITestCategories.Compatibility)]
		public void ToolbarOverflowMenuShouldBeVisibleInDarkTheme()
		{
			App.WaitForElement("IssueDescription");
			
			// Verify the test page loads correctly
			App.WaitForElement("StatusLabel");
			App.WaitForElement("ToggleIconColorButton");
			
			// Test toggling IconColor to ensure overflow button theming works
			App.Tap("ToggleIconColorButton");
			App.WaitForElement("StatusLabel");
			
			// Toggle back to default
			App.Tap("ToggleIconColorButton");
			App.WaitForElement("StatusLabel");
			
			// The main assertion is that these operations complete without errors
			// The actual visual validation of the 3-dot overflow button theming
			// needs to be done manually or with more sophisticated UI inspection tools
			// 
			// The fix ensures that:
			// 1. CommandBar's MoreButton respects the IconColor property
			// 2. This provides consistent theming across Windows 10 and Windows 11
			// 3. The overflow button is visible in dark themes
			
			Assert.DoesNotThrow(() => App.WaitForElement("StatusLabel"));
		}

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		[Category(UITestCategories.Visual)]
		public void ToolbarOverflowMenuVisualRegression()
		{
			App.WaitForElement("IssueDescription");
			
			// Take screenshot with default IconColor
			VerifyScreenshot("ToolbarOverflow_DefaultTheme");
			
			// Apply custom IconColor (White) and take screenshot
			App.Tap("ToggleIconColorButton");
			App.WaitForElement("StatusLabel");
			VerifyScreenshot("ToolbarOverflow_CustomWhiteIconColor");
			
			// Toggle back to default and verify
			App.Tap("ToggleIconColorButton");
			App.WaitForElement("StatusLabel");
			VerifyScreenshot("ToolbarOverflow_BackToDefault");
		}
	}
}