// Android opens ListView context actions with a long press, entering contextual action mode.
// iOS reveals actions with a swipe instead; this regression concerns Android's action-mode toolbar.
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
		// Match either toolbar title, excluding tab labels, so a stale title produces an assertion failure.
		var title = App.WaitForElement(AppiumQuery.ByXPath(
			"//android.widget.TextView[(@text='Messages' or @text='Details') and not(ancestor::android.widget.HorizontalScrollView)]"));
		Assert.That(title.GetText(), Is.EqualTo("Details"),
			"The toolbar title should update after switching tabs with context actions open.");
	}
}
#endif
