#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Secondary ToolBar still not supported for Shell page of iOS and Catalyst Issue Link: https://github.com/dotnet/maui/issues/30674
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6127 : _IssuesUITest
{

	public Issue6127(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] ToolbarItem Order property ignored";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void Issue6127Test()
	{
		//AutomationId for ToolBarIcon is not works in android
#if ANDROID
		App.WaitForElement(AppiumQuery.ByXPath("//android.widget.Button[@content-desc='PrimaryToolbarIcon']"));
#else
		App.WaitForElement("PrimaryToolbarIcon");
#endif
		App.TapMoreButton();
		App.WaitForElement("Coffee");
	}
}
#endif