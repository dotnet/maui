using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22452 : _IssuesUITest
{
	public Issue22452(TestDevice device) : base(device) { }

	public override string Issue => "Fix error when running new template maui app on iOS";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMac]
	public void NavigatingBetweenFlyoutItems()
	{
		App.WaitForElement("TapTwoPage");
		App.Tap("FlyoutItem1");
		App.Tap("FlyoutItem2");
		VerifyScreenshot();
	}
}