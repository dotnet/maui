using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31936 : _IssuesUITest
	{
		public override string Issue => "Back button glyph (FontImageSource) is not vertically centered on iOS 26";

		public Issue31936(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Shell)]
		public void BackButtonFontImageSourceShouldBeVerticallyCentered()
		{
			// Wait for the page to load
			App.WaitForElement("StatusLabel");

			// Take a screenshot to visually verify the back button is centered
			// The back button with FontImageSource glyph should be vertically centered in the navigation bar
			// This is particularly important on iOS 26 where font metrics changed
			VerifyScreenshot();
		}
	}
}
