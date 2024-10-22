using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11244 : _IssuesUITest
{
	public Issue11244(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] BackButtonBehavior no longer displays on the first routed page in 4.7";

	/*
	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnAndroid]
	[FailsOnIOS]
	public void LeftToolbarItemTextDisplaysWhenFlyoutIsDisabled()
	{
		App.WaitForElement("Logout");
	}
	*/
}