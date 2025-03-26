// FontImageSource color is not applied to the Tab Icon on Windows for the Tabbedpage 
// Issue: https://github.com/dotnet/maui/issues/26752
#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26662 : _IssuesUITest
	{
		public Issue26662(TestDevice device) : base(device) { }

		public override string Issue => "Unable to dynamically set unselected IconImageSource Color on Android";

		[Test, Order(1)]
		[Category(UITestCategories.TabbedPage)]
		public void FontImageSourceColorShouldApplyOnTabIcon()
		{
			App.WaitForElement("Button");
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Category(UITestCategories.TabbedPage)]
		public void DynamicFontImageSourceColorShouldApplyOnTabIcon()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}
#endif