#if IOS
using System.Drawing;
using ImageMagick;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31936 : _IssuesUITest
{
	public Issue31936(TestDevice device) : base(device) { }

	public override string Issue => "Back button FontImageSource glyph is not vertically centered on iOS 26";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FontImageSourceBackGlyphIsVerticallyCentered()
	{
		var platformVersion = (string?)((AppiumApp)App).Driver.Capabilities.GetCapability("platformVersion");
		Assert.That(Version.TryParse(platformVersion, out var version), Is.True, "The iOS version must be available.");
		if (version!.Major < 26)
			Assert.Ignore("This regression concerns the iOS 26 navigation button.");

		App.SetOrientationPortrait();
		App.WaitForElement("Issue31936RootMarker");
		App.Tap("Issue31936OpenG4");
		App.WaitForElement("Issue31936G4Marker");
		App.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeNavigationBar//XCUIElementTypeStaticText[@name='G4']"));

		var window = App.WaitForElement(AppiumQuery.ByXPath("//XCUIElementTypeWindow")).GetRect();
		var backButtonQuery = AppiumQuery.ByXPath("//XCUIElementTypeNavigationBar//XCUIElementTypeButton[1]");
		var backButton = App.WaitForElement(backButtonQuery).GetRect();
		var offset = MeasureGlyphOffset(App.Screenshot(), window, backButton);

		Assert.That(Math.Abs(offset), Is.LessThanOrEqualTo(1),
			$"Issue31936 custom back glyph vertical center mismatch: offset={offset:F2} points, tolerance=1 point, frame={backButton}.");

		App.Tap(backButtonQuery);
		App.WaitForElement("Issue31936RootMarker");
	}

	static double MeasureGlyphOffset(byte[] screenshot, Rectangle window, Rectangle frame)
	{
		Assert.That(window.Width, Is.GreaterThan(0));
		Assert.That(window.Height, Is.GreaterThan(0));
		Assert.That(frame.Width, Is.GreaterThan(8));
		Assert.That(frame.Height, Is.GreaterThan(8));

		using var image = new MagickImage(screenshot);
		var scaleX = image.Width / (double)window.Width;
		var scaleY = image.Height / (double)window.Height;

		// Exclude the glass rim; only the white glyph on the dark navigation bar is ink.
		var left = Math.Max(0, (int)Math.Ceiling((frame.Left - window.Left + 4) * scaleX));
		var top = Math.Max(0, (int)Math.Ceiling((frame.Top - window.Top + 4) * scaleY));
		var right = Math.Min((int)image.Width, (int)Math.Floor((frame.Right - window.Left - 4) * scaleX));
		var bottom = Math.Min((int)image.Height, (int)Math.Floor((frame.Bottom - window.Top - 4) * scaleY));
		Assert.That(right, Is.GreaterThan(left));
		Assert.That(bottom, Is.GreaterThan(top));

		using var pixels = image.GetPixels();
		var count = 0;
		var sumY = 0d;
		var minY = bottom;
		var maxY = top;
		for (var y = top; y < bottom; y++)
		{
			for (var x = left; x < right; x++)
			{
				var color = pixels.GetPixel(x, y).ToColor();
				if (color is null || color.R < 230 || color.G < 230 || color.B < 230)
					continue;

				count++;
				sumY += y + 0.5;
				minY = Math.Min(minY, y);
				maxY = Math.Max(maxY, y);
			}
		}

		Assert.That(count, Is.GreaterThan(5), "The native back button must contain a rendered white glyph.");
		Assert.That(maxY - minY, Is.GreaterThan(2), "The glyph must span multiple pixel rows.");
		var centroid = sumY / count / scaleY + window.Top;
		return centroid - (frame.Top + frame.Height / 2d);
	}
}
#endif
