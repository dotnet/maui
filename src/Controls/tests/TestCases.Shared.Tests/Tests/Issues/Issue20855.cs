using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20855 : _IssuesUITest
	{

		public Issue20855(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Grouped CollectionView items not rendered properly on Android, works on Windows";

		[Test]
		[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
		[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on release/10.0.1xx-sr11; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
		public void GroupedCollectionViewItems()
		{
			App.WaitForElement("Item 1");
			VerifyScreenshot("GroupedCollectionViewItems");
		}
	}
}