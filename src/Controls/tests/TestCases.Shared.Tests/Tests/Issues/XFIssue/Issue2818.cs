#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
// Orientation not supported in Catalyst and Windows
// On iOS FlyoutPage RTL is not working as expected, Issue: https://github.com/dotnet/maui/issues/26726
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
		var windowSize = App.WaitForElement("RootLayout");
		App.BackgroundApp();
		App.WaitForNoElement("RootLayout");
		App.ForegroundApp();
		var newWindowSize = App.WaitForElement("RootLayout");
		Assert.That(newWindowSize.GetRect().Width, Is.EqualTo(windowSize.GetRect().Width));
		Assert.That(newWindowSize.GetRect().Height, Is.EqualTo(windowSize.GetRect().Height));
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}

}
#endif



