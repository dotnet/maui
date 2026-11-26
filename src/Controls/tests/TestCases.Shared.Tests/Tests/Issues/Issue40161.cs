#if ANDROID || IOS // The existing background/resume helpers only support Android and iOS.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40161(TestDevice device) : _IssuesUITest(device)
{
	public override string Issue => "Shell root search appears on a pushed page after resuming";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ResumingPushedPageDoesNotRestoreRootSearchHandler()
	{
		App.WaitForElement("PushPage");
		var search = App.GetCollapsibleShellSearchQuery("Root search 40161");

		// Prove the native query recognizes root search before asserting its absence.
		App.WaitForElement(search);
		App.Tap("PushPage");
		App.WaitForElement("PopPage");
		App.WaitForNoElement(search);

		try
		{
			App.BackgroundApp();
		}
		finally
		{
			App.ForegroundApp();
		}

		App.WaitForElement("PopPage");
		App.WaitForNoElement(search, "Root search must not appear on the pushed page after resuming.");

		App.Tap("PopPage");
		App.WaitForElement("PushPage");
		App.WaitForElement(search, "Popping must restore the root page's native search UI.");
	}
}
#endif
