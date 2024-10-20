using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11247 : _IssuesUITest
{
	public Issue11247(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell FlyoutIsPresented not working if set in \"navigating\" handler";

	/*
	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnAndroid]
	public void SettingFlyoutIsPresentedInNavigatingKeepsFlyoutOpen()
	{
		App.Tap("CloseFlyout");
		ShowFlyout();
		App.Tap("FlyoutItem 1");
		App.Tap("FlyoutItem 2");
		App.WaitForElement("FlyoutItem 1");
		App.WaitForElement("FlyoutItem 2");

	}
	*/
}