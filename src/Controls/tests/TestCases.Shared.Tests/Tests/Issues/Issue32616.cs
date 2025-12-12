using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32616 : _IssuesUITest
	{
		public Issue32616(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Shell Flyout appears in Release builds even when FlyoutBehavior=\"Disabled\" (MacCatalyst)";

		[Test]
		[Category(UITestCategories.Shell)]
		public void FlyoutShouldNotAppearWhenDisabled()
		{
			// Initial state: flyout is disabled
			App.WaitForElement("StatusLabel");
			App.WaitForElement("EnableButton");
			
			// Verify flyout icon is not present when disabled
			App.WaitForNoFlyoutIcon();
			
			// Enable flyout
			App.Tap("EnableButton");
			
			// Verify flyout icon appears when enabled
			App.WaitForFlyoutIcon();
			
			// Disable flyout again
			App.Tap("DisableButton");
			
			// Verify flyout icon disappears when disabled
			App.WaitForNoFlyoutIcon();
		}
	}
}
