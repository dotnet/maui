#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Exercises the Android RecyclerView handler only
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38925 : _IssuesUITest
{
	public Issue38925(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView with GridItemsLayout: SpacingItemDecoration walks all positions for every cell";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void LargeGroupedGridLoadsAndScrollsWithinTimeout()
	{
		App.WaitForElement("LoadButton");

		// Each step must complete within the default wait; before the fix every one of
		// these blocked the UI thread for tens of seconds with 10 000 grouped items.
		App.Tap("LoadButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Loaded"), Is.True,
			"Assigning a large grouped ItemsSource did not finish laying out in time.");

		App.Tap("ScrollToEndButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Scrolled to end"), Is.True,
			"ScrollTo the last group did not finish in time.");

		App.Tap("ScrollToStartButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Scrolled to start"), Is.True,
			"ScrollTo the first group did not finish in time.");
	}
}
#endif
