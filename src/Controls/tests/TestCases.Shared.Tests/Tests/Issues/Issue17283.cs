using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue17283 : _IssuesUITest
	{
		public override string Issue => "[Android] CarouselView doesn't scroll to the right Position after changing the ItemSource with Loop enabled";

		public Issue17283(TestDevice testDevice) : base(testDevice) { }

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Currently fails on Windows; see https://github.com/dotnet/maui/issues/24482")]
		[FailsOnMacWhenRunningOnXamarinUITest("VerifyScreenshot is not implemented on Mac")]
		public void CarouselViewShouldScrollToRightPosition()
		{
			App.WaitForElement("goToLastItemButton");
			App.Click("goToLastItemButton");
			App.WaitForElement("5");
			App.Click("reloadItemsButton");
			App.WaitForElement("5last");
			VerifyScreenshot();
		}
	}
}