#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35760 : _IssuesUITest
{
	public Issue35760(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell title updates after switching tabs with context actions open";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void TitleShouldUpdateAfterSwitchingTabWithContextActionsOpen()
	{
		App.WaitForElement("Message1");
		App.LongPress("Message1");
		App.WaitForElement("Archive");
		App.Click("GoToDetails");
		App.WaitForElement("DetailsContent");
		App.WaitForNoElement("Archive");
		VerifyScreenshot();
	}
}
#endif
