#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38276_Grouped : _IssuesUITest
{
	public Issue38276_Grouped(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Grouped CollectionView does not update its rendered height after ItemsSource changes";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void GroupedCollectionViewHeightUpdatesWithHeaderAndFooterTemplates()
	{
		App.WaitForElement("LargeGroupsButton");
		App.WaitForElement("SmallGroupsButton");
		App.WaitForElement("Group 1 header");
		App.WaitForElement("Group 1 footer");
		var largeGroupsInitialHeight = App.WaitForElement("GroupedCollectionView").GetRect().Height;

		App.Tap("SmallGroupsButton");

		App.WaitForElement("Group 1 header");
		App.WaitForElement("Group 1 footer");
		App.RetryAssert(() =>
		{
			var height = App.FindElement("GroupedCollectionView").GetRect().Height;
			Assert.That(
				height,
				Is.LessThan(largeGroupsInitialHeight),
				"Grouped CollectionView should shrink when ItemsSource changes to one group with one item.");
		});

		var smallGroupHeight = App.FindElement("GroupedCollectionView").GetRect().Height;

		App.Tap("LargeGroupsButton");

		App.RetryAssert(() =>
		{
			var height = App.FindElement("GroupedCollectionView").GetRect().Height;
			Assert.That(
				height,
				Is.GreaterThan(smallGroupHeight),
				"Grouped CollectionView should grow when ItemsSource changes back to multiple populated groups.");
		});
	}
}
#endif