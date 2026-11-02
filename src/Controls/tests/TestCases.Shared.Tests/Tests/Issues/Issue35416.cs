using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35416 : _IssuesUITest
{
	public Issue35416(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Shell.FlyoutHeader background is incorrect";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 2)]
	public void FlyoutBackgroundShowsThroughFlyoutHeader()
	{
		App.WaitForElement("Issue35416Label");
		App.TapShellFlyoutIcon();
		App.WaitForElement("Issue35416FlyoutHeader");
		VerifyScreenshot();
	}
}
