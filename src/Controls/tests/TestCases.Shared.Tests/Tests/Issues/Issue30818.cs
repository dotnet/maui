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
		[Category(UITestCategories.Navigation)]
		public void ToolbarItemColorWithCustomBarTextColorShouldWork()
		{
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