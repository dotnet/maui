using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32667 : _IssuesUITest
	{
		public Issue32667(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Navbar keeps reserving space after navigating to page with Shell.NavBarIsVisible=\"False\"";

		[Test]
		[Category(UITestCategories.Shell)]
		public void NavBarShouldNotReserveSpaceWhenNavigatingBackToHiddenNavBarPage()
		{
			// Verify we start on the main page with hidden nav bar
			App.WaitForElement("MainPageLabel");
			
			// Navigate to sub page (which has visible nav bar)
			App.Tap("NavigateButton");
			
			// Wait for sub page to load
			App.WaitForElement("SubPageLabel");
			
			// Navigate back using the back button
			// On iOS, this would be the navigation bar back button
			App.Back();
			
			// Wait for main page to reappear
			App.WaitForElement("MainPageLabel");
			
			// Verify screenshot - this will show whether the nav bar space is incorrectly reserved
			// With the bug, there would be a blank space at the top
			// With the fix, the content should fill to the top (accounting for safe area)
			VerifyScreenshot();
		}
	}
}
