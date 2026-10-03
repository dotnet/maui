#if IOS || MACCATALYST // The floating glass tab bar is a UIKit-only feature introduced in iOS/MacCatalyst 26. This issue does not affect Android or Windows.
using NUnit.Framework;
using ImageMagick;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35490 : _IssuesUITest
{
	public Issue35490(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS 26] TabbedPage with NavigationPage children clips content above floating glass tab bar";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void NavigationPageChildContentExtendsUnderFloatingTabBar()
	{
		App.WaitForElement("Tab1Label");
		VerifyBackgroundUnderTabBar();
	}

#if IOS
	void VerifyBackgroundUnderTabBar()
	{
		if (!HelperExtensions.IsIOS26OrHigher((AppiumIOSApp)App))
		{
			VerifyScreenshot();
			return;
		}

		App.RetryAssert(() =>
		{
			var window = App.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeApplication")).GetRect();
			var tabBar = App.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeTabBar")).GetRect();
			Assert.That(tabBar.Height, Is.GreaterThan(0));
			using var screenshot = new MagickImage(App.Screenshot());
			using var pixels = screenshot.GetPixels();
			var scale = screenshot.Width / (double)window.Width;
			var y = (int)Math.Round(tabBar.CenterY() * scale);
			var referenceY = (int)Math.Round((tabBar.Top - 30) * scale);
			// Sample the exposed content beside the floating bar, not its translucent material or text.
			foreach (var x in new[] { window.Left + 5, window.Right - 5 })
			{
				var pixelX = (int)Math.Round(x * scale);
				var reference = pixels.GetPixel(pixelX, referenceY).ToColor()!;
				Assert.That(reference.B, Is.GreaterThan(reference.R), "The reference must be the purple page, not white chrome.");
				Assert.That(reference.R, Is.GreaterThan(reference.G));
				var color = pixels.GetPixel(pixelX, y).ToColor();
				Assert.That(color, Is.EqualTo(reference),
					"The NavigationPage background must extend below the top of the floating tab bar.");
			}
		});
	}
#else
	void VerifyBackgroundUnderTabBar()
	{
		VerifyScreenshot();
	}
#endif
}
#endif
