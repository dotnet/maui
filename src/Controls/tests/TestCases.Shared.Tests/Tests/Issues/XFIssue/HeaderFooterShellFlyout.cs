using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class HeaderFooterShellFlyout : _IssuesUITest
{

	//MenuItems AutomationId fails on Windows due to navViewItem replacements with native controls,
	//and, text-based IDs are ineffective on iOS. Conditional compilation is used to ensure tests run on all platforms.
#if WINDOWS
	const string ToggleHeaderFooter = "Toggle Header/Footer View";
	const string ToggleHeaderFooterTemplate = "Toggle Header/Footer Template";
	const string ResizeHeaderFooter = "Resize Header/Footer";
#else
	const string ToggleHeaderFooter = "ToggleHeaderFooter";
	const string ToggleHeaderFooterTemplate = "ToggleHeaderFooterTemplate";
	const string ResizeHeaderFooter = "ResizeHeaderFooter";
#endif

	public HeaderFooterShellFlyout(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Header Footer";

#if IOS
    [Test]
    public void FlyoutHeaderWithZeroMarginShouldHaveNoY()
    {
        App.Tap("ZeroMarginHeader");
        var layout = App.WaitForElement("ZeroMarginLayout").GetRect().Y;
        Assert.That(layout, Is.EqualTo(0));
    }
#endif

	[Test]
	public void AFlyoutTests()
	{
		App.WaitForElement("PageLoaded");
		App.Tap(ToggleHeaderFooter);
		App.WaitForElement("Header");
		App.WaitForElement("Footer");

		// Verify Template takes priority over header footer
		App.Tap(ToggleHeaderFooterTemplate);
		App.WaitForElement("Header Template");
		App.WaitForElement("Footer Template");
		App.WaitForNoElement("Header");
		App.WaitForNoElement("Footer");

		// Verify turning off Template shows Views again
		App.Tap(ToggleHeaderFooterTemplate);
		App.WaitForElement("Header");
		App.WaitForElement("Footer");
		App.WaitForNoElement("Header Template");
		App.WaitForNoElement("Footer Template");

		// Verify turning off header/footer clear out views correctly
		App.Tap(ToggleHeaderFooter);
		App.WaitForNoElement("Header Template");
		App.WaitForNoElement("Footer Template");
		App.WaitForNoElement("Header");
		App.WaitForNoElement("Footer");

		// verify header and footer react to size changes
		// These tests are ignored on iOS and Catalyst because the header height doesn't update correctly. Refer to issue: https://github.com/dotnet/maui/issues/26397
		// On Windows, the stack layout's AutomationId isn't behaving as expected, so the Y position of the first flyout item is used to verify header and footer sizes.
#if ANDROID

		App.Tap(ResizeHeaderFooter);
		var headerSizeSmall = App.WaitForElement("HeaderView").GetRect();
		var footerSizeSmall = App.WaitForElement("FooterView").GetRect();

		App.Tap(ResizeHeaderFooter);
		var headerSizeLarge = App.WaitForElement("HeaderView").GetRect();
		var footerSizeLarge = App.WaitForElement("FooterView").GetRect();

		App.Tap(ResizeHeaderFooter);
		var headerSizeSmall2 = App.WaitForElement("HeaderView").GetRect();
		var footerSizeSmall2 = App.WaitForElement("FooterView").GetRect();

		Assert.That(headerSizeLarge.Height, Is.GreaterThan(headerSizeSmall.Height));
		Assert.That(footerSizeLarge.Height, Is.GreaterThan(footerSizeSmall.Height));
		Assert.That(headerSizeSmall2.Height, Is.EqualTo(headerSizeSmall.Height));
		Assert.That(footerSizeSmall2.Height, Is.EqualTo(footerSizeSmall.Height));

#elif WINDOWS

        App.Tap(ResizeHeaderFooter);
        App.WaitForElement("Header");
        var headerSizeSmall = App.WaitForElement("Flyout Item").GetRect();
        var footerSizeSmall = App.WaitForElement("Footer").GetRect();

        App.Tap(ResizeHeaderFooter);
        var headerSizeLarge = App.WaitForElement("Flyout Item").GetRect();
        var footerSizeLarge = App.WaitForElement("Footer").GetRect();

        App.Tap(ResizeHeaderFooter);
        var headerSizeSmall2 = App.WaitForElement("Flyout Item").GetRect();
        var footerSizeSmall2 = App.WaitForElement("Footer").GetRect();

        Assert.That(headerSizeLarge.Y, Is.GreaterThan(headerSizeSmall.Y));
        Assert.That(footerSizeLarge.Y, Is.LessThan(footerSizeSmall.Y));
        Assert.That(headerSizeSmall2.Y, Is.EqualTo(headerSizeSmall.Y));
        Assert.That(footerSizeSmall2.Y, Is.EqualTo(footerSizeSmall.Y));
#endif
	}
}