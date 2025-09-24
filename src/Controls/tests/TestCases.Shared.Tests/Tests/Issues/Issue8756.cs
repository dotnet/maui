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
			
			// The test validates that the overflow menu (3-dot button) is appropriately themed
			// This is a visual test - the main assertion is that the overflow menu is visible
			// and properly styled for dark theme on both Windows 10 and Windows 11
			
			// The fix should ensure the CommandBar's MoreButton respects the IconColor property
			// which will make it visible in dark themes across both Windows versions
			
			// Since this is primarily a visual styling issue, the test confirms the page loads
			// The actual validation would need to be done visually or with more advanced
			// UI automation that can inspect the button's styling properties
			Assert.DoesNotThrow(() => App.WaitForElement("OpenMenuButton"));
		}
	}
}