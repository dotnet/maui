#if TEST_FAILS_ON_WINDOWS
// Windows: The snapshot does not capture the FlyoutIcon applied to the TitleBar.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IssueShell6738 : _IssuesUITest
	{
		public override string Issue => "The color of the custom icon in Shell always resets to the default blue";

		public IssueShell6738(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test, Order(1)]
		[Category(UITestCategories.Shell)]
		[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
		public void EnsureCustomFlyoutIconColor()
		{
			App.WaitForElement("IconColorChangeButton");
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Category(UITestCategories.Shell)]
		[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
		public void EnsureFlyoutIconWithForegroundColor()
		{
			string changeColor = "IconColorChangeButton";
			string defaultColor = "IconColorDefaultButton";
			App.WaitForElement(changeColor);
			App.Tap(changeColor);
			App.WaitForElement(changeColor);
			VerifyScreenshot();
			App.WaitForElement(defaultColor);
			App.Tap(defaultColor);
			App.WaitForElement(defaultColor);
			VerifyScreenshot("EnsureFlyoutIconAsDefaultIconColor");
		}
	}
}
#endif