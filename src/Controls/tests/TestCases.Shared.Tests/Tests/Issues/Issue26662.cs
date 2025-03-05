//The issues on Android, iOS, and Mac have been resolved with the following PR:
//https://github.com/dotnet/maui/pull/26757. Once it is merged, we can remove the test failure condition.
#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
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