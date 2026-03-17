#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue30818 : _IssuesUITest
	{
		public Issue30818(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ToolbarItem color with custom BarTextColor not working";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarItemColorWithCustomBarTextColorShouldWork()
		{
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/33970
			}
			App.WaitForElement("SetResetButton");
			VerifyScreenshot();

			// Wait for the page to load and toolbar item to be present
			App.Tap("SetRedButton");
			VerifyScreenshot("ToolbarItemColorWithCustomBarTextColorShouldWork_Red");
			
			// Set the BarTextColor to red
			App.WaitForElement("SetGreenButton");
			App.Tap("SetGreenButton");
			VerifyScreenshot("ToolbarItemColorWithCustomBarTextColorShouldWork_Green");


			App.Tap("SetResetButton");
			VerifyScreenshot();
		}
	}
}
#endif