using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38276 : _IssuesUITest
{
	public Issue38276(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView does not update its rendered height after ItemsSource changes";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewHeightUpdatesAfterItemsSourceChanges()
	{
		App.WaitForElement("LargeItemsButton");
		App.WaitForElement("SingleItemButton");
		var collectionView = App.WaitForElement("CollectionView");
		var largeItemsInitialHeight = collectionView.GetRect().Height;

		App.Tap("SingleItemButton");

		App.RetryAssert(() =>
		{
			var height = App.FindElement("CollectionView").GetRect().Height;
			Assert.That(
				height,
				Is.LessThan(largeItemsInitialHeight),
				"CollectionView should shrink after the first ItemsSource update.");
		});

		var singleItemHeight = App.FindElement("CollectionView").GetRect().Height;
		Assert.That(
			singleItemHeight,
			Is.LessThan(largeItemsInitialHeight),
			"CollectionView height should decrease when ItemsSource changes from 10 items to 1 item.");

		App.Tap("LargeItemsButton");

		App.RetryAssert(() =>
		{
			var height = App.FindElement("CollectionView").GetRect().Height;
			Assert.That(
				height,
				Is.GreaterThan(singleItemHeight),
				"CollectionView should grow after the second ItemsSource update.");
		});

		var largeItemsFinalHeight = App.FindElement("CollectionView").GetRect().Height;
		Assert.That(
			largeItemsFinalHeight,
			Is.GreaterThan(singleItemHeight),
			"CollectionView height should increase when ItemsSource changes from 1 item to 10 items.");
	}
}
