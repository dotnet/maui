#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Exercises the Android RecyclerView handler only
using System.Globalization;
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
	public void LargeGroupedGridLoadsWithinTimeout()
	{
		App.WaitForElement("LoadButton");
		App.Tap("LoadButton");

		Assert.That(App.WaitForTextToBePresentInElement("StatusLabel", "Loaded"), Is.True,
			"Assigning a large grouped ItemsSource did not finish in time.");
		App.WaitForElement("G1 Item 1");

		// Tap can return after the UI stall, so also assert the time measured inside the app.
		var timing = App.FindElement("TimingLabel").GetText()
			?? throw new AssertionException("The app did not report the ItemsSource load time.");
		var elapsedMilliseconds = long.Parse(timing, CultureInfo.InvariantCulture);
		Assert.That(elapsedMilliseconds, Is.LessThan(15000),
			"Assigning a large grouped ItemsSource blocked the UI thread for 15 seconds or more.");
	}
}
#endif
