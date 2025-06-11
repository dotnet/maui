#if ANDROID
//Since bottom tab placement is specific to Android, I enabled it only for Android: https://learn.microsoft.com/en-us/dotnet/maui/android/platform-specifics/tabbedpage-toolbar-placement?view=net-maui-9.0
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue29109 : _IssuesUITest
	{
		public Issue29109(TestDevice device) : base(device) { }

		public override string Issue => "[Android] Unable to set unselected iconImageSource color when toolbar placement is set to bottom";

		[Test, Order(1)]
		[Category(UITestCategories.TabbedPage)]
		public void FontImageSourceColorShouldApplyOnBottomTabIconOnAndroid()
		{
			App.WaitForElement("Button");
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Category(UITestCategories.TabbedPage)]
		public void DynamicFontImageSourceColorShouldApplyOnBottomTabIconOnAndroid()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}
#endif