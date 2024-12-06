using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class HeaderFooterShellFlyout : _IssuesUITest
{
	public HeaderFooterShellFlyout(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Header Footer";

	// [Test]
	// [Ignore("This test fails intermittently, especially on iOS 17; ignore until we can fix it")]
	// public async Task FlyoutHeaderWithZeroMarginShouldHaveNoY()
	// {
	// 	App.WaitForElement("PageLoaded");
	// 	this.TapInFlyout("ZeroMarginHeader", makeSureFlyoutStaysOpen: true);
	// 	// Adding this to just really make sure layout is finished
	// 	// Once we move this to appium we can remove this
	// 	await Task.Delay(1000);
	// 	var layout = App.WaitForElement("ZeroMarginLayout")[0].Rect.Y;
	// 	Assert.AreEqual(0, layout);
	// }

	// [Test]
	// [FailsOnIOS]
	// public void FlyoutTests()
	// {
	// 	App.WaitForElement("PageLoaded");

	// 	// Verify Header an Footer show up at all
	// 	TapInFlyout("ToggleHeaderFooter", makeSureFlyoutStaysOpen: true);
	// 	App.WaitForElement("Header View");
	// 	App.WaitForElement("Footer View");

	// 	// Verify Template takes priority over header footer
	// 	TapInFlyout("ToggleHeaderFooterTemplate", makeSureFlyoutStaysOpen: true);
	// 	App.WaitForElement("Header Template");
	// 	App.WaitForElement("Footer Template");
	// 	App.WaitForNoElement("Header View");
	// 	App.WaitForNoElement("Footer View");

	// 	// Verify turning off Template shows Views again
	// 	TapInFlyout("ToggleHeaderFooterTemplate", makeSureFlyoutStaysOpen: true);
	// 	App.WaitForElement("Header View");
	// 	App.WaitForElement("Footer View");
	// 	App.WaitForNoElement("Header Template");
	// 	App.WaitForNoElement("Footer Template");

	// 	// Verify turning off header/footer clear out views correctly
	// 	TapInFlyout("ToggleHeaderFooter", makeSureFlyoutStaysOpen: true);
	// 	App.WaitForNoElement("Header Template");
	// 	App.WaitForNoElement("Footer Template");
	// 	App.WaitForNoElement("Header View");
	// 	App.WaitForNoElement("Footer View");

	// 	// verify header and footer react to size changes
	// 	TapInFlyout("ResizeHeaderFooter", makeSureFlyoutStaysOpen: true);
	// 	var headerSizeSmall = App.WaitForElement("Header View")[0].Rect;
	// 	var footerSizeSmall = App.WaitForElement("Footer View")[0].Rect;
	// 	TapInFlyout("ResizeHeaderFooter", makeSureFlyoutStaysOpen: true);
	// 	var headerSizeLarge = App.WaitForElement("Header View")[0].Rect;
	// 	var footerSizeLarge = App.WaitForElement("Footer View")[0].Rect;

	// 	TapInFlyout("ResizeHeaderFooter", makeSureFlyoutStaysOpen: true);
	// 	var headerSizeSmall2 = App.WaitForElement("Header View")[0].Rect;
	// 	var footerSizeSmall2 = App.WaitForElement("Footer View")[0].Rect;

	// 	Assert.Greater(headerSizeLarge.Height, headerSizeSmall.Height);
	// 	Assert.Greater(footerSizeLarge.Height, footerSizeSmall.Height);
	// 	Assert.AreEqual(headerSizeSmall2.Height, headerSizeSmall.Height);
	// 	Assert.AreEqual(footerSizeSmall2.Height, footerSizeSmall.Height);
	// }
}