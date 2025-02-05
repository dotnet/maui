#if TEST_FAILS_ON_ANDROID // ToolBarItem not shown on FlyoutPage, issue: https://github.com/dotnet/maui/issues/26330
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2266 : _IssuesUITest
{
	public Issue2266(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting a different Detail page from a FlyoutPage after 2nd time on MainPage";

	[Test]
	[Category(UITestCategories.Navigation)]

	public void SwapMainPageWithFlyoutPages()
	{
		App.WaitForElement("FlyoutPage Navigation");
		App.Tap("FlyoutPage Navigation");
		App.Tap("OpenMaster");
		App.Tap("Page 1");
		App.Tap("START");
		App.Tap("FlyoutPage Navigation ->> Page 1");
		App.WaitForElement("Page 1");
		App.Tap("START");
		App.Tap("FlyoutPage Navigation ->> Page 2");
		App.WaitForElement("Page 2");
		App.Tap("START");
		App.Tap("FlyoutPage Navigation ->> Page 3");
		App.WaitForElement("Page 3");
	}
}
#endif