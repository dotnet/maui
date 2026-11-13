#if TEST_FAILS_ON_WINDOWS // ScrollAnchorMode.End isn't implemented on Windows: the list doesn't follow its end there.
using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class CollectionViewAnchorModeEnd : _IssuesUITest
{
	const string ListId = "MessagesCollectionView";
	const string FooterId = "MessagesFooter";
	const string FooterHeightReferenceId = "FooterHeightReference";
	const string IsAnchoredId = "IsAnchoredLabel";
	const string AddId = "AddButton";
	const string GrowId = "GrowButton";
	const string ScrollToFirstId = "ScrollToFirstButton";
	const string JumpToLatestId = "JumpToLatestButton";

	// Device pixels on Android, points elsewhere: the anchor scrolls to the exact end, so this only absorbs rounding.
	const int Tolerance = 3;

	public CollectionViewAnchorModeEnd(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView AnchorMode End keeps the end in view";

	protected override bool ResetAfterEachTest => true;

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void FollowsTheEndAsItemsAreAddedAndTheLastItemGrows()
	{
		App.WaitForElement(ListId);
		Assert.That(WaitUntil(IsEndInView), Is.True, "The list should open at its end, with the footer in view.");
		Assert.That(App.WaitForTextEqualToElement(IsAnchoredId, "True"), Is.True, "The list should follow its end.");

		App.Tap(AddId);
		App.Tap(AddId);
		App.Tap(AddId);
		App.WaitForElement("Message33");
		Assert.That(WaitUntil(IsEndInView), Is.True, "New items should keep the end in view.");

		for (var i = 0; i < 5; i++)
		{
			App.Tap(GrowId);
		}

		Assert.That(WaitUntil(IsEndInView), Is.True, "The last item growing should keep the end in view.");
		Assert.That(App.WaitForTextEqualToElement(IsAnchoredId, "True"), Is.True, "The list should still follow its end.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void ScrollingAwayStopsFollowingAndScrollingBackToTheEndResumes()
	{
		App.WaitForElement(ListId);
		Assert.That(WaitUntil(IsEndInView), Is.True, "The list should open at its end.");

		App.ScrollUp(ListId, ScrollStrategy.Gesture, swipePercentage: 0.6);
		Assert.That(App.WaitForTextEqualToElement(IsAnchoredId, "False"), Is.True, "Scrolling away from the end should stop following it.");

		App.Tap(AddId);
		App.Tap(AddId);
		Assert.That(WaitUntil(() => !IsEndInView()), Is.True, "New items shouldn't pull the list back to the end while the user reads.");
		Assert.That(App.FindElements("Message32"), Is.Empty, "The newest item shouldn't be scrolled into view.");

		for (var i = 0; i < 8 && !App.WaitForTextEqualToElement(IsAnchoredId, "True", TimeSpan.FromSeconds(1)); i++)
		{
			App.ScrollDown(ListId, ScrollStrategy.Gesture, swipePercentage: 0.9);
		}

		Assert.That(App.WaitForTextEqualToElement(IsAnchoredId, "True"), Is.True, "Scrolling back to the end should follow it again.");
		Assert.That(WaitUntil(IsEndInView), Is.True, "The end should be in view after scrolling back to it.");

		App.Tap(AddId);
		App.WaitForElement("Message33");
		Assert.That(WaitUntil(IsEndInView), Is.True, "New items should keep the end in view again.");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void ScrollToAwayFromTheEndStopsFollowingAndScrollToEndResumes()
	{
		App.WaitForElement(ListId);
		Assert.That(WaitUntil(IsEndInView), Is.True, "The list should open at its end.");

		App.Tap(ScrollToFirstId);
		App.WaitForElement("Message1");
		Assert.That(App.WaitForTextEqualToElement(IsAnchoredId, "False"), Is.True, "ScrollTo away from the end should stop following it.");

		App.Tap(AddId);
		Assert.That(App.WaitForTextEqualToElement(IsAnchoredId, "False"), Is.True, "The list should still not follow its end.");
		App.WaitForElement("Message1");
		Assert.That(App.FindElements(FooterId), Is.Empty, "New items shouldn't pull the list back to the end.");

		App.Tap(JumpToLatestId);
		Assert.That(App.WaitForTextEqualToElement(IsAnchoredId, "True"), Is.True, "Scrolling to the end should follow it again.");
		Assert.That(WaitUntil(IsEndInView), Is.True, "The end should be in view, footer included.");

		App.Tap(AddId);
		App.WaitForElement("Message32");
		Assert.That(WaitUntil(IsEndInView), Is.True, "New items should keep the end in view again.");
	}

	// The footer ends the content: the end is in view when the footer is wholly on screen at the bottom of the list.
	bool IsEndInView()
	{
		var footer = App.FindElements(FooterId).FirstOrDefault();
		if (footer is null)
		{
			return false;
		}

		var footerRect = footer.GetRect();
		var listRect = App.WaitForElement(ListId).GetRect();
		var referenceRect = App.WaitForElement(FooterHeightReferenceId).GetRect();
		return Math.Abs(Bottom(footerRect) - Bottom(listRect)) <= Tolerance
			&& footerRect.Height >= referenceRect.Height - Tolerance;
	}

	static int Bottom(Rectangle rect) => rect.Y + rect.Height;

	static bool WaitUntil(Func<bool> condition)
	{
		var timeout = DateTime.Now.AddSeconds(10);
		while (!condition())
		{
			if (DateTime.Now > timeout)
			{
				return false;
			}

			Thread.Sleep(250);
		}

		return true;
	}
}
#endif
