#if TEST_FAILS_ON_WINDOWS // When selecting a single item, the selection background is applied to all items.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20062 : _IssuesUITest
{
	public Issue20062(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView - SelectedItem visual state manager not working";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void CollectionViewSelectionChangesVisualState()
	{
		App.WaitForElement("CollectionView");
		App.WaitForElement("Item0");
		App.Tap("Item0");
		App.WaitForElement("Item2");
		App.Tap("Item2");
		VerifyScreenshot();
	}
}
#endif