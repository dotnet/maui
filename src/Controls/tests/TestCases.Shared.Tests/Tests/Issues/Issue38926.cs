#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Exercises the Android RecyclerView handler only
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38926 : _IssuesUITest
{
	// Each jump takes ~100 ms with the fix; before it, 1.8–12.8 s on a fast device (200 000 items).
	static readonly TimeSpan ScrollBudget = TimeSpan.FromSeconds(5);

	public Issue38926(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView GridItemsLayout: span lookups over uncached positions are O(position) per cell";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void ScrollToColdRegionOfLargeGroupedGridIsFast()
	{
		App.WaitForElement("LoadGroupedButton");

		App.Tap("LoadGroupedButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Loaded grouped"), Is.True);

		// Each jump lands on positions GridLayoutManager has not cached yet. Before the fix every one
		// walked GetSpanSize from the nearest cached key, hundreds of thousands of JNI calls per jump.
		App.Tap("ScrollToEndButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Scrolled to end", ScrollBudget), Is.True,
			"ScrollTo the last group did not finish in time.");

		App.Tap("ScrollToMiddleButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Scrolled to middle", ScrollBudget), Is.True,
			"ScrollTo the middle group did not finish in time.");

		App.Tap("ScrollToStartButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Scrolled to start", ScrollBudget), Is.True,
			"ScrollTo the first group did not finish in time.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void ScrollToColdRegionOfLargeFlatGridIsFast()
	{
		App.WaitForElement("LoadFlatButton");

		App.Tap("LoadFlatButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Loaded flat"), Is.True);

		App.Tap("ScrollToEndButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Scrolled to end", ScrollBudget), Is.True,
			"ScrollTo the last item did not finish in time.");

		App.Tap("ScrollToMiddleButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Scrolled to middle", ScrollBudget), Is.True,
			"ScrollTo the middle item did not finish in time.");

		App.Tap("ScrollToStartButton");
		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Scrolled to start", ScrollBudget), Is.True,
			"ScrollTo the first item did not finish in time.");
	}
}
#endif
