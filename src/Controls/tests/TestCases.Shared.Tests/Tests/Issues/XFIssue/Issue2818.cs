#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
// Orientation not supported in Catalyst and Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.FlyoutPage)]
public class Issue2818 : _IssuesUITest
{
	public Issue2818(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Right-to-Left FlyoutPage in Xamarin.Forms Hamburger icon issue";

	[Test] // "While in landscape orientation and with the flow direction set to RightToLeft in IOS, the Flyout items are not displayed."
	public void RootViewMovesAndContentIsVisible()
	{
		var idiom = App.WaitForElement("Idiom");
		App.Tap("OpenRootView");
		App.WaitForElement("CloseRootView");
		App.Tap("CloseRootView");
		App.SetOrientationLandscape();
		App.Tap("OpenRootView");
		var positionStart = App.WaitForElement("CloseRootView").GetRect().X;
		App.Tap("CloseRootView");
		App.WaitForElement("ShowLeftToRight");
		App.Tap("ShowLeftToRight");
		App.WaitForElement("OpenRootView");
		App.Tap("OpenRootView");
		var secondPosition = App.WaitForElement("CloseRootView").GetRect().X;
		Assert.That(positionStart, Is.Not.EqualTo(secondPosition));
	}

	[Test]
	public void RootViewSizeDoesntChangeAfterBackground()
	{
		var idiom = App.WaitForElement("Idiom");
		App.SetOrientationLandscape();
		App.Tap("OpenRootView");
		App.WaitForElement("CloseRootView");
		App.Tap("CloseRootView");
		App.WaitForElementTillPageNavigationSettled("ShowLeftToRight");
		App.Tap("ShowLeftToRight");
		App.WaitForElement("OpenRootView");
		App.Tap("OpenRootView");

		// Capture the expected size as values (not an element reference) before backgrounding.
		// The pre-background element reference goes stale across the background/foreground cycle,
		// so snapshot the width/height now and compare the restored layout against these values.
		var expectedRect = App.WaitForElement("RootLayout").GetRect();

		App.BackgroundApp();
		App.WaitForNoElement("RootLayout");
		App.ForegroundApp();

		// After foregrounding, the RootLayout view is rebuilt and may momentarily report an
		// intermediate layout size while the window / flyout re-applies RTL + orientation
		// constraints. Re-resolve the element on each attempt (instead of holding a single,
		// possibly-intermediate reference) and wait for the restored size to match the size
		// captured before backgrounding. Assertions stay exact-equality, so a genuine size
		// change still fails the test rather than being masked.
		App.RetryAssert(() =>
		{
			var restoredRect = App.WaitForElement("RootLayout").GetRect();
			Assert.That(restoredRect.Width, Is.EqualTo(expectedRect.Width));
			Assert.That(restoredRect.Height, Is.EqualTo(expectedRect.Height));
		}, timeout: TimeSpan.FromSeconds(5));
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}

}
#endif



