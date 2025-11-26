#if ANDROID || IOS && TEST_FAILS_ON_ANDROID // related issue link: https://github.com/dotnet/maui/issues/32275
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue15173 : _IssuesUITest
	{
		public override string Issue => "Shell Flyout overlay does not resize on device rotation";

		public Issue15173(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Shell)]
		public void FlyoutOverlayResizesOnRotation()
		{
			// Open the flyout
			App.TapShellFlyoutIcon();

			// Wait for flyout to open
			App.WaitForElement("MenuItem1");

			// Rotate to landscape
			App.SetOrientationLandscape();

			// Wait for rotation to complete
			Task.Delay(2000).Wait();

			// Verify menu items are still visible after rotation
			// If the overlay resized correctly, the flyout should still be fully visible
			App.WaitForElement("MenuItem1");
			App.WaitForElement("MenuItem2");
			App.WaitForElement("MenuItem3");
			VerifyScreenshot();
		}
	}
}
#endif