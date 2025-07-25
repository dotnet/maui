#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Secondary ToolBar still not supported for Shell page of iOS and Catalyst Issue Link: https://github.com/dotnet/maui/issues/30674
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
internal class Issue20177 : _IssuesUITest
{
	public Issue20177(TestDevice device) : base(device) { }

	public override string Issue => "Shell TitleColor changes the secondary ToolbarItems TextColor";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolBarSecondayItemsShouldNotUseBarTextColor()
	{
		App.WaitForMoreButton();
		App.TapMoreButton();
		VerifyScreenshot();
	}
}
#endif