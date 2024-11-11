using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue889 : _IssuesUITest
{
	public Issue889(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Assigning to FlyoutPage.Detail after construction doesn't work";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void Issue899TestsAppCrashWhenSwitchingTabs()
	{
		App.WaitForElement("PushPage");
		App.Tap("PushPage");
		App.WaitForElement("PushedPageLabel");

		App.TapBackArrow();

		App.WaitForElement("PushPage");
	}
}