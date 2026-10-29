#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19831 : _IssuesUITest
{
	public override string Issue => "[Android] Action mode menu doesn't disappear when switch on another tab";

	public Issue19831(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.ContextActions)]
	public void ActionModeMenuShouldNotBeVisibleAfterSwitchingTab()
	{
		_ = App.WaitForElement("Item1");

		// 1. Open a context menu.
		App.LongPress("Item1");

		// 2. Navigate to a different tab.
		App.Click("button");

		// 3. The test passes if the action mode menu is not visible.
		VerifyScreenshot();
	}
}

public class Issue19831Renderer : _IssuesUITest
{
	public override string Issue => "ShellRenderer title updates after switching tabs with context actions open";

	public Issue19831Renderer(TestDevice device)
		: base(device)
	{ }

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void PageTitleShouldUpdateAfterSwitchingTabWithActionModeOpen()
	{
		App.WaitForElement("Item1");
		App.LongPress("Item1");
		App.WaitForElement("View");
		App.Click("button");
		App.WaitForElement("Page 2");
		App.WaitForNoElement("View");
	}
}
#endif