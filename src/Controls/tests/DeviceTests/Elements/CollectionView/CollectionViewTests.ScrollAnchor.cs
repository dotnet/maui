#if ANDROID || IOS || MACCATALYST
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
#if IOS || MACCATALYST
using CollectionViewHandler = Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2;
#else
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Handlers.Items;
#endif
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests
	{
		// Device-independent units: the anchor scrolls to the exact end, so this only absorbs rounding to device pixels.
		const double AnchorTolerance = 1;

		[Fact(DisplayName = "AnchorMode End shows the end when the list appears")]
		public async Task AnchorModeEndShowsTheEndWhenTheListAppears()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at the end of its content, footer included.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "AnchorMode End follows new items and a growing last item")]
		public async Task AnchorModeEndFollowsNewItemsAndAGrowingLastItem()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				for (var i = 0; i < 3; i++)
				{
					items.Add(new AnchorItem($"Added {i}", 40));
				}

				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 40) <= AnchorTolerance, timeout: 3000, message: "The new items should be laid out in view.");
				await AssertAtEnd(handler, "New items should keep the end in view.");

				items[^1].Height = 160;

				// Until the platform lays the item out at its new size, the view still shows the end it had.
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 160) <= AnchorTolerance, timeout: 3000, message: "The last item should grow.");
				await AssertAtEnd(handler, "The last item growing should keep the end in view.");

				// The view shows the end it had until the platform lays out the removal.
				items.RemoveAt(items.Count - 1);
				await AssertEventually(() => !ShowsItemOfHeight(handler, 160), timeout: 3000, message: "The removal should be laid out.");
				await AssertAtEnd(handler, "Removing the last item should keep the end in view.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "AnchorMode End follows a grouped list with a header and footer")]
		public async Task AnchorModeEndFollowsAGroupedList()
		{
			SetupBuilder();
			var groups = new ObservableCollection<AnchorGroup>(
				Enumerable.Range(0, 4).Select(group => new AnchorGroup($"Group {group}", CreateAnchorItems(8))));
			var collectionView = CreateAnchorCollectionView(groups, ScrollAnchorMode.End);
			collectionView.IsGrouped = true;
			collectionView.GroupHeaderTemplate = CreateAnchorGroupHeaderTemplate();

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The grouped list should open at its end.");

				groups[^1].Add(new AnchorItem("Added to the last group", 40));
				await AssertEventually(() => Math.Abs(GetGroupedItemHeight(handler, groups, groups.Count - 1, groups[^1].Count - 1) - 40) <= AnchorTolerance, timeout: 3000, message: "The added item should be laid out in view.");
				await AssertAtEnd(handler, "An item added to the last group should keep the end in view.");

				groups.Add(new AnchorGroup("New group", CreateAnchorItems(3)));
				await AssertEventually(() => Math.Abs(GetGroupedItemHeight(handler, groups, groups.Count - 1, 2) - 40) <= AnchorTolerance, timeout: 3000, message: "The new group should be laid out in view.");
				await AssertAtEnd(handler, "A new group should keep the end in view.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "ScrollToEnd during an animated ScrollTo in a grouped list stays at the end")]
		public async Task ScrollToEndDuringAnAnimatedScrollToInAGroupedListStaysAtTheEnd()
		{
			SetupBuilder();
			var groups = new ObservableCollection<AnchorGroup>(
				Enumerable.Range(0, 4).Select(group => new AnchorGroup($"Group {group}", CreateAnchorItems(8))));
			var collectionView = CreateAnchorCollectionView(groups, ScrollAnchorMode.End);
			collectionView.IsGrouped = true;
			collectionView.GroupHeaderTemplate = CreateAnchorGroupHeaderTemplate();

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The grouped list should open at its end.");

				// The newer request wins, also once its animation ends.
				collectionView.ScrollTo(0, groupIndex: 0, animate: true);
				await Task.Delay(100);
				collectionView.ScrollToEnd(animate: true);
				await Task.Delay(1500);

				await AssertAtEnd(handler, "ScrollToEnd should stay at the end once its animation ends.");
				Assert.True(collectionView.IsAnchored, "ScrollToEnd should follow the end.");
			});
		}

		[Theory(DisplayName = "An animated ScrollTo that finds nothing leaves ScrollToEnd reaching the end of a grouped list")]
		[InlineData(ScrollToPosition.End)]
		[InlineData(ScrollToPosition.MakeVisible)]
		public async Task AnAnimatedScrollToThatFindsNothingLeavesScrollToEndReachingTheEndOfAGroupedList(ScrollToPosition position)
		{
			SetupBuilder();
			var groups = new ObservableCollection<AnchorGroup>(
				Enumerable.Range(0, 4).Select(group => new AnchorGroup($"Group {group}", CreateAnchorItems(8))));
			var collectionView = CreateAnchorCollectionView(groups, ScrollAnchorMode.None);
			collectionView.IsGrouped = true;
			collectionView.GroupHeaderTemplate = CreateAnchorGroupHeaderTemplate();

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// The view doesn't scroll for an item it doesn't have: ScrollToEnd still goes the rest of the way once its
				// animation ends, as measuring the rows it scrolls past moves the end.
				collectionView.ScrollToEnd(animate: true);
				collectionView.ScrollTo(new AnchorItem("Not in the list", 40), position: position, animate: true);

				await AssertAtEnd(handler, "ScrollToEnd should reach the end once its animation ends.", timeout: 5000);
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "ScrollToStart or ScrollToEnd from Scrolled as a ScrollTo scrolls a grouped list wins")]
		[InlineData(ScrollToPosition.End, ScrollAnchorMode.Start)]
		[InlineData(ScrollToPosition.MakeVisible, ScrollAnchorMode.Start)]
		[InlineData(ScrollToPosition.End, ScrollAnchorMode.End)]
		[InlineData(ScrollToPosition.MakeVisible, ScrollAnchorMode.End)]
		public async Task ScrollingToTheEdgeFromScrolledAsAScrollToScrollsAGroupedListWins(ScrollToPosition position, ScrollAnchorMode edge)
		{
			SetupBuilder();
			var groups = new ObservableCollection<AnchorGroup>(
				Enumerable.Range(0, 4).Select(group => new AnchorGroup($"Group {group}", CreateAnchorItems(8))));

			// Scrolling to the start of a list that doesn't follow it; to the end of one that does, which keeps it in view as
			// the rows above are measured.
			var anchorMode = edge == ScrollAnchorMode.End ? ScrollAnchorMode.End : ScrollAnchorMode.None;
			var collectionView = CreateAnchorCollectionView(groups, anchorMode);
			collectionView.IsGrouped = true;
			collectionView.GroupHeaderTemplate = CreateAnchorGroupHeaderTemplate();

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (edge == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "The list should open at its end.");
				}
				else
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				}

				// The app asks for the edge as soon as the ScrollTo scrolls the list: the newer request wins, also over a grouped
				// list scrolling to the item again once the rows it brought into view are measured.
				var scrolledToEdge = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!scrolledToEdge)
					{
						scrolledToEdge = true;
						if (edge == ScrollAnchorMode.End)
						{
							collectionView.ScrollToEnd(animate: false);
						}
						else
						{
							collectionView.ScrollToStart(animate: false);
						}
					}
				};

				collectionView.ScrollTo(2, groupIndex: 1, position: position, animate: false);
				await AssertEventually(() => scrolledToEdge, timeout: 3000, message: "The ScrollTo should scroll the list.");

				// And a row in view changes size: the item isn't put back where the ScrollTo put it as the list is laid out again.
				var groupIndex = edge == ScrollAnchorMode.End ? groups.Count - 1 : 0;
				var index = edge == ScrollAnchorMode.End ? groups[groupIndex].Count - 1 : 0;
				groups[groupIndex][index].Height = 80;
				var laidOut = await Wait(() => Math.Abs(GetGroupedItemHeight(handler, groups, groupIndex, index) - 80) <= AnchorTolerance, 3000);
				await Task.Delay(500);
				if (edge == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "The later ScrollToEnd should win.");
					Assert.True(collectionView.IsAnchored, "ScrollToEnd should follow the end.");
				}
				else
				{
					Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The later ScrollToStart should win. Distance from the start: {GetDistanceFromStart(handler)}.");
					Assert.False(collectionView.IsAnchored);
				}

				Assert.True(laidOut, "The row should be laid out at its new size.");
			});
		}

		[Theory(DisplayName = "AnchorMode End keeps the end in view when the header or footer is replaced")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task AnchorModeEndKeepsTheEndWhenTheHeaderOrFooterIsReplaced(bool replaceHeader)
		{
			SetupBuilder();

			// Every row the same height: the content is the size it was when the platform lays the list out again from its
			// start, so only the move tells that it did.
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);
			collectionView.Header = new Label { Text = "Header", HeightRequest = 40 };
			collectionView.Footer = new Label { Text = "Footer", HeightRequest = 40 };

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				if (replaceHeader)
				{
					collectionView.Header = new Label { Text = "New header", HeightRequest = 40 };
				}
				else
				{
					collectionView.Footer = new Label { Text = "New footer", HeightRequest = 40 };
				}

				await AssertAtEnd(handler, "Replacing the header or footer should keep the end in view.");
				Assert.True(collectionView.IsAnchored, "Replacing the header or footer shouldn't stop following the end.");
			});
		}

		[Fact(DisplayName = "A new group template doesn't follow the end of a list that stopped following it")]
		public async Task ANewGroupTemplateDoesNotFollowTheEndAgain()
		{
			SetupBuilder();
			var groups = new ObservableCollection<AnchorGroup>(
				Enumerable.Range(0, 4).Select(group => new AnchorGroup($"Group {group}", CreateAnchorItems(8))));
			var collectionView = CreateAnchorCollectionView(groups, ScrollAnchorMode.End);
			collectionView.IsGrouped = true;
			collectionView.GroupHeaderTemplate = CreateAnchorGroupHeaderTemplate();

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The grouped list should open at its end.");

				collectionView.ScrollTo(0, groupIndex: 0, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "ScrollTo away from the end should stop following it.");

				// The items are the same: the list keeps not following its end.
				collectionView.GroupHeaderTemplate = CreateAnchorGroupHeaderTemplate();
				Assert.False(collectionView.IsAnchored, "A new group header template shouldn't follow the end again.");

				await Task.Delay(500);
				Assert.False(collectionView.IsAnchored, "A new group header template shouldn't follow the end again.");
				Assert.True(GetDistanceToEnd(handler) > AnchorTolerance, "A new group header template shouldn't scroll the list to its end.");
			});
		}

		[Theory(DisplayName = "ScrollTo away from the end stops following it, and ScrollToEnd follows it again")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ScrollToEndFollowsTheEndAgain(bool animate)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "ScrollTo away from the end should stop following it.");
				await Task.Delay(300);
				var position = GetScrollPosition(handler);

				items.Add(new AnchorItem("Added while reading", 40));
				await Task.Delay(500);
				Assert.True(position == GetScrollPosition(handler), "A new item shouldn't move a list that doesn't follow its end.");
				Assert.False(collectionView.IsAnchored);

				// The view follows the end from the moment it's asked to, all the way there: a "jump to latest" button that
				// shows while it doesn't would otherwise flash on the way.
				var anchoredChanges = new List<bool>();
				collectionView.PropertyChanged += (_, e) =>
				{
					if (e.PropertyName == StructuredItemsView.IsAnchoredProperty.PropertyName)
					{
						anchoredChanges.Add(collectionView.IsAnchored);
					}
				};

				collectionView.ScrollToEnd(animate);
				Assert.True(collectionView.IsAnchored, "ScrollToEnd should follow the end right away.");
				await AssertAtEnd(handler, "ScrollToEnd should reach the end of the content, footer included.", timeout: 5000);
				Assert.True(collectionView.IsAnchored, "ScrollToEnd should follow the end again.");
				Assert.DoesNotContain(false, anchoredChanges);

				items.Add(new AnchorItem("Added after jumping", 40));
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 40) <= AnchorTolerance, timeout: 3000, message: "The new item should be laid out in view.");
				await AssertAtEnd(handler, "New items should keep the end in view again.");
				Assert.True(collectionView.IsAnchored, "The list should still follow its end.");
			});
		}

		// A list that turns vertical starts over, as a new one does, when its new layout replaces the old one: a ScrollTo the
		// app requests right after, in the same update, is newer, and judged by where it lands. One requested right before was
		// for the old layout, and the list goes to its end.
		[Theory(DisplayName = "A ScrollTo in the same update as a horizontal list turning vertical wins when it comes after")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AScrollToInTheSameUpdateAsAHorizontalListTurningVerticalWinsWhenItComesAfter(bool scrollFirst)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);
			collectionView.ItemsLayout = LinearItemsLayout.Horizontal;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// Horizontal, the list isn't anchored. Settled: laid out where it is.
				var laidOut = await Wait(() => GetHorizontalDistanceToEnd(handler) > 100, 3000);
				Assert.True(laidOut, $"The items should extend past the end of the list. Distance to the end: {GetHorizontalDistanceToEnd(handler)}.");
				await Task.Delay(300);
				Assert.False(collectionView.IsAnchored, "A horizontal list shouldn't follow its end.");

				if (scrollFirst)
				{
					collectionView.ScrollTo(10, position: ScrollToPosition.Start, animate: false);
					collectionView.ItemsLayout = LinearItemsLayout.Vertical;

					await AssertAtEnd(handler, "The new layout should show the end, as a new list does.");
					await Task.Delay(500);
					await AssertAtEnd(handler, "The list should stay at its end.");
					Assert.True(collectionView.IsAnchored, "The list should follow its end.");
					return;
				}

				collectionView.ItemsLayout = LinearItemsLayout.Vertical;
				collectionView.ScrollTo(10, position: ScrollToPosition.Start, animate: false);

				var landed = await Wait(() => !double.IsNaN(GetItemHeight(handler, 10)), 3000);
				Assert.True(landed, $"ScrollTo should bring item 10 into view. Position: {GetScrollPosition(handler)}, distance to the end: {GetDistanceToEnd(handler)}, anchored: {collectionView.IsAnchored}.");
				await Task.Delay(500);
				Assert.False(double.IsNaN(GetItemHeight(handler, 10)), $"The list should stay at item 10, not go to the end the new layout shows. Position: {GetScrollPosition(handler)}.");
				Assert.True(GetDistanceToEnd(handler) > 100, $"The list shouldn't be at its end. Distance to the end: {GetDistanceToEnd(handler)}.");
				Assert.False(collectionView.IsAnchored, "Away from the end, the list shouldn't follow it.");
			});
		}

		[Theory(DisplayName = "A ScrollTo that finds nothing to scroll to keeps following the end")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AScrollToThatFindsNothingKeepsFollowingTheEnd(bool byIndex)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The item isn't in the list, or the index is past its end: the view doesn't scroll, so there's no request to
				// judge.
				if (byIndex)
				{
					collectionView.ScrollTo(items.Count + 100, animate: false);
				}
				else
				{
					collectionView.ScrollTo(new AnchorItem("Not in the list", 40), animate: false);
				}

				items.Add(new AnchorItem("Added", 40));

				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 40) <= AnchorTolerance, timeout: 3000, message: "The new item should be laid out in view.");
				await AssertAtEnd(handler, "The new item should keep the end in view.");
				Assert.True(collectionView.IsAnchored, "The list should still follow its end.");
			});
		}

		// An index the items don't have: past the last item, or before the first, where Android's adapter has the footer or
		// header.
		[Theory(DisplayName = "A ScrollTo for an index the items don't have right after ScrollToEnd leaves it reaching the end")]
		[InlineData(10000, 0, 0)]
		[InlineData(1, 0, 600)]
		[InlineData(-1, 50, 0)]
		public async Task AScrollToForAnIndexTheItemsDoNotHaveRightAfterScrollToEndLeavesItReachingTheEnd(int index, int headerHeight, int footerHeight)
		{
			SetupBuilder();

			// A last row taller than the view, the footer or the one item: ScrollToEnd goes the rest of the way once that row is
			// laid out where it scrolls to.
			var items = new ObservableCollection<AnchorItem> { new("Item", footerHeight > 0 ? 40 : 600) };
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);
			collectionView.Header = headerHeight > 0 ? new Label { Text = "Header", HeightRequest = headerHeight } : null;
			collectionView.Footer = footerHeight > 0 ? new Label { Text = "Footer", HeightRequest = footerHeight } : null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// The list doesn't have the index: the view doesn't scroll for it.
				collectionView.ScrollToEnd(animate: false);
				collectionView.ScrollTo(index, animate: false);

				await AssertAtEnd(handler, "ScrollToEnd should still reach the end.");
			});
		}

		[Theory(DisplayName = "ScrollToEnd without AnchorMode scrolls past the footer once")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ScrollToEndWithoutAnchorModeScrollsToTheEndOnce(bool animate)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// The rows on the way are measured as the view scrolls past them, which moves the end.
				collectionView.ScrollToEnd(animate);
				await AssertAtEnd(handler, "ScrollToEnd should reach the end of the content, footer included.", timeout: 5000);
				Assert.False(collectionView.IsAnchored);

				items.Add(new AnchorItem("Added", 40));
				await Task.Delay(500);
				Assert.True(GetDistanceToEnd(handler) > AnchorTolerance, "Without AnchorMode, the list shouldn't follow its end.");
			});
		}

		[Fact(DisplayName = "ScrollToEnd without AnchorMode reaches the end of a row measured in a later layout")]
		public async Task ScrollToEndWithoutAnchorModeReachesTheEndOfARowMeasuredInALaterLayout()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// It reaches the end once the row is measured. As the row changes, UIKit can lay the rows above the view out
				// again at their estimated size and measure them once more in a layout of its own, such as when accessibility is
				// on: that moves the end away from a list that doesn't follow it, later.
				var reachedEnd = false;
				bool ShowsEndOfMeasuredRow() =>
					Math.Abs(GetItemHeight(handler, items.Count - 1) - 120) <= AnchorTolerance && Math.Abs(GetDistanceToEnd(handler)) <= AnchorTolerance;
				collectionView.Scrolled += (_, _) => reachedEnd |= ShowsEndOfMeasuredRow();

				// The platform measures the last row again in a layout after the ones the scroll lays out right away, as UIKit
				// can for the rows a scroll brings into view.
				collectionView.ScrollToEnd(animate: false);
				items[^1].Height = 120;

				var reached = await Wait(() => reachedEnd || ShowsEndOfMeasuredRow(), 3000);
				Assert.True(reached, $"ScrollToEnd should reach the end of the rows it scrolled to, as measured. Distance to the end: {GetDistanceToEnd(handler)}. Last row: {GetItemHeight(handler, items.Count - 1)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		// On Android the animated scroll starts once the view is laid out, so a row the app grows right after the call, in the
		// same update, is part of where it goes. iOS and Mac Catalyst lay the view out as ScrollToEnd is called, and follow a
		// row grown after it only while UIKit measures the rows the scroll brought into view.
		[Theory(DisplayName = "An animated ScrollToEnd without AnchorMode reaches the end of a last row that grows in the same update")]
		[InlineData(true)]
#if ANDROID
		[InlineData(false)]
#endif
		public async Task AnAnimatedScrollToEndWithoutAnchorModeReachesTheEndOfALastRowThatGrowsInTheSameUpdate(bool growFirst)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// At the end, the last rows are in view as the scroll starts: how far it goes is measured from them.
				collectionView.ScrollToEnd(animate: false);
				await AssertAtEnd(handler, "ScrollToEnd should reach the end.");

				// It reaches the end of the row once the row has grown. As the row changes, UIKit can lay the rows above the view
				// out again at their estimated size and measure them once more in a layout of its own: that moves the end away
				// from a list that doesn't follow it, later.
				var reachedEnd = false;
				bool ShowsEndOfGrownRow() =>
					Math.Abs(GetItemHeight(handler, items.Count - 1) - 120) <= AnchorTolerance && Math.Abs(GetDistanceToEnd(handler)) <= AnchorTolerance;
				collectionView.Scrolled += (_, _) => reachedEnd |= ShowsEndOfGrownRow();

				if (growFirst)
				{
					items[^1].Height = 120;
					collectionView.ScrollToEnd(animate: true);
				}
				else
				{
					collectionView.ScrollToEnd(animate: true);
					items[^1].Height = 120;
				}

				var reached = await Wait(() => reachedEnd || ShowsEndOfGrownRow(), 3000);
				Assert.True(reached, $"ScrollToEnd should reach the end of the row as it grew. Distance to the end: {GetDistanceToEnd(handler)}. Last row: {GetItemHeight(handler, items.Count - 1)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		// A list that scrolls horizontally scrolls to its first or last item, by the item's index in its group.
		[Fact(DisplayName = "ScrollToStart, ScrollToEnd and ScrollTo an item of a group scroll a horizontal list of groups without headers to its edges")]
		public async Task ScrollToStartScrollToEndAndScrollToAnItemOfAGroupScrollAHorizontalListOfGroupsWithoutHeadersToItsEdges()
		{
			SetupBuilder();

			// A single item at either edge, and an empty group after the last.
			var groups = new List<List<string>>
			{
				new() { "First" },
				new(Enumerable.Range(0, 10).Select(index => $"Item {index}")),
				new() { "Last" },
				new(),
			};
			var collectionView = CreateHorizontalGroupedCollectionView(groups, groupHeaders: false);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// Until the list is laid out, it has no content to scroll, and is at both of its edges.
				var distance = double.NaN;
				var laidOut = await Wait(() => (distance = GetHorizontalDistanceToEnd(handler)) > 100, 3000);
				Assert.True(laidOut, $"The items should extend past the end of the list. Distance to the end: {distance}.");

				collectionView.ScrollToEnd(animate: false);
				var atEnd = await Wait(() => Math.Abs(distance = GetHorizontalDistanceToEnd(handler)) <= AnchorTolerance, 3000);
				Assert.True(atEnd, $"ScrollToEnd should show the last item at the end. Distance to the end: {distance}.");

				collectionView.ScrollToStart(animate: false);
				var atStart = await Wait(() => Math.Abs(distance = GetHorizontalDistanceFromStart(handler)) <= AnchorTolerance, 3000);
				Assert.True(atStart, $"ScrollToStart should show the first item at the start. Distance from the start: {distance}.");

				// ScrollTo counts only a group's items: the only item of a group without a header is its first.
				collectionView.ScrollTo(0, groupIndex: 2, position: ScrollToPosition.End, animate: false);
				atEnd = await Wait(() => Math.Abs(distance = GetHorizontalDistanceToEnd(handler)) <= AnchorTolerance, 3000);
				Assert.True(atEnd, $"ScrollTo should show the item of the last group with items at the end. Distance to the end: {distance}.");

				collectionView.ScrollTo(0, groupIndex: 0, position: ScrollToPosition.Start, animate: false);
				atStart = await Wait(() => Math.Abs(distance = GetHorizontalDistanceFromStart(handler)) <= AnchorTolerance, 3000);
				Assert.True(atStart, $"ScrollTo should show the item of the first group at the start. Distance from the start: {distance}.");
			});
		}

		[Fact(DisplayName = "ScrollToEnd and ScrollTo an item of a group scroll a horizontal list of groups to that item, not an equal one before it")]
		public async Task ScrollToEndAndScrollToAnItemOfAGroupScrollAHorizontalListOfGroupsToThatItemNotAnEqualOneBeforeIt()
		{
			SetupBuilder();
			var groups = new List<List<string>>
			{
				new() { "Same" },
				new(Enumerable.Range(0, 10).Select(index => $"Item {index}")),
				new() { "Same" },
			};
			var collectionView = CreateHorizontalGroupedCollectionView(groups, groupHeaders: true);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// Until the list is laid out, it has no content to scroll, and is at both of its edges.
				var distance = double.NaN;
				var laidOut = await Wait(() => (distance = GetHorizontalDistanceToEnd(handler)) > 100, 3000);
				Assert.True(laidOut, $"The items should extend past the end of the list. Distance to the end: {distance}.");

				collectionView.ScrollToEnd(animate: false);
				var atEnd = await Wait(() => Math.Abs(distance = GetHorizontalDistanceToEnd(handler)) <= AnchorTolerance, 3000);
				Assert.True(atEnd, $"ScrollToEnd should show the last item at the end. Distance to the end: {distance}.");

				// Back to the first group, whose item is in view and equal to the last group's.
				collectionView.ScrollToStart(animate: false);
				var awayFromEnd = await Wait(() => (distance = GetHorizontalDistanceToEnd(handler)) > 100, 3000);
				Assert.True(awayFromEnd, $"ScrollToStart should show the first item. Distance to the end: {distance}.");

				collectionView.ScrollTo(0, groupIndex: 2, position: ScrollToPosition.End, animate: false);
				atEnd = await Wait(() => Math.Abs(distance = GetHorizontalDistanceToEnd(handler)) <= AnchorTolerance, 3000);
				Assert.True(atEnd, $"ScrollTo should show the item of the last group at the end, not the equal one of the first. Distance to the end: {distance}.");
			});
		}

		// The list shows the items a sequence that can be read only once gave it: its edges are where those are.
		[Theory(DisplayName = "ScrollToStart and ScrollToEnd scroll a horizontal list of a sequence read once to its edges")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ScrollToStartAndScrollToEndScrollAHorizontalListOfASequenceReadOnceToItsEdges(bool animate)
		{
			SetupBuilder();
			var collectionView = new CollectionView
			{
				ItemsLayout = LinearItemsLayout.Horizontal,
				HeightRequest = 100,
				WidthRequest = 300,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { WidthRequest = 100 };
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// Set once the list has its layout and template, so that the list reads it once.
				collectionView.ItemsSource = new ReadOnceSequence(Enumerable.Range(0, 20).Select(index => $"Item {index}"));
				await AssertEventually(() => GetPlatformItemCount(handler) == 20, timeout: 3000, message: "The list should show the items the sequence gave it.");
				await AssertEventually(() => GetHorizontalDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				// Until the list is laid out, it has no content to scroll, and is at both of its edges.
				var distance = double.NaN;
				var laidOut = await Wait(() => (distance = GetHorizontalDistanceToEnd(handler)) > 100, 3000);
				Assert.True(laidOut, $"The items should extend past the end of the list. Distance to the end: {distance}.");

				collectionView.ScrollToEnd(animate);
				var atEnd = await Wait(() => Math.Abs(distance = GetHorizontalDistanceToEnd(handler)) <= AnchorTolerance, 3000);
				Assert.True(atEnd, $"ScrollToEnd should show the last item the list shows at the end. Distance to the end: {distance}.");

				collectionView.ScrollToStart(animate);
				var atStart = await Wait(() => Math.Abs(distance = GetHorizontalDistanceFromStart(handler)) <= AnchorTolerance, 3000);
				Assert.True(atStart, $"ScrollToStart should show the first item the list shows at the start. Distance from the start: {distance}.");
			});
		}

#if IOS || MACCATALYST
		// iOS shows new items from their start. Android's RecyclerView applies a scroll requested before it gets a new adapter to
		// the new one.
		[Fact(DisplayName = "ScrollToEnd right before ItemsSource is replaced doesn't scroll the new items")]
		public async Task ScrollToEndRightBeforeItemsSourceIsReplacedDoesNotScrollTheNewItems()
		{
			SetupBuilder();
			var collectionView = CreateAnchorCollectionView(CreateAnchorItems(40), ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// As many new items as before: what the scroll left for later is for the old items.
				collectionView.ScrollToEnd(animate: false);
				collectionView.ItemsSource = CreateAnchorItems(40);

				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The new items should show from their start. Position: {GetScrollPosition(handler)}.");
			});
		}
#endif

		[Theory(DisplayName = "ScrollToEnd reaches the end of a last item taller than the view")]
		[InlineData(ScrollAnchorMode.None, false)]
		[InlineData(ScrollAnchorMode.None, true)]
		[InlineData(ScrollAnchorMode.End, false)]
		[InlineData(ScrollAnchorMode.End, true)]
		public async Task ScrollToEndReachesTheEndOfALastItemTallerThanTheView(ScrollAnchorMode anchorMode, bool animate)
		{
			SetupBuilder();

			// A long answer ends the chat: taller than the view, with nothing after it.
			var items = CreateAnchorItems(10);
			items.Add(new AnchorItem("Long answer", 600));
			var collectionView = CreateAnchorCollectionView(items, anchorMode);
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (anchorMode == ScrollAnchorMode.None)
				{
					collectionView.ScrollToEnd(animate: false);
				}

				await AssertAtEnd(handler, "The list should start at the end of the long answer.");

				// Read back into the answer: it starts above the view and ends below it.
				ScrollPlatformViewBy(handler, -100);
				await AssertEventually(() => !collectionView.IsAnchored && Math.Abs(GetDistanceToEnd(handler) - 100) <= AnchorTolerance, timeout: 3000, message: "The view should show the middle of the long answer.");

				collectionView.ScrollToEnd(animate);
				await AssertAtEnd(handler, "ScrollToEnd should reach the end of the long answer, not its start.", timeout: 5000);
				Assert.Equal(anchorMode == ScrollAnchorMode.End, collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "A ScrollTo right after ScrollToEnd wins")]
		[InlineData(ScrollAnchorMode.None)]
		[InlineData(ScrollAnchorMode.End)]
		public async Task AScrollToRightAfterScrollToEndWins(ScrollAnchorMode anchorMode)
		{
			SetupBuilder();

			// A single answer taller than the view: the end of the last item is in reach from its start.
			var items = new ObservableCollection<AnchorItem> { new("Long answer", 600) };
			var collectionView = CreateAnchorCollectionView(items, anchorMode);
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (anchorMode == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "The list should open at its end.");
					ScrollPlatformViewBy(handler, -100);
				}
				else
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
					ScrollPlatformViewBy(handler, 100);
				}

				await AssertEventually(() => Math.Abs(GetDistanceFromStart(handler) - (anchorMode == ScrollAnchorMode.End ? 200 : 100)) <= AnchorTolerance, timeout: 3000, message: "The view should show the middle of the answer.");

				collectionView.ScrollToEnd(animate: false);
				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);

				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The later ScrollTo should win.");
				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The later ScrollTo should win. Distance from the start: {GetDistanceFromStart(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "A ScrollTo or AnchorMode change right after an animated ScrollToEnd wins")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AScrollToOrAnchorModeChangeRightAfterAnAnimatedScrollToEndWins(bool changeAnchorMode)
		{
			SetupBuilder();
			var collectionView = CreateAnchorCollectionView(CreateAnchorItems(40), ScrollAnchorMode.None);
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// Before the animated scroll has started: on Android it waits for the view to be laid out.
				collectionView.ScrollToEnd(animate: true);
				if (changeAnchorMode)
				{
					collectionView.AnchorMode = ScrollAnchorMode.Start;
				}
				else
				{
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				}

				// Long enough for the animated scroll to the end to have ended, had it carried on.
				await Task.Delay(1500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The later request should win. Position: {GetScrollPosition(handler)}.");
				Assert.Equal(changeAnchorMode, collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "A ScrollToStart or ScrollToEnd right after an animated ScrollTo wins")]
		[InlineData(ScrollAnchorMode.None, ScrollAnchorMode.End)]
		[InlineData(ScrollAnchorMode.End, ScrollAnchorMode.End)]
		[InlineData(ScrollAnchorMode.None, ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.Start, ScrollAnchorMode.Start)]
		public async Task AScrollToTheEdgeRightAfterAnAnimatedScrollToWins(ScrollAnchorMode anchorMode, ScrollAnchorMode edge)
		{
			SetupBuilder();

			// Few enough rows that the list has measured them all once it has shown its start and its end. UIKit can measure
			// rows above the view in a layout of its own after a scroll, such as when accessibility is on, which moves the end
			// away from a list that doesn't follow it.
			var items = CreateAnchorItems(12);
			var collectionView = CreateAnchorCollectionView(items, anchorMode);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// The view is at the edge already, so the later request doesn't move it: it stops the earlier one there.
				if (edge == ScrollAnchorMode.End)
				{
					collectionView.ScrollToEnd(animate: false);
					await AssertAtEnd(handler, "The list should be at its end.");

					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
					collectionView.ScrollToEnd(animate: false);
				}
				else
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

					collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: true);
					collectionView.ScrollToStart(animate: false);
				}

				// Long enough for the earlier animation to have ended, had it carried on.
				await Task.Delay(1000);
				if (edge == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "The later ScrollToEnd should win.");
				}
				else
				{
					Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The later ScrollToStart should win. Distance from the start: {GetDistanceFromStart(handler)}.");
				}

				Assert.Equal(anchorMode != ScrollAnchorMode.None, collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "A ScrollTo as the list starts following the end again wins")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AScrollToAsTheListStartsFollowingTheEndAgainWins(bool animate)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// ScrollTo(0) shows the first item at the top: with no header, that's the start of the content.
			collectionView.Header = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");
				ScrollPlatformViewBy(handler, -200);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the end should stop following it.");

				// The app hears that the list follows its end again, and asks for its start instead: the newer request wins.
				var scrolledToStart = false;
				collectionView.PropertyChanged += (_, e) =>
				{
					if (!scrolledToStart && e.PropertyName == StructuredItemsView.IsAnchoredProperty.PropertyName && collectionView.IsAnchored)
					{
						scrolledToStart = true;
						collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
					}
				};

				collectionView.ScrollToEnd(animate);
				Assert.True(scrolledToStart, "ScrollToEnd should report that the list follows its end.");

				// Long enough for an animated scroll to the end to have ended, had it carried on.
				await Task.Delay(1500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The later ScrollTo should win. Position: {GetScrollPosition(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "A ScrollTo from Scrolled as ScrollToEnd scrolls the list wins")]
		[InlineData(ScrollAnchorMode.None)]
		[InlineData(ScrollAnchorMode.End)]
		public async Task AScrollToFromScrolledAsScrollToEndScrollsTheListWins(ScrollAnchorMode anchorMode)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, anchorMode);
			collectionView.Header = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (anchorMode == ScrollAnchorMode.End)
				{
					// Read from the top: the list doesn't follow its end.
					await AssertAtEnd(handler, "The list should open at its end.");
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				}

				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance && !collectionView.IsAnchored, timeout: 3000, message: "The list should show its start.");

				// The app asks for the start again as soon as ScrollToEnd moves the list away from it: the newer request wins.
				var scrolledToStart = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!scrolledToStart && GetDistanceFromStart(handler) > 100)
					{
						scrolledToStart = true;
						collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
					}
				};

				collectionView.ScrollToEnd(animate: false);
				await AssertEventually(() => scrolledToStart, timeout: 3000, message: "ScrollToEnd should scroll the list.");
				await Task.Delay(1000);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The later ScrollTo should win. Position: {GetScrollPosition(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "ScrollToEnd from Scrolled as a ScrollTo scrolls the list wins")]
		public async Task ScrollToEndFromScrolledAsAScrollToScrollsTheListWins()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The app asks for the end again as soon as the ScrollTo moves the list away from it: the newer request wins,
				// and the list follows the end all the way there.
				var scrolledToEnd = false;
				var stoppedFollowing = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!scrolledToEnd && GetDistanceToEnd(handler) > 100)
					{
						scrolledToEnd = true;
						collectionView.ScrollToEnd(animate: true);
					}
				};
				collectionView.PropertyChanged += (_, e) =>
				{
					if (scrolledToEnd && e.PropertyName == nameof(CollectionView.IsAnchored) && !collectionView.IsAnchored)
					{
						stoppedFollowing = true;
					}
				};

				collectionView.ScrollTo(5, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(() => scrolledToEnd, timeout: 3000, message: "The ScrollTo should scroll the list.");
				await AssertAtEnd(handler, "The later ScrollToEnd should win.", timeout: 5000);
				Assert.False(stoppedFollowing, "ScrollToEnd should follow the end on its way there.");
				Assert.True(collectionView.IsAnchored, "ScrollToEnd should follow the end.");

				items.Add(new AnchorItem("Added", 40));
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 40) <= AnchorTolerance, timeout: 3000, message: "The new item should be laid out in view.");
				await AssertAtEnd(handler, "Following the end, a new item should keep the end in view.");
			});
		}

		[Fact(DisplayName = "ScrollToEnd from Scrolled as a ScrollTo lays out a last item taller than the view reaches its end")]
		public async Task ScrollToEndFromScrolledAsAScrollToLaysOutATallLastItemReachesItsEnd()
		{
			SetupBuilder();

			// A long answer ends the chat: taller than the view, with nothing after it. A list that doesn't follow its end, so
			// nothing but ScrollToEnd takes it the rest of the way past the start of the answer, where scrolling to it stops.
			var items = CreateAnchorItems(10);
			items.Add(new AnchorItem("Long answer", 600));
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// The app asks for the end as soon as the ScrollTo lays the long answer out: from the Scrolled event that layout
				// raises, before the layout that scrolls to the end.
				var scrolledToEnd = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!scrolledToEnd && !double.IsNaN(GetItemHeight(handler, items.Count - 1, hasHeader: false)))
					{
						scrolledToEnd = true;
						collectionView.ScrollToEnd(animate: false);
					}
				};

				collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(() => scrolledToEnd, timeout: 3000, message: "The ScrollTo should lay the long answer out.");
				await AssertAtEnd(handler, "ScrollToEnd should reach the end of the long answer, not its start.");

				// Long enough for the layouts the scrolls asked for to have run.
				await Task.Delay(500);
				await AssertAtEnd(handler, "The list should stay at the end of the long answer.");
				Assert.False(collectionView.IsAnchored);
			});
		}

#if IOS || MACCATALYST
		// UIKit can stop the animation of ScrollToStart short of the start, having measured the rows it brought into view on the
		// way: ScrollToStart goes the rest of the way once UIKit reports that the animation ended, which raises Scrolled.
		[Fact(DisplayName = "ScrollToEnd from Scrolled as ScrollToStart goes the rest of the way reaches the end")]
		public async Task ScrollToEndFromScrolledAsScrollToStartGoesTheRestOfTheWayReachesTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				collectionView.ScrollToEnd(animate: false);
				await AssertAtEnd(handler, "ScrollToEnd should reach the end.");

				// The app asks for the end again as soon as the list is back at its start.
				var scrolledToEnd = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!scrolledToEnd && GetDistanceFromStart(handler) <= AnchorTolerance)
					{
						scrolledToEnd = true;
						collectionView.ScrollToEnd(animate: true);
					}
				};

				// UIKit stops the animation short of the start, as it measures the rows it brings into view on the way: they're
				// estimated at 30 high, and 40 high. ScrollToStart goes the rest of the way once UIKit reports that it ended.
				collectionView.ScrollToStart(animate: true);
				await AssertEventually(() => scrolledToEnd, timeout: 5000, message: "ScrollToStart should go the rest of the way to the start.");

				// The end moves past where the animation of ScrollToEnd stops: it goes the rest of the way once that ends.
				items[^1].Height = 120;
				await AssertAtEnd(handler, "ScrollToEnd should reach the end once its animation ends.", timeout: 5000);
				Assert.False(collectionView.IsAnchored);
			});
		}
#endif

#if ANDROID
		[Fact(DisplayName = "A ScrollTo as new items are laid out wins over following the end")]
		public async Task AScrollToAsNewItemsAreLaidOutWinsOverFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// ScrollTo(0) shows the first item at the top: with no header, that's the start of the content.
			collectionView.Header = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// Right after the layout that lays out the new items, which leaves the end below the view: before the anchor
				// gets the view there.
				var recyclerView = (RecyclerView)handler.PlatformView;
				void ScrollToStart(object sender, EventArgs e)
				{
					recyclerView.ViewTreeObserver.GlobalLayout -= ScrollToStart;
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				}

				recyclerView.ViewTreeObserver.GlobalLayout += ScrollToStart;
				for (var i = 0; i < 20; i++)
				{
					items.Add(new AnchorItem($"Added {i}", 40));
				}

				var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 3000);
				Assert.True(atStart, $"The list should scroll to its start. Position: {GetScrollPosition(handler)}, anchored: {collectionView.IsAnchored}.");
				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, "Following the end shouldn't undo the ScrollTo.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "A scroll away from the end as new items are laid out stops following the end")]
		public async Task AScrollAwayAsNewItemsAreLaidOutStopsFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// The last item ends the content.
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// Right after the layout that lays out the new items, which leaves the end below the view: before the anchor
				// gets the view there, code scrolls the native view up, as the keyboard or accessibility can.
				var recyclerView = (RecyclerView)handler.PlatformView;
				string position = null;
				void ScrollUp(object sender, EventArgs e)
				{
					recyclerView.ViewTreeObserver.GlobalLayout -= ScrollUp;
					recyclerView.ScrollBy(0, -(int)(200 * recyclerView.Resources.DisplayMetrics.Density));
					position = GetScrollPosition(handler);
				}

				recyclerView.ViewTreeObserver.GlobalLayout += ScrollUp;
				for (var i = 0; i < 20; i++)
				{
					items.Add(new AnchorItem($"Added {i}", 40));
				}

				await AssertEventually(() => position is not null, timeout: 3000, message: "The new items should be laid out.");
				await Task.Delay(500);
				Assert.True(position == GetScrollPosition(handler), $"The list should stay where the scroll took it, at {position}, not at {GetScrollPosition(handler)}.");
				Assert.True(GetDistanceToEnd(handler) > AnchorTolerance, "The list shouldn't go to its end.");
				Assert.False(collectionView.IsAnchored, "A scroll away from the end should stop following it.");
			});
		}

		// RecyclerView reports that a fling ended before the anchor looks at the settled view, which it does in the next frame: a
		// scroll made in between, as the keyboard, accessibility or code can make, is a new scroll, not the end of the fling.
		[Fact(DisplayName = "A scroll away right as a fling ends stops following the end")]
		public async Task AScrollAwayRightAsAFlingEndsStopsFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				var recyclerView = (RecyclerView)handler.PlatformView;
				var density = recyclerView.Resources.DisplayMetrics.Density;
				var scrolledAway = false;
				var listener = new ScrollEndedListener(() =>
				{
					if (!scrolledAway)
					{
						scrolledAway = true;
						recyclerView.ScrollBy(0, -(int)(200 * density));
					}
				});

				recyclerView.AddOnScrollListener(listener);
				try
				{
					// A fling that ends within the threshold of the end, which the list still follows.
					recyclerView.SmoothScrollBy(0, -(int)(4 * density));
					await AssertEventually(() => scrolledAway, timeout: 3000, message: "The fling should end.");
					await Task.Delay(500);
				}
				finally
				{
					recyclerView.RemoveOnScrollListener(listener);
				}

				Assert.True(GetDistanceToEnd(handler) > 100, $"The list shouldn't be pulled back to its end. Position: {GetScrollPosition(handler)}.");
				Assert.False(collectionView.IsAnchored, "A scroll away from the end should stop following it.");
			});
		}

		// Without the first row laid out, the anchor estimates how far the content extends above the rows in view, from their
		// average height. A first row much taller than the others, just above the view at the end, makes that estimate far off.
		// A scroll that first lays it out, as code scrolling the native view or accessibility makes, moves the view as far as the
		// rows in view moved, not as far as the estimate was off.
		[Fact(DisplayName = "A scroll away from the end that first lays out a tall first row stops following the end")]
		public async Task AScrollAwayFromTheEndThatFirstLaysOutATallFirstRowStopsFollowingTheEnd()
		{
			SetupBuilder();

			// A first row taller than three viewports, then rows that fill a little more than one.
			var items = new ObservableCollection<AnchorItem>(
				Enumerable.Range(0, 111).Select(index => new AnchorItem($"Item {index}", index == 0 ? 1000 : 3)));
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// The first row starts the content.
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");
				await Task.Delay(300);
				Assert.True(double.IsPositiveInfinity(GetDistanceFromStart(handler)), "The first row shouldn't be laid out at the end.");

				// Code scrolls the native view up 100, which lays the first row out.
				var recyclerView = (RecyclerView)handler.PlatformView;
				recyclerView.ScrollBy(0, -(int)(100 * recyclerView.Resources.DisplayMetrics.Density));
				var position = GetScrollPosition(handler);
				Assert.False(double.IsPositiveInfinity(GetDistanceFromStart(handler)), "The scroll should lay the first row out.");

				await Task.Delay(500);
				Assert.True(position == GetScrollPosition(handler), $"The list should stay where the scroll took it, at {position}, not at {GetScrollPosition(handler)}.");
				Assert.False(collectionView.IsAnchored, "A scroll away from the end should stop following it.");
			});
		}
#endif

#if IOS || MACCATALYST
		// UIKit reports that a drag, a deceleration or an animated scroll of the app's ended, and the anchor looks at where the view
		// settled once it's laid out next: a scroll the delegate reports before that, as the keyboard, accessibility or code make,
		// is a new scroll, not the end of the one that ended.
		[Fact(DisplayName = "A scroll away right as an animated scroll ends stops following the end")]
		public async Task AScrollAwayRightAsAnAnimatedScrollEndsStopsFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// An animated scroll of the app's scrolls as a fling does until UIKit reports that it ended: this one ends at the end.
				var platformView = handler.Controller.CollectionView;
				var scrollAnchor = handler.Controller.ScrollAnchor;
				scrollAnchor.ScrollRequesting();
				scrollAnchor.ScrollRequested(platformView, isLatest: true, animating: true);
				platformView.SetNeedsLayout();
				platformView.LayoutIfNeeded();
				Assert.True(collectionView.IsAnchored, "Ending at the end, the scroll should leave the list following it.");

				// UIKit reports that it ended, and code scrolls the native view up before the layout that report asks for.
				handler.Controller.ScrollAnimationEnded();
				ScrollPlatformViewBy(handler, -200);

				await Task.Delay(500);
				Assert.True(GetDistanceToEnd(handler) > 100, $"The list shouldn't be pulled back to its end. Distance to the end: {GetDistanceToEnd(handler)}.");
				Assert.False(collectionView.IsAnchored, "A scroll away from the end should stop following it.");
			});
		}

		// The keyboard manager insets the view and moves its offset before the layout either asks for: the offset moved for the
		// inset, which the anchor judges as layout, keeping the end in view.
		[Fact(DisplayName = "An offset change that comes with an inset change outside a layout pass keeps the end in view")]
		public async Task AnOffsetChangeThatComesWithAnInsetChangeOutsideALayoutPassKeepsTheEndInView()
		{
			SetupBuilder();
			var collectionView = CreateAnchorCollectionView(CreateAnchorItems(40), ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				var platformView = handler.Controller.CollectionView;
				var inset = platformView.ContentInset;
				platformView.ContentInset = new UIKit.UIEdgeInsets(inset.Top, inset.Left, inset.Bottom + 300, inset.Right);
				ScrollPlatformViewBy(handler, -200);

				await AssertAtEnd(handler, "The list should keep its end in view.");
				await Task.Delay(500);
				Assert.True(Math.Abs(GetDistanceToEnd(handler)) <= AnchorTolerance, $"The list should stay at its end. Distance to the end: {GetDistanceToEnd(handler)}.");
				Assert.True(collectionView.IsAnchored, "An offset change that comes with an inset change shouldn't stop following the end.");
			});
		}

		// A scroll of the app's code or of the keyboard, without animating, or an animated one, as a tap on the status bar makes,
		// stops the animated scroll ScrollToEnd started on its way to the end: UIKit reports that the animation ended. The list
		// no longer goes to the end, and is judged by where it stopped.
		[Theory(DisplayName = "A scroll that stops an animated ScrollToEnd on its way leaves the list where it stopped")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AScrollThatStopsAnAnimatedScrollToEndOnItsWayLeavesTheListWhereItStopped(bool animated)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");
				collectionView.ScrollToStart(animate: false);
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should be at its start.");
				await Task.Delay(300);
				Assert.False(collectionView.IsAnchored, "At its start, the list shouldn't follow its end.");

				collectionView.ScrollToEnd(animate: true);
				Assert.True(collectionView.IsAnchored, "ScrollToEnd should follow the end on its way there.");

				var platformView = handler.Controller.CollectionView;
				platformView.SetContentOffset(new CoreGraphics.CGPoint(platformView.ContentOffset.X, -platformView.AdjustedContentInset.Top), animated);

				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should be back at its start.");
				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The list should stay at its start. Distance from the start: {GetDistanceFromStart(handler)}.");
				Assert.False(collectionView.IsAnchored, "Stopped on its way, the list shouldn't follow the end.");

				// A new last item doesn't take the list to the end.
				items.Add(new AnchorItem("New", 40));
				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The list shouldn't be taken to its end. Distance from the start: {GetDistanceFromStart(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		// UIKit doesn't animate a scroll of a view that isn't in a window: it scrolls the view there, and reports that the
		// animation ended before the request returns. A grouped list that measures all its items scrolls to the item again once
		// the animation ends: right away then, not when a later animation ends.
		[Theory(DisplayName = "An animated ScrollTo in a grouped list made while it isn't in a window doesn't scroll back to its item later")]
		[InlineData(ScrollToPosition.End)]
		[InlineData(ScrollToPosition.MakeVisible)]
		public async Task AnAnimatedScrollToInAGroupedListMadeOutsideTheWindowDoesNotScrollBackToItsItemLater(ScrollToPosition position)
		{
			SetupBuilder();
			var groups = new ObservableCollection<AnchorGroup>(
				Enumerable.Range(0, 4).Select(group => new AnchorGroup($"Group {group}", CreateAnchorItems(8))));
			var collectionView = CreateAnchorCollectionView(groups, ScrollAnchorMode.None);
			collectionView.IsGrouped = true;
			collectionView.GroupHeaderTemplate = CreateAnchorGroupHeaderTemplate();

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// The app scrolls the list to an item of its last group while the list is out of its window.
				var platformView = handler.PlatformView;
				var superview = platformView.Superview;
				var index = Array.IndexOf(superview.Subviews, platformView);
				platformView.RemoveFromSuperview();
				collectionView.ScrollTo(4, groupIndex: 3, position: position, animate: true);
				superview.InsertSubview(platformView, index);
				await AssertEventually(() => GetDistanceFromStart(handler) > 100, timeout: 3000, message: "The list should show the item.");
				await Task.Delay(300);
				var distanceAtItem = GetDistanceFromStart(handler);

				// An animation the app didn't request through the list, as the keyboard makes, scrolls the list back toward its
				// start. UIKit stops it short of the start, as it measures the rows above that the jump skipped.
				var collectionViewPlatform = handler.Controller.CollectionView;
				collectionViewPlatform.SetContentOffset(new CoreGraphics.CGPoint(collectionViewPlatform.ContentOffset.X, -collectionViewPlatform.AdjustedContentInset.Top), true);
				await AssertEventually(() => GetDistanceFromStart(handler) < distanceAtItem / 2, timeout: 3000, message: "The list should scroll back toward its start.");

				// Long enough for the animation to end, and for a scroll back to the item, had its end started one.
				await Task.Delay(1000);
				Assert.True(GetDistanceFromStart(handler) < distanceAtItem / 2, $"The list shouldn't scroll back to the item. Distance from the start: {GetDistanceFromStart(handler)}, at the item: {distanceAtItem}.");
			});
		}

		// An animated ScrollToStart or ScrollToEnd of a list at that edge already has nowhere to go: UIKit doesn't animate, and
		// never reports an end. The list follows the edge from where it is, and a scroll away from it right after, as code
		// moving the native view makes, stops it following.
		[Theory(DisplayName = "An animated ScrollToStart or ScrollToEnd of a list at that edge lets a scroll right after it take the list away")]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		public async Task AnAnimatedScrollToAnEdgeTheListIsAtLetsAScrollRightAfterItTakeTheListAway(ScrollAnchorMode edge)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var toStart = edge == ScrollAnchorMode.Start;
			var collectionView = CreateAnchorCollectionView(items, edge);

			// Only the anchor could move the list as items are added (see TurningAnchorModeOffFromScrolledStopsFollowingTheEnd).
			collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				double Distance() => toStart ? GetDistanceFromStart(handler) : GetDistanceToEnd(handler);
				if (toStart)
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				}
				else
				{
					await AssertAtEnd(handler, "The list should open at its end.");
				}

				// Laid out where it is.
				await Task.Delay(300);

				if (toStart)
				{
					collectionView.ScrollToStart(animate: true);
				}
				else
				{
					collectionView.ScrollToEnd(animate: true);
				}

				Assert.True(collectionView.IsAnchored, "The list should follow the edge it's at.");

				// Code scrolls the native view 200 away from the edge. The rows it brings into view are measured, which can only move
				// the end further away.
				var platformView = handler.Controller.CollectionView;
				platformView.SetContentOffset(new CoreGraphics.CGPoint(platformView.ContentOffset.X, platformView.ContentOffset.Y + (toStart ? 200 : -200)), false);

				await Task.Delay(500);
				Assert.True(Distance() >= 200 - AnchorTolerance, $"The list should stay where the scroll took it. Distance from the edge: {Distance()}.");
				Assert.False(collectionView.IsAnchored, "Scrolled away, the list shouldn't follow the edge.");

				// The content growing doesn't take the list back to the edge.
				items.Add(new AnchorItem("New", 40));
				await Task.Delay(500);
				Assert.True(Distance() >= 150, $"The list shouldn't be taken back to the edge. Distance from the edge: {Distance()}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		// An animated ScrollToStart of a list that doesn't follow its start goes the rest of the way once its animation ends: UIKit
		// stops it short as it measures the rows it brings into view above. A scroll that stops it on its way, as code moving the
		// native view makes, without animating or animated, leaves the list where that scroll took it.
		[Theory(DisplayName = "A scroll that stops an animated ScrollToStart of a list that doesn't follow its start leaves the list where it took it")]
		[InlineData(ScrollAnchorMode.None, false)]
		[InlineData(ScrollAnchorMode.None, true)]
		[InlineData(ScrollAnchorMode.End, false)]
		[InlineData(ScrollAnchorMode.End, true)]
		public async Task AScrollThatStopsAnAnimatedScrollToStartOfAListThatDoesNotFollowItsStartLeavesTheListWhereItTookIt(ScrollAnchorMode anchorMode, bool animated)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, anchorMode);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (anchorMode != ScrollAnchorMode.End)
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
					collectionView.ScrollToEnd(animate: false);
				}

				await AssertAtEnd(handler, "The list should be at its end.");
				await Task.Delay(300);
				var extent = GetDistanceFromStart(handler);

				collectionView.ScrollToStart(animate: true);

				// Code scrolls the native view to 100 short of the end as the animation starts.
				var platformView = handler.Controller.CollectionView;
				var inset = platformView.AdjustedContentInset;
				var end = Math.Max(-inset.Top, platformView.ContentSize.Height + inset.Bottom - platformView.Bounds.Height);
				platformView.SetContentOffset(new CoreGraphics.CGPoint(platformView.ContentOffset.X, end - 100), animated);

				await AssertEventually(() => GetDistanceToEnd(handler) >= 100 - AnchorTolerance, timeout: 3000, message: "The scroll should take the list to 100 short of its end.");

				// Long enough for the animation the scroll stopped to end, and for a scroll to the start, had its end started one.
				// UIKit can move the end further away as it measures the rows the scroll brought into view.
				await Task.Delay(1000);
				var distance = GetDistanceToEnd(handler);
				Assert.True(distance >= 100 - AnchorTolerance && distance < extent / 2, $"The list should stay where the scroll took it, near its end. Distance to the end: {distance}, from the start: {GetDistanceFromStart(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		// A scroll UIKit animates away from the end, as code moving the native view or a tap on the status bar make, scrolls as a
		// fling does: the content growing as it starts doesn't take the list back to the end, which would stop the animation.
		[Fact(DisplayName = "A scroll UIKit animates away from the end isn't stopped by the content growing as it starts")]
		public async Task AScrollUIKitAnimatesAwayFromTheEndIsNotStoppedByTheContentGrowingAsItStarts()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");
				await Task.Delay(300);
				var extent = GetDistanceFromStart(handler);

				// The last item grows, as a streamed answer does, as code animates the native view to 100 short of the end.
				var platformView = handler.Controller.CollectionView;
				var inset = platformView.AdjustedContentInset;
				var end = Math.Max(-inset.Top, platformView.ContentSize.Height + inset.Bottom - platformView.Bounds.Height);
				items[^1].Height += 50;
				platformView.SetContentOffset(new CoreGraphics.CGPoint(platformView.ContentOffset.X, end - 100), true);

				// The animation goes where it was going, 100 short of the end that UIKit moves as it measures the rows it brings into
				// view, and as the content grows past the view.
				await AssertEventually(() => !collectionView.IsAnchored && GetDistanceToEnd(handler) >= 100, timeout: 3000, message: "The scroll should take the list 100 short of its end, or further.");

				// Long enough for the animation to end. UIKit moves the end further away as it measures the rows above again, by as
				// much as it estimated them wrong: the list ended 130 short of the end on macOS 27, and 360 short on macOS 26.
				await Task.Delay(1000);
				var distance = GetDistanceToEnd(handler);
				Assert.True(distance >= 100 && distance < extent / 2, $"The list should stay where the scroll took it, away from both edges. Distance to the end: {distance}, from the start: {GetDistanceFromStart(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}
#endif

		[Fact(DisplayName = "Turning AnchorMode off stops following the end")]
		public async Task TurningAnchorModeOffStopsFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				collectionView.AnchorMode = ScrollAnchorMode.None;
				Assert.False(collectionView.IsAnchored);

				items.Add(new AnchorItem("Added", 40));
				await Task.Delay(500);
				Assert.True(GetDistanceToEnd(handler) > AnchorTolerance, "With AnchorMode off, the list shouldn't follow its end.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "Turning AnchorMode off from Scrolled as the view follows the end stops following it")]
		public async Task TurningAnchorModeOffFromScrolledStopsFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// KeepItemsInView, the default, shows the first item as items are added to a list that doesn't follow an edge.
			// KeepScrollOffset leaves the list where it is: only the anchor could move it.
			collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The scroll that follows the new item raises Scrolled, before the anchor is done with it.
				var turnedOff = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!turnedOff)
					{
						turnedOff = true;
						collectionView.AnchorMode = ScrollAnchorMode.None;
					}
				};

				items.Add(new AnchorItem("Added", 40));
				await AssertEventually(() => turnedOff, timeout: 3000, message: "Following the new item should scroll the list.");
				await Task.Delay(500);
				Assert.False(collectionView.IsAnchored, "With AnchorMode off, the list shouldn't follow its end.");

				var position = GetScrollPosition(handler);
				items.Add(new AnchorItem("Added after turning AnchorMode off", 40));
				await Task.Delay(500);
				Assert.True(position == GetScrollPosition(handler), "With AnchorMode off, a new item shouldn't move the list.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "Turning AnchorMode off as the list first reports following the end stops following it")]
		public async Task TurningAnchorModeOffAsTheListFirstReportsFollowingTheEndStopsFollowingIt()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// Only the anchor could move the list as items are added (see TurningAnchorModeOffFromScrolledStopsFollowingTheEnd).
			collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			// The first report comes as the handler starts anchoring the list.
			var turnedOff = false;
			collectionView.PropertyChanged += (_, e) =>
			{
				if (!turnedOff && e.PropertyName == StructuredItemsView.IsAnchoredProperty.PropertyName && collectionView.IsAnchored)
				{
					turnedOff = true;
					collectionView.AnchorMode = ScrollAnchorMode.None;
				}
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => turnedOff, timeout: 3000, message: "The list should report following its end.");
				await Task.Delay(500);
				Assert.False(collectionView.IsAnchored, "With AnchorMode off, the list shouldn't follow its end.");

				var position = GetScrollPosition(handler);
				items.Add(new AnchorItem("Added after turning AnchorMode off", 40));
				await Task.Delay(500);
				Assert.True(position == GetScrollPosition(handler), "With AnchorMode off, a new item shouldn't move the list.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		// Turning anchoring off isn't a scroll: unlike an edge the view starts to follow, it doesn't stop a scroll the app
		// requested.
		[Theory(DisplayName = "Turning AnchorMode off during an animated scroll the app requested lets it carry on")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task TurningAnchorModeOffDuringAnAnimatedScrollTheAppRequestedLetsItCarryOn(bool byScrollToStart)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// Without a header, the first item starts the content: ScrollTo scrolls to the same place as ScrollToStart.
			collectionView.Header = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				if (byScrollToStart)
				{
					collectionView.ScrollToStart(animate: true);
				}
				else
				{
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
				}

				collectionView.AnchorMode = ScrollAnchorMode.None;
				Assert.False(collectionView.IsAnchored);

				var distance = double.NaN;
				var reached = await Wait(() => (distance = GetDistanceFromStart(handler)) <= AnchorTolerance, 3000);
				Assert.True(reached, $"The scroll should carry on to the start. Distance from the start: {distance}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		// The view follows the edge an animated ScrollToStart or ScrollToEnd scrolls to, past where its animation stops, as the
		// rows it brings into view are measured. Turning anchoring off leaves it to go the rest of the way, as one to an edge the
		// view doesn't follow does.
		[Theory(DisplayName = "Turning AnchorMode off during an animated ScrollToStart or ScrollToEnd to the edge it follows lets it reach that edge")]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		public async Task TurningAnchorModeOffDuringAnAnimatedScrollToTheEdgeItFollowsLetsItReachThatEdge(ScrollAnchorMode edge)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, edge);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// To the other edge without animating: on iOS, the rows in between keep the size UIKit estimates for them until
				// the scroll back brings them into view.
				if (edge == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "The list should open at its end.");
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				}
				else
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
					collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: false);
				}

				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the edge should stop following it.");

				// Before the animation's first frame.
				if (edge == ScrollAnchorMode.End)
				{
					collectionView.ScrollToEnd(animate: true);
				}
				else
				{
					collectionView.ScrollToStart(animate: true);
				}

				collectionView.AnchorMode = ScrollAnchorMode.None;
				Assert.False(collectionView.IsAnchored);

				if (edge == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "The scroll should carry on to the end.");
				}
				else
				{
					var distance = double.NaN;
					var reached = await Wait(() => (distance = GetDistanceFromStart(handler)) <= AnchorTolerance, 3000);
					Assert.True(reached, $"The scroll should carry on to the start. Distance from the start: {distance}.");
				}

				Assert.False(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "AnchorMode Start keeps items added before the first item in view")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task AnchorModeStartKeepsItemsAddedBeforeTheFirstInView(bool hasHeader)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start);
			if (!hasHeader)
			{
				// The first item starts the content: Android keeps it where it is and adds new items above the view.
				collectionView.Header = null;
			}

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				Assert.True(collectionView.IsAnchored);

				for (var i = 0; i < 3; i++)
				{
					// No other item is this tall: once the first item laid out is, the new item is laid out.
					items.Insert(0, new AnchorItem($"Newest {i}", 64));
					var atStart = await Wait(() => Math.Abs(GetItemHeight(handler, 0, hasHeader) - 64) <= AnchorTolerance && GetDistanceFromStart(handler) <= AnchorTolerance, 3000);
					Assert.True(atStart, $"The new first item should be in view at the start. Position: {GetScrollPosition(handler)}, distance from the start: {GetDistanceFromStart(handler)}.");
				}

				Assert.True(collectionView.IsAnchored, "The list should still follow its start.");

				// Following the start, items added after the last item don't move the list.
				var position = GetScrollPosition(handler);
				items.Add(new AnchorItem("Added at the end", 40));
				await Task.Delay(500);
				Assert.True(position == GetScrollPosition(handler), "A new last item shouldn't move a list that follows its start.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "AnchorMode Start shows a new first item when the last item is removed in the same update")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task AnchorModeStartShowsANewFirstItemWhenTheLastItemIsRemovedInTheSameUpdate(bool hasHeader)
		{
			SetupBuilder();

			// The last item is as tall as the new first item, and no other item is: removing it keeps the content the size it
			// was, and the only item that tall afterwards is the new one.
			var items = CreateAnchorItems(39);
			items.Add(new AnchorItem("Oldest", 64));
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start);
			if (!hasHeader)
			{
				collectionView.Header = null;
			}

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// A feed that keeps as many items as it had: the content stays the size it was, while the platform keeps the
				// row it showed first where it is, which can leave the new first item above the view. Only the platform saying
				// it laid the change out tells that move from a scroll.
				items.Insert(0, new AnchorItem("Newest", 64));
				items.RemoveAt(items.Count - 1);

				var atStart = await Wait(() => Math.Abs(GetItemHeight(handler, 0, hasHeader) - 64) <= AnchorTolerance && GetDistanceFromStart(handler) <= AnchorTolerance, 3000);
				Assert.True(atStart, $"The new first item should be in view at the start. Position: {GetScrollPosition(handler)}, anchored: {collectionView.IsAnchored}.");
				Assert.True(collectionView.IsAnchored, "The list should still follow its start.");
			});
		}

		[Fact(DisplayName = "AnchorMode Start keeps the start in view over KeepLastItemInView while it follows it")]
		public async Task AnchorModeStartKeepsTheStartInViewOverKeepLastItemInViewWhileItFollowsIt()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start);
			collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				items.Add(new AnchorItem("Added while following the start", 40));

				// Long enough for the animated scroll KeepLastItemInView makes to have run, had it applied.
				await Task.Delay(1000);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"Following the start, a new last item shouldn't scroll the list. Position: {GetScrollPosition(handler)}.");
				Assert.True(collectionView.IsAnchored);

				// Once the list doesn't follow its start, KeepLastItemInView applies as usual.
				ScrollPlatformViewBy(handler, 200);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the start should stop following it.");
				items.Add(new AnchorItem("Added after scrolling away", 40));
				await AssertEventually(() => !double.IsNaN(GetItemHeight(handler, items.Count - 1)), timeout: 3000, message: "KeepLastItemInView should bring the new last item into view.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "AnchorMode Start stops following the start once scrolled away from it")]
		public async Task AnchorModeStartStopsFollowingTheStartOnceScrolledAway()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start);
			collectionView.Header = null;

			// Only the anchor could move the list as items are added (see TurningAnchorModeOffFromScrolledStopsFollowingTheEnd).
			collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// Less than a row: the first item stays laid out, so the distance from the start is exact.
				ScrollPlatformViewBy(handler, 30);
				await AssertEventually(() => !collectionView.IsAnchored && Math.Abs(GetDistanceFromStart(handler) - 30) <= AnchorTolerance, timeout: 3000, message: "Scrolling away from the start should stop following it.");

				items.Insert(0, new AnchorItem("Newest", 64));
				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) > AnchorTolerance, $"A new first item shouldn't scroll a list that doesn't follow its start. Position: {GetScrollPosition(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

#if ANDROID
		// Without the first row laid out, the anchor measures the offset from rows it laid out before, which counts on the rows
		// above the view keeping their size. On iOS and Mac Catalyst, the offset is UIKit's, from its layout of every row.
		[Theory(DisplayName = "A row above the view that grows out of view doesn't make the list follow the start before it gets there")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ARowAboveTheViewThatGrowsOutOfViewDoesntMakeTheListFollowTheStartEarly(bool replaced)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start);
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance && collectionView.IsAnchored, timeout: 3000, message: "The list should open at its start, and follow it.");

				// In steps shorter than the view: each look measures from rows the one before laid out, from the start on.
				for (var i = 0; i < 4; i++)
				{
					ScrollPlatformViewBy(handler, 100);
					await Task.Delay(100);
				}

				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the start should stop following it.");

				// The second item, out of view above, grows by far more than the list scrolled: replaced, which the adapter
				// reports, or resized by a binding, which nothing reports until the row is laid out again.
				if (replaced)
				{
					items[1] = new AnchorItem("Tall", 1000);
				}
				else
				{
					items[1].Height = 1000;
				}

				await Task.Delay(300);

				// Back by more than the list scrolled down, and much less than the distance to the start now: the view ends
				// up inside the tall row.
				for (var i = 0; i < 5; i++)
				{
					ScrollPlatformViewBy(handler, -100);
					await Task.Delay(100);
				}

				// Long enough for a scroll to the start to have happened, had the list followed it.
				await Task.Delay(500);
				Assert.True(ShowsItemOfHeight(handler, 1000), $"The view should show the tall row. Position: {GetScrollPosition(handler)}.");
				Assert.True(double.IsPositiveInfinity(GetDistanceFromStart(handler)), $"The view shouldn't be at the start. Position: {GetScrollPosition(handler)}.");
				Assert.False(collectionView.IsAnchored, "The list shouldn't follow the start before it gets there.");

				// Once it does get there, it follows the start again.
				ScrollPlatformViewBy(handler, -2000);
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance && collectionView.IsAnchored, timeout: 3000, message: "Scrolling back to the start should follow it again.");
			});
		}
#endif

		[Theory(DisplayName = "A scroll that brings new rows into view stops following the edge")]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		public async Task AScrollThatBringsNewRowsIntoViewStopsFollowingTheEdge(ScrollAnchorMode edge)
		{
			SetupBuilder();

			// Rows of different heights: the platform's idea of the content's size changes as it lays out the rows the
			// scroll brings into view.
			var items = new ObservableCollection<AnchorItem>(Enumerable.Range(0, 40).Select(index => new AnchorItem($"Item {index}", index % 2 == 0 ? 40 : 90)));
			var collectionView = CreateAnchorCollectionView(items, edge);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (edge == ScrollAnchorMode.Start)
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				}
				else
				{
					await AssertAtEnd(handler, "The list should open at its end.");
				}

				// A scroll the list didn't ask for and the user isn't making, as accessibility or the keyboard can make.
				ScrollPlatformViewBy(handler, edge == ScrollAnchorMode.Start ? 400 : -400);
				await Task.Delay(500);
				Assert.False(collectionView.IsAnchored, "Scrolling away from the edge should stop following it.");
				var distance = edge == ScrollAnchorMode.Start ? GetDistanceFromStart(handler) : GetDistanceToEnd(handler);
				Assert.True(distance > AnchorTolerance, $"The list shouldn't scroll back to the edge. Position: {GetScrollPosition(handler)}.");
			});
		}

		[Theory(DisplayName = "ScrollToStart reaches above the header and follows the start again")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ScrollToStartFollowsTheStartAgain(bool animate)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				collectionView.ScrollTo(20, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "ScrollTo away from the start should stop following it.");

				var anchoredChanges = new List<bool>();
				collectionView.PropertyChanged += (_, e) =>
				{
					if (e.PropertyName == StructuredItemsView.IsAnchoredProperty.PropertyName)
					{
						anchoredChanges.Add(collectionView.IsAnchored);
					}
				};

				collectionView.ScrollToStart(animate);
				Assert.True(collectionView.IsAnchored, "ScrollToStart should follow the start right away.");
				var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 5000);
				Assert.True(atStart, $"ScrollToStart should reach the start of the content, header included. Distance from the start: {GetDistanceFromStart(handler)}.");
				Assert.True(collectionView.IsAnchored, "ScrollToStart should follow the start again.");
				Assert.DoesNotContain(false, anchoredChanges);

				items.Insert(0, new AnchorItem("Newest", 64));
				var newestInView = await Wait(() => Math.Abs(GetItemHeight(handler, 0) - 64) <= AnchorTolerance && GetDistanceFromStart(handler) <= AnchorTolerance, 3000);
				Assert.True(newestInView, $"The new first item should be in view at the start. Position: {GetScrollPosition(handler)}.");
				Assert.True(collectionView.IsAnchored, "The list should still follow its start.");
			});
		}

		[Theory(DisplayName = "ScrollToStart without AnchorMode scrolls above the header once")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ScrollToStartWithoutAnchorModeScrollsToTheStartOnce(bool animate)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				collectionView.ScrollToEnd(animate: false);
				await AssertAtEnd(handler, "ScrollToEnd should reach the end.");

				// The rows on the way are measured as the view scrolls past them, which moves what's above the view.
				collectionView.ScrollToStart(animate);
				var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 5000);
				Assert.True(atStart, $"ScrollToStart should reach the start of the content, header included. Distance from the start: {GetDistanceFromStart(handler)}.");
				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, "The list should stay at its start.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "Scrolling to the edge a list doesn't follow scrolls there once")]
		[InlineData(ScrollAnchorMode.Start, false)]
		[InlineData(ScrollAnchorMode.Start, true)]
		[InlineData(ScrollAnchorMode.End, false)]
		[InlineData(ScrollAnchorMode.End, true)]
		public async Task ScrollingToTheEdgeAListDoesNotFollowScrollsThereOnce(ScrollAnchorMode anchorMode, bool animate)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, anchorMode);

			// Only the anchor could move the list as items are added (see TurningAnchorModeOffFromScrolledStopsFollowingTheEnd).
			collectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (anchorMode == ScrollAnchorMode.Start)
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
					collectionView.ScrollToEnd(animate);
					await AssertAtEnd(handler, "ScrollToEnd should reach the end of the content, footer included.", timeout: 5000);
				}
				else
				{
					await AssertAtEnd(handler, "The list should open at its end.");
					collectionView.ScrollToStart(animate);
					var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 5000);
					Assert.True(atStart, $"ScrollToStart should reach the start of the content, header included. Distance from the start: {GetDistanceFromStart(handler)}.");
				}

				await Task.Delay(500);
				Assert.False(collectionView.IsAnchored, "The list shouldn't follow an edge its AnchorMode doesn't select.");

				// It stays where it went: a new last item doesn't move it.
				var position = GetScrollPosition(handler);
				items.Add(new AnchorItem("Added at the end", 40));
				await Task.Delay(500);
				Assert.True(position == GetScrollPosition(handler), "A new last item shouldn't move the list.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "An animated ScrollToStart from the end of a list that follows its end gets there as the content changes")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AnAnimatedScrollToStartFromTheEndGetsThereAsTheContentChanges(bool addItem)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The app scrolls to the start as a chat's last answer streams in. The list is laid out before the animation
				// moves it, and the content changes after that, while the list is still at the end: it carries on to the start.
				collectionView.ScrollToStart(animate: true);
#if IOS || MACCATALYST
				handler.Controller.CollectionView.LayoutIfNeeded();
#endif
				if (addItem)
				{
					items.Add(new AnchorItem("Added at the end", 40));
				}
				else
				{
					items[^1].Height = 120;
				}
#if IOS || MACCATALYST
				handler.Controller.CollectionView.LayoutIfNeeded();
#endif

				var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 5000);
				Assert.True(atStart, $"ScrollToStart should reach the start. Distance from the start: {GetDistanceFromStart(handler)}.");
				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The list should stay at its start. Distance from the start: {GetDistanceFromStart(handler)}.");
				Assert.False(collectionView.IsAnchored, "The list shouldn't follow an edge its AnchorMode doesn't select.");
			});
		}

		[Theory(DisplayName = "An animated ScrollTo from the end of a list that follows its end gets there as the content changes")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AnAnimatedScrollToFromTheEndGetsThereAsTheContentChanges(bool addItem)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// The first item starts the content.
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The app scrolls to the first item as a chat's last answer streams in. The list is laid out before the
				// animation moves it, and the content changes after that, while the list is still at the end: it carries on.
				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
#if IOS || MACCATALYST
				handler.Controller.CollectionView.LayoutIfNeeded();
#endif
				if (addItem)
				{
					items.Add(new AnchorItem("Added at the end", 40));
				}
				else
				{
					items[^1].Height = 120;
				}
#if IOS || MACCATALYST
				handler.Controller.CollectionView.LayoutIfNeeded();
#endif

				var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 5000);
				Assert.True(atStart, $"ScrollTo should reach the first item. Distance from the start: {GetDistanceFromStart(handler)}.");
				await Task.Delay(500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The list should stay at its first item. Distance from the start: {GetDistanceFromStart(handler)}.");
				Assert.False(collectionView.IsAnchored, "The list shouldn't follow an edge its AnchorMode doesn't select.");
			});
		}

		[Fact(DisplayName = "An animated ScrollTo to where a list that follows its end already is doesn't stop it following the end")]
		public async Task AnAnimatedScrollToWhereTheListIsKeepsItFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			// The last item ends the content.
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");
				await Task.Delay(300);

				// The last item is where the app scrolls it already, so nothing animates: the list still follows its end.
				collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: true);
				await Task.Delay(500);
				await AssertAtEnd(handler, "The list should stay at its end.");
				Assert.True(collectionView.IsAnchored, "The list should still follow its end.");

				items[^1].Height = 120;
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1, hasHeader: false) - 120) <= AnchorTolerance, timeout: 3000, message: "The last item should grow.");
				await AssertAtEnd(handler, "Following the end, the list should keep the grown last item's end in view.");
				Assert.True(collectionView.IsAnchored);
			});
		}

#if IOS || MACCATALYST
		// UIKit doesn't animate a scroll of a view that isn't in a window: it scrolls the view there, and reports that the
		// animation ended before the request returns.
		[Theory(DisplayName = "An animated ScrollToStart or ScrollToEnd made while the list isn't in a window gets there, and the list follows its edge again")]
		[InlineData(ScrollAnchorMode.End)]
		[InlineData(ScrollAnchorMode.Start)]
		public async Task AnAnimatedScrollToAnEdgeMadeOutsideTheWindowGetsThere(ScrollAnchorMode anchorMode)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, anchorMode);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				var toEnd = anchorMode == ScrollAnchorMode.Start;
				if (toEnd)
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				}
				else
				{
					await AssertAtEnd(handler, "The list should open at its end.");
				}

				// The app scrolls the list to the edge it doesn't follow while the list is out of its window.
				var platformView = handler.PlatformView;
				var superview = platformView.Superview;
				var index = Array.IndexOf(superview.Subviews, platformView);
				platformView.RemoveFromSuperview();
				if (toEnd)
				{
					collectionView.ScrollToEnd(animate: true);
				}
				else
				{
					collectionView.ScrollToStart(animate: true);
				}

				superview.InsertSubview(platformView, index);

				if (toEnd)
				{
					await AssertAtEnd(handler, "ScrollToEnd should reach the end once the rows there are measured.", timeout: 5000);
				}
				else
				{
					var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 5000);
					Assert.True(atStart, $"ScrollToStart should reach the start. Distance from the start: {GetDistanceFromStart(handler)}.");
				}

				await Task.Delay(500);
				Assert.False(collectionView.IsAnchored, "The list shouldn't follow an edge its AnchorMode doesn't select.");

				if (!toEnd)
				{
					// Back at its end without a request, the list follows its end again: a new last item keeps the end in view.
					for (var attempt = 0; attempt < 5 && GetDistanceToEnd(handler) > AnchorTolerance; attempt++)
					{
						ScrollPlatformViewBy(handler, GetDistanceToEnd(handler));
						await Task.Delay(100);
					}

					await AssertAtEnd(handler, "The list should be back at its end.");
					Assert.True(collectionView.IsAnchored, "Back at its end, the list should follow it.");
					items.Add(new AnchorItem("Added at the end", 40));
					await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 40) <= AnchorTolerance, timeout: 3000, message: "The new item should be laid out in view.");
					await AssertAtEnd(handler, "Following the end, a new last item should keep the end in view.");
				}
			});
		}
#endif

		[Fact(DisplayName = "AnchorMode Start | End follows whichever edge the list is at")]
		public async Task AnchorModeStartAndEndFollowsWhicheverEdgeTheListIsAt()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start | ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");
				Assert.True(collectionView.IsAnchored);

				collectionView.ScrollToStart(animate: true);
				var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 5000);
				Assert.True(atStart, $"ScrollToStart should reach the start. Distance from the start: {GetDistanceFromStart(handler)}.");
				Assert.True(collectionView.IsAnchored, "The list should follow its start.");

				items.Insert(0, new AnchorItem("Newest", 64));
				var newestInView = await Wait(() => Math.Abs(GetItemHeight(handler, 0) - 64) <= AnchorTolerance && GetDistanceFromStart(handler) <= AnchorTolerance, 3000);
				Assert.True(newestInView, $"The new first item should be in view at the start. Position: {GetScrollPosition(handler)}.");

				var position = GetScrollPosition(handler);
				items.Add(new AnchorItem("Added at the end", 40));
				await Task.Delay(500);
				Assert.True(position == GetScrollPosition(handler), "Following the start, a new last item shouldn't move the list.");

				collectionView.ScrollToEnd(animate: true);
				await AssertAtEnd(handler, "ScrollToEnd should reach the end.", timeout: 5000);
				items.Add(new AnchorItem("Added after jumping", 40));
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 40) <= AnchorTolerance, timeout: 3000, message: "The new item should be laid out in view.");
				await AssertAtEnd(handler, "Following the end, a new last item should keep the end in view.");
				Assert.True(collectionView.IsAnchored, "The list should follow its end.");
			});
		}

		[Fact(DisplayName = "Changing AnchorMode starts following the edge it selects")]
		public async Task ChangingAnchorModeStartsFollowingTheEdgeItSelects()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				collectionView.AnchorMode = ScrollAnchorMode.Start;
				var atStart = await Wait(() => GetDistanceFromStart(handler) <= AnchorTolerance, 3000);
				Assert.True(atStart, $"Following the start should show it. Distance from the start: {GetDistanceFromStart(handler)}.");
				Assert.True(collectionView.IsAnchored);

				collectionView.AnchorMode = ScrollAnchorMode.End;
				await AssertAtEnd(handler, "Following the end should show it.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "Changing AnchorMode during an animated ScrollTo shows the edge it selects")]
		[InlineData(ScrollAnchorMode.Start, 50)]
		[InlineData(ScrollAnchorMode.End, 50)]
		[InlineData(ScrollAnchorMode.Start, 0)]
		[InlineData(ScrollAnchorMode.End, 0)]
		public async Task ChangingAnchorModeDuringAnAnimatedScrollToShowsTheEdgeItSelects(ScrollAnchorMode edge, int delay)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// An animated scroll away from the edge the new mode selects.
				if (edge == ScrollAnchorMode.End)
				{
					collectionView.ScrollToEnd(animate: false);
					await AssertAtEnd(handler, "The list should be at its end.");
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
				}
				else
				{
					collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: true);
				}

				// A few frames into the animation, or before its first frame, while the view is still at that edge and nothing
				// needs to scroll it there: the newer change wins.
				if (delay > 0)
				{
					await Task.Delay(delay);
				}

				collectionView.AnchorMode = edge;

				await Task.Delay(1500);
				if (edge == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "Following the end should show it.");
				}
				else
				{
					Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"Following the start should show it. Position: {GetScrollPosition(handler)}.");
				}

				Assert.True(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "Changing AnchorMode as an animated ScrollTo to an item in view moves the view shows the edge it selects")]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		public async Task ChangingAnchorModeAsAnAnimatedScrollToAnItemInViewMovesTheViewShowsTheEdgeItSelects(ScrollAnchorMode edge)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				if (edge == ScrollAnchorMode.End)
				{
					collectionView.ScrollToEnd(animate: false);
					await AssertAtEnd(handler, "The list should be at its end.");
				}

				// The newer change wins over the scroll the app requested, from its first frame away from the edge to its last.
				var changed = false;
				collectionView.Scrolled += (sender, args) =>
				{
					if (!changed && (edge == ScrollAnchorMode.End ? args.VerticalDelta < 0 : args.VerticalDelta > 0))
					{
						changed = true;
						collectionView.AnchorMode = edge;
					}
				};

				// An item in view, most of the 300-unit view from that edge. RecyclerView's smooth scroller finds it as it
				// starts, and stops: RecyclerView settles all of the way to it, as it settles a fling.
				if (edge == ScrollAnchorMode.End)
				{
					collectionView.ScrollTo(items.Count - 6, position: ScrollToPosition.End, animate: true);
				}
				else
				{
					collectionView.ScrollTo(5, position: ScrollToPosition.Start, animate: true);
				}

				await AssertEventually(() => changed, timeout: 3000, message: "The scroll should move the view away from the edge.");
				await Task.Delay(1500);
				if (edge == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "Following the end should show it.");
				}
				else
				{
					Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"Following the start should show it. Position: {GetScrollPosition(handler)}.");
				}

				Assert.True(collectionView.IsAnchored);
			});
		}

		// A list that scrolls horizontally isn't anchored: the mode has no effect on it, and doesn't stop a scroll the app
		// requested.
		[Theory(DisplayName = "Changing AnchorMode during an animated ScrollTo in a horizontal list lets it carry on")]
		[InlineData(ScrollAnchorMode.None)]
		[InlineData(ScrollAnchorMode.Start)]
		public async Task ChangingAnchorModeDuringAnAnimatedScrollToInAHorizontalListLetsItCarryOn(ScrollAnchorMode initialMode)
		{
			SetupBuilder();
			var collectionView = new CollectionView
			{
				AnchorMode = initialMode,
				ItemsLayout = LinearItemsLayout.Horizontal,
				HeightRequest = 100,
				WidthRequest = 300,
				ItemsSource = Enumerable.Range(0, 40).Select(index => $"Item {index}").ToList(),
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { WidthRequest = 100 };
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetHorizontalDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				// Until the list is laid out, it has no content to scroll, and is at both of its edges.
				var distance = double.NaN;
				var laidOut = await Wait(() => (distance = GetHorizontalDistanceToEnd(handler)) > 100, 3000);
				Assert.True(laidOut, $"The items should extend past the end of the list. Distance to the end: {distance}.");

				// From a frame of the scroll's animation, far from the last item. iOS raises Scrolled as it starts the scroll, before
				// the animation.
				var scrolling = false;
				var changed = false;
				var steps = 0;
				collectionView.Scrolled += (sender, args) =>
				{
					if (changed)
					{
						steps++;
					}
					else if (scrolling && args.HorizontalDelta > 0)
					{
						changed = true;
						collectionView.AnchorMode = ScrollAnchorMode.End;
					}
				};

				collectionView.ScrollTo(39, position: ScrollToPosition.End, animate: true);
				scrolling = true;

				await AssertEventually(() => changed, timeout: 3000, message: "The scroll should move the list.");
				var atEnd = await Wait(() => Math.Abs(distance = GetHorizontalDistanceToEnd(handler)) <= AnchorTolerance, 3000);
				Assert.True(atEnd, $"The scroll should carry on to the last item. Distance to the end: {distance}.");

				// On iOS, a scroll stopped short gets there in one step: UIKit lays the view out again with the item it scrolls to
				// where the scroll was taking it.
				Assert.True(steps > 1, $"The scroll should carry on animated. Steps after the change: {steps}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

#if ANDROID
		// Stopping a scroll raises RemainingItemsThresholdReached when it's pending, as RecyclerView goes idle: a scroll the app
		// requests from it is newer than the edge the list goes to.
		// ScrollToStart and ScrollToEnd stop the view whether it follows the edge or not.
		[Theory(DisplayName = "A ScrollTo from RemainingItemsThresholdReached as the list stops a scroll to go to an edge wins")]
		[InlineData("AnchorMode", ScrollAnchorMode.None)]
		[InlineData("ScrollToEnd", ScrollAnchorMode.End)]
		[InlineData("ScrollToEnd", ScrollAnchorMode.None)]
		[InlineData("ScrollToStart", ScrollAnchorMode.Start)]
		[InlineData("ScrollToStart", ScrollAnchorMode.None)]
		public async Task AScrollToFromRemainingItemsThresholdReachedAsTheListStopsAScrollToGoToAnEdgeWins(string stopBy, ScrollAnchorMode anchorMode)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var toStart = stopBy == "ScrollToStart";
			var collectionView = CreateAnchorCollectionView(items, anchorMode);

			// Without a header and footer, the first and last items start and end the content: ScrollTo either scrolls to
			// that edge. The threshold is reached with the tenth item in view.
			collectionView.Header = null;
			collectionView.Footer = null;
			collectionView.RemainingItemsThreshold = 30;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// The scroll goes away from the edge the list then goes to, which the app's ScrollTo takes it back from.
				if (anchorMode == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "The list should open at its end.");
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				}
				else
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
					if (toStart)
					{
						collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: false);
					}
				}

				if (toStart)
				{
					await AssertAtEnd(handler, "The list should be at its end.");
				}
				else
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should be at its start.");
				}

				var stopped = false;
				var scrolledTo = false;
				collectionView.RemainingItemsThresholdReached += (sender, args) =>
				{
					if (stopped && !scrolledTo)
					{
						scrolledTo = true;
						collectionView.ScrollTo(toStart ? items.Count - 1 : 0, position: toStart ? ScrollToPosition.End : ScrollToPosition.Start, animate: false);
					}
				};

				// Past the threshold, which RecyclerView raises once it's idle, the list goes to an edge: that stops the scroll.
				var passedThreshold = false;
				collectionView.Scrolled += (sender, args) =>
				{
					if (stopped)
					{
						return;
					}

					if (passedThreshold)
					{
						stopped = true;
						switch (stopBy)
						{
							case "ScrollToEnd":
								collectionView.ScrollToEnd(animate: false);
								break;
							case "ScrollToStart":
								collectionView.ScrollToStart(animate: false);
								break;
							default:
								collectionView.AnchorMode = ScrollAnchorMode.End;
								break;
						}
					}
					else if (args.LastVisibleItemIndex >= 12 && (toStart ? args.VerticalDelta < 0 : args.VerticalDelta > 0))
					{
						passedThreshold = true;
					}
				};

				if (toStart)
				{
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
				}
				else
				{
					collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: true);
				}

				await AssertEventually(() => stopped, timeout: 3000, message: "The scroll should pass the threshold.");
				Assert.True(scrolledTo, "Stopping the scroll should raise RemainingItemsThresholdReached.");

				await Task.Delay(1500);
				if (toStart)
				{
					var distance = GetDistanceToEnd(handler);
					Assert.True(Math.Abs(distance) <= AnchorTolerance, $"The ScrollTo should show the end. Distance to the end: {distance}.");
				}
				else
				{
					Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The ScrollTo should show the start. Position: {GetScrollPosition(handler)}.");
				}

				Assert.False(collectionView.IsAnchored);
			});
		}

		// A ScrollTo that doesn't animate stops a scroll under way too, before RecyclerView sets the position it jumps to: a
		// ScrollToStart or ScrollToEnd the app requests from the RemainingItemsThresholdReached that raises is newer.
		[Theory(DisplayName = "A ScrollToStart or ScrollToEnd from RemainingItemsThresholdReached as a ScrollTo stops the list wins")]
		[InlineData(ScrollAnchorMode.Start, ScrollAnchorMode.None)]
		[InlineData(ScrollAnchorMode.Start, ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End, ScrollAnchorMode.None)]
		[InlineData(ScrollAnchorMode.End, ScrollAnchorMode.End)]
		public async Task AScrollToStartOrEndFromRemainingItemsThresholdReachedAsAScrollToStopsTheListWins(ScrollAnchorMode edge, ScrollAnchorMode anchorMode)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var toStart = edge == ScrollAnchorMode.Start;
			var collectionView = CreateAnchorCollectionView(items, anchorMode);

			// The threshold is reached with the tenth item in view.
			collectionView.Header = null;
			collectionView.Footer = null;
			collectionView.RemainingItemsThreshold = 30;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// The scroll goes away from the edge the app's ScrollToStart or ScrollToEnd then takes the list back to.
				if (toStart)
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				}
				else
				{
					if (anchorMode != ScrollAnchorMode.End)
					{
						await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
						collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: false);
					}

					await AssertAtEnd(handler, "The list should be at its end.");
				}

				var stopped = false;
				var scrolledToEdge = false;
				collectionView.RemainingItemsThresholdReached += (sender, args) =>
				{
					if (stopped && !scrolledToEdge)
					{
						scrolledToEdge = true;
						if (toStart)
						{
							collectionView.ScrollToStart(animate: false);
						}
						else
						{
							collectionView.ScrollToEnd(animate: false);
						}
					}
				};

				// Past the threshold, which RecyclerView raises once it's idle, the app scrolls to an item without animating, which
				// shows it with the least scrolling: that stops the scroll.
				var passedThreshold = false;
				collectionView.Scrolled += (sender, args) =>
				{
					if (stopped)
					{
						return;
					}

					if (passedThreshold)
					{
						stopped = true;
						collectionView.ScrollTo(20, animate: false);
					}
					else if (args.LastVisibleItemIndex >= 12 && (toStart ? args.VerticalDelta > 0 : args.VerticalDelta < 0))
					{
						passedThreshold = true;
					}
				};

				if (toStart)
				{
					collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: true);
				}
				else
				{
					collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
				}

				await AssertEventually(() => stopped, timeout: 3000, message: "The scroll should pass the threshold.");
				Assert.True(scrolledToEdge, "Stopping the scroll should raise RemainingItemsThresholdReached.");

				await Task.Delay(1500);
				if (toStart)
				{
					Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"ScrollToStart should show the start. Position: {GetScrollPosition(handler)}.");
				}
				else
				{
					var distance = GetDistanceToEnd(handler);
					Assert.True(Math.Abs(distance) <= AnchorTolerance, $"ScrollToEnd should show the end. Distance to the end: {distance}.");
				}

				Assert.Equal(anchorMode != ScrollAnchorMode.None, collectionView.IsAnchored);
			});
		}

		// A layout that doesn't scroll vertically isn't anchored: a change of AnchorMode doesn't scroll it. A ScrollTo that stops a
		// scroll, which raises RemainingItemsThresholdReached, from which the app changes AnchorMode, still goes to its item.
		[Theory(DisplayName = "A ScrollTo that stops a horizontal list goes to its item when the app changes AnchorMode from RemainingItemsThresholdReached")]
		[InlineData(ScrollAnchorMode.None)]
		[InlineData(ScrollAnchorMode.Start)]
		public async Task AScrollToThatStopsAHorizontalListGoesToItsItemWhenTheAppChangesAnchorModeFromRemainingItemsThresholdReached(ScrollAnchorMode anchorMode)
		{
			SetupBuilder();
			var collectionView = new CollectionView
			{
				AnchorMode = anchorMode,
				ItemsLayout = LinearItemsLayout.Horizontal,
				HeightRequest = 100,
				WidthRequest = 300,
				RemainingItemsThreshold = 30,
				ItemsSource = Enumerable.Range(0, 40).Select(index => $"Item {index}").ToList(),
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { WidthRequest = 100 };
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				// How far the start of an item is from the start of the list, if it's laid out.
				double GetItemDistanceFromStart(int position)
				{
					var recyclerView = (RecyclerView)handler.PlatformView;
					var layoutManager = recyclerView.GetLayoutManager();
					return layoutManager.FindViewByPosition(position) is { } child
						? (layoutManager.GetDecoratedLeft(child) - recyclerView.PaddingLeft) / (double)recyclerView.Resources.DisplayMetrics.Density
						: double.PositiveInfinity;
				}

				// Until the list is laid out, it has no content to scroll.
				var laidOut = await Wait(() => GetHorizontalDistanceToEnd(handler) > 100, 3000);
				Assert.True(laidOut, $"The items should extend past the end of the list. Distance to the end: {GetHorizontalDistanceToEnd(handler)}.");

				var stopped = false;
				var changedMode = false;
				collectionView.RemainingItemsThresholdReached += (sender, args) =>
				{
					if (stopped && !changedMode)
					{
						changedMode = true;
						collectionView.AnchorMode = ScrollAnchorMode.End;
					}
				};

				// Past the threshold, which RecyclerView raises once it's idle, the app scrolls to an item without animating: that
				// stops the scroll.
				var passedThreshold = false;
				collectionView.Scrolled += (sender, args) =>
				{
					if (stopped)
					{
						return;
					}

					if (passedThreshold)
					{
						stopped = true;
						collectionView.ScrollTo(5, position: ScrollToPosition.Start, animate: false);
					}
					else if (args.LastVisibleItemIndex >= 12 && args.HorizontalDelta > 0)
					{
						passedThreshold = true;
					}
				};

				collectionView.ScrollTo(39, position: ScrollToPosition.End, animate: true);

				await AssertEventually(() => stopped, timeout: 3000, message: "The scroll should pass the threshold.");
				Assert.True(changedMode, "Stopping the scroll should raise RemainingItemsThresholdReached.");

				await Task.Delay(1000);
				var distance = GetItemDistanceFromStart(5);
				Assert.True(Math.Abs(distance) <= AnchorTolerance, $"The ScrollTo should show its item at the start of the list. Item 5 is {distance} from the start.");
				Assert.False(collectionView.IsAnchored);
			});
		}
#endif

		[Theory(DisplayName = "Changing AnchorMode as ScrollToEnd reports following the end shows the edge it selects")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ChangingAnchorModeAsScrollToEndReportsFollowingTheEndShowsTheEdgeItSelects(bool animate)
		{
			SetupBuilder();
			var collectionView = CreateAnchorCollectionView(CreateAnchorItems(40), ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");
				ScrollPlatformViewBy(handler, -200);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the end should stop following it.");

				// The app hears that the list follows its end again, and follows its start instead: the newer change wins.
				var changedMode = false;
				collectionView.PropertyChanged += (_, e) =>
				{
					if (!changedMode && e.PropertyName == StructuredItemsView.IsAnchoredProperty.PropertyName && collectionView.IsAnchored)
					{
						changedMode = true;
						collectionView.AnchorMode = ScrollAnchorMode.Start;
					}
				};

				collectionView.ScrollToEnd(animate);
				Assert.True(changedMode, "ScrollToEnd should report that the list follows its end.");

				// Long enough for an animated scroll to the end to have ended, had it carried on.
				await Task.Delay(1500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"Following the start should show it. Position: {GetScrollPosition(handler)}.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "Turning AnchorMode off and scrolling as ScrollToStart or ScrollToEnd reports following the edge wins")]
		[InlineData(ScrollAnchorMode.Start, false)]
		[InlineData(ScrollAnchorMode.Start, true)]
		[InlineData(ScrollAnchorMode.End, false)]
		[InlineData(ScrollAnchorMode.End, true)]
		public async Task TurningAnchorModeOffAndScrollingAsScrollingToTheEdgeReportsFollowingItWins(ScrollAnchorMode edge, bool animate)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, edge);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (edge == ScrollAnchorMode.End)
				{
					await AssertAtEnd(handler, "The list should open at its end.");
					ScrollPlatformViewBy(handler, -200);
				}
				else
				{
					await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
					ScrollPlatformViewBy(handler, 200);
				}

				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the edge should stop following it.");

				// The app hears that the list follows the edge again, turns anchoring off and scrolls to the item at the other end:
				// that's newer than the animated scroll to the edge, also once nothing anchors the list.
				var scrolled = false;
				collectionView.PropertyChanged += (_, e) =>
				{
					if (!scrolled && e.PropertyName == StructuredItemsView.IsAnchoredProperty.PropertyName && collectionView.IsAnchored)
					{
						scrolled = true;
						collectionView.AnchorMode = ScrollAnchorMode.None;
						if (edge == ScrollAnchorMode.End)
						{
							collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: animate);
						}
						else
						{
							collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: animate);
						}
					}
				};

				if (edge == ScrollAnchorMode.End)
				{
					collectionView.ScrollToEnd(animate: true);
				}
				else
				{
					collectionView.ScrollToStart(animate: true);
				}

				Assert.True(scrolled, "Scrolling to the edge should report that the list follows it.");

				// Long enough for the animated scroll to the edge to have ended, had it carried on: it would replace an animated
				// ScrollTo.
				await Task.Delay(1500);
				var distance = edge == ScrollAnchorMode.End ? GetDistanceToEnd(handler) : GetDistanceFromStart(handler);
				Assert.True(distance > 300, $"The list should show the item the later ScrollTo scrolled to, not the edge. Distance from the edge: {distance}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "Changing AnchorMode from Scrolled as ScrollToEnd lays the list out wins")]
		public async Task ChangingAnchorModeFromScrolledAsScrollToEndLaysTheListOutWins()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The last row grows, and the list follows the end as it lays the row out, which raises Scrolled: the app follows
				// the start from there instead. That's newer than the ScrollToEnd that has the list laid out, and wins.
				var changedMode = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!changedMode)
					{
						changedMode = true;
						collectionView.AnchorMode = ScrollAnchorMode.Start;
					}
				};

				items[^1].Height = 120;
				collectionView.ScrollToEnd(animate: true);
				await AssertEventually(() => changedMode, timeout: 3000, message: "The list should follow the end as the last row grows.");

				// Long enough for an animated scroll to the end to have ended, had it carried on.
				await Task.Delay(1500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"Following the start should show it. Position: {GetScrollPosition(handler)}.");
				Assert.True(collectionView.IsAnchored);
			});
		}

#if IOS || MACCATALYST
		// ScrollToStart and ScrollToEnd lay the view out before they scroll (they scroll to the edge as laid out): the app can
		// turn anchoring off from the Scrolled event that layout raises, before the request scrolls. On Android a view that
		// follows the end starts the animated scroll right away, and its anchor follows the end the row grows to as the layout
		// after it grows the row: once anchoring is off, nothing scrolls on to that end.
		[Fact(DisplayName = "Turning AnchorMode off from Scrolled as ScrollToEnd lays the list out stops following, and the scroll carries on")]
		public async Task TurningAnchorModeOffFromScrolledAsScrollToEndLaysTheListOutStopsFollowing()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The last row grows, and the list follows the end as it lays the row out, which raises Scrolled: the app turns
				// anchoring off from there. That only stops following: the ScrollToEnd the app requested still goes to the end,
				// as it would had the app turned anchoring off after it, and the list doesn't follow the end from there.
				var turnedOff = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!turnedOff)
					{
						turnedOff = true;
						collectionView.AnchorMode = ScrollAnchorMode.None;
					}
				};

				items[^1].Height = 120;
				collectionView.ScrollToEnd(animate: true);
				await AssertEventually(() => turnedOff, timeout: 3000, message: "The list should follow the end as the last row grows.");
				await AssertAtEnd(handler, "The ScrollToEnd the app requested should carry on to the end.", timeout: 5000);

				// Long enough for the animated scroll to the end to have ended.
				await Task.Delay(1500);
				Assert.False(collectionView.IsAnchored, "Turning anchoring off should stop following the end.");
				Assert.Equal(ScrollAnchorMode.None, collectionView.AnchorMode);

				items.Add(new AnchorItem("Added", 40));
				await Task.Delay(500);
				Assert.True(GetDistanceToEnd(handler) > AnchorTolerance, $"With anchoring off, the list shouldn't follow a new last item. Distance to the end: {GetDistanceToEnd(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}
#endif

		[Fact(DisplayName = "An animated ScrollToStart right after ScrollToEnd stays at the start")]
		public async Task AnAnimatedScrollToStartRightAfterScrollToEndStaysAtTheStart()
		{
			SetupBuilder();

			// One item taller than the view: scrolling to it lines up its start, and ScrollToEnd scrolls the rest of the way
			// once it's laid out.
			var collectionView = CreateAnchorCollectionView(new ObservableCollection<AnchorItem> { new("Tall", 600) }, ScrollAnchorMode.None);
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				collectionView.ScrollToEnd(animate: false);
				collectionView.ScrollToStart(animate: true);

				// Long enough for the animation, and for what ScrollToEnd left for the next layout, to have run.
				await Task.Delay(1500);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The later ScrollToStart should win. Position: {GetScrollPosition(handler)}.");
			});
		}

		// The list is already at the edge the animated scroll goes to: RecyclerView's smooth scroller finds its target in place,
		// and stops before the layout that lands the jump to the other edge. Without a header and footer, the rows all the same
		// height, RecyclerView estimates the content exactly: the anchor measures no change in size after the jump either.
		[Theory(DisplayName = "An animated ScrollToStart or ScrollToEnd right after one to the other edge that doesn't animate wins")]
		[InlineData(ScrollAnchorMode.None, ScrollAnchorMode.Start, true)]
		[InlineData(ScrollAnchorMode.None, ScrollAnchorMode.End, true)]
		[InlineData(ScrollAnchorMode.Start, ScrollAnchorMode.Start, true)]
		[InlineData(ScrollAnchorMode.Start, ScrollAnchorMode.End, true)]
		[InlineData(ScrollAnchorMode.End, ScrollAnchorMode.Start, true)]
		[InlineData(ScrollAnchorMode.End, ScrollAnchorMode.End, true)]
		[InlineData(ScrollAnchorMode.Start | ScrollAnchorMode.End, ScrollAnchorMode.Start, true)]
		[InlineData(ScrollAnchorMode.Start | ScrollAnchorMode.End, ScrollAnchorMode.End, true)]
		[InlineData(ScrollAnchorMode.None, ScrollAnchorMode.Start, false)]
		[InlineData(ScrollAnchorMode.None, ScrollAnchorMode.End, false)]
		[InlineData(ScrollAnchorMode.Start, ScrollAnchorMode.Start, false)]
		[InlineData(ScrollAnchorMode.Start, ScrollAnchorMode.End, false)]
		[InlineData(ScrollAnchorMode.End, ScrollAnchorMode.Start, false)]
		[InlineData(ScrollAnchorMode.End, ScrollAnchorMode.End, false)]
		[InlineData(ScrollAnchorMode.Start | ScrollAnchorMode.End, ScrollAnchorMode.Start, false)]
		[InlineData(ScrollAnchorMode.Start | ScrollAnchorMode.End, ScrollAnchorMode.End, false)]
		public async Task AnAnimatedScrollToAnEdgeRightAfterAJumpToTheOtherEdgeWins(ScrollAnchorMode anchorMode, ScrollAnchorMode edge, bool headerAndFooter)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, anchorMode);
			if (!headerAndFooter)
			{
				collectionView.Header = null;
				collectionView.Footer = null;
			}

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				var toStart = edge == ScrollAnchorMode.Start;
				bool AtEdge() => toStart ? GetDistanceFromStart(handler) <= AnchorTolerance : Math.Abs(GetDistanceToEnd(handler)) <= AnchorTolerance;
				if (toStart)
				{
					collectionView.ScrollToStart(animate: false);
				}
				else
				{
					collectionView.ScrollToEnd(animate: false);
				}

				await AssertEventually(AtEdge, timeout: 3000, message: "The list should be at the edge.");

				// Settled there: laid out where it is, and looked at.
				await Task.Delay(300);

				if (toStart)
				{
					collectionView.ScrollToEnd(animate: false);
					collectionView.ScrollToStart(animate: true);
				}
				else
				{
					collectionView.ScrollToStart(animate: false);
					collectionView.ScrollToEnd(animate: true);
				}

				var reached = await Wait(AtEdge, 5000);
				Assert.True(reached, $"The later animated scroll should win. Position: {GetScrollPosition(handler)}.");

				// Nothing the earlier request left takes the list away again.
				await Task.Delay(500);
				Assert.True(AtEdge(), $"The list should stay at the edge. Position: {GetScrollPosition(handler)}.");
				Assert.Equal(anchorMode.HasFlag(edge), collectionView.IsAnchored);
			});
		}

		// An animated ScrollTo for an item, rather than for an edge: RecyclerView's smooth scroller finds the item in place and
		// stops before the layout that lands the jump to the other edge, which would then take the list there.
		[Theory(DisplayName = "An animated ScrollTo right after a jump to the other edge wins")]
		[InlineData(ScrollAnchorMode.None)]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		[InlineData(ScrollAnchorMode.Start | ScrollAnchorMode.End)]
		public async Task AnAnimatedScrollToRightAfterAJumpToTheOtherEdgeWins(ScrollAnchorMode anchorMode)
		{
			SetupBuilder();
			var collectionView = CreateAnchorCollectionView(CreateAnchorItems(40), anchorMode);
			collectionView.Header = null;
			collectionView.Footer = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				bool AtStart() => GetDistanceFromStart(handler) <= AnchorTolerance;
				collectionView.ScrollToStart(animate: false);
				await AssertEventually(AtStart, timeout: 3000, message: "The list should be at its start.");

				// Settled there: laid out where it is, and looked at.
				await Task.Delay(300);

				collectionView.ScrollToEnd(animate: false);
				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: true);

				// The list can show the end first, and scroll back from there.
				await Task.Delay(300);
				var reached = await Wait(AtStart, 5000);
				Assert.True(reached, $"The later ScrollTo should win. Position: {GetScrollPosition(handler)}.");

				// Nothing the earlier request left takes the list away again.
				await Task.Delay(500);
				Assert.True(AtStart(), $"The list should stay at its start. Position: {GetScrollPosition(handler)}.");
				Assert.Equal(anchorMode.HasFlag(ScrollAnchorMode.Start), collectionView.IsAnchored);
			});
		}

		// A horizontal list isn't anchored: ScrollToStart and ScrollToEnd scroll it to its first and last item. RecyclerView's
		// smooth scroller finds the item at the edge already in place, and stops before the layout that lands the jump to the
		// other edge, which would then take the list there.
		[Theory(DisplayName = "An animated ScrollToStart or ScrollToEnd right after a jump to the other edge of a horizontal list wins")]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		public async Task AnAnimatedScrollToAnEdgeRightAfterAJumpToTheOtherEdgeOfAHorizontalListWins(ScrollAnchorMode edge)
		{
			SetupBuilder();
			var collectionView = new CollectionView
			{
				ItemsLayout = LinearItemsLayout.Horizontal,
				HeightRequest = 100,
				WidthRequest = 300,
				ItemsSource = Enumerable.Range(0, 20).Select(index => $"Item {index}").ToList(),
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { WidthRequest = 100 };
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				var toStart = edge == ScrollAnchorMode.Start;
				bool AtEdge() => toStart
					? GetHorizontalDistanceFromStart(handler) <= AnchorTolerance
					: Math.Abs(GetHorizontalDistanceToEnd(handler)) <= AnchorTolerance;

				// Until the list is laid out, it has no content to scroll, and is at both of its edges.
				var laidOut = await Wait(() => GetHorizontalDistanceToEnd(handler) > 100, 3000);
				Assert.True(laidOut, $"The items should extend past the end of the list. Distance to the end: {GetHorizontalDistanceToEnd(handler)}.");
				if (toStart)
				{
					collectionView.ScrollToStart(animate: false);
				}
				else
				{
					collectionView.ScrollToEnd(animate: false);
				}

				await AssertEventually(AtEdge, timeout: 3000, message: "The list should be at the edge.");

				// Settled there: laid out where it is.
				await Task.Delay(300);

				if (toStart)
				{
					collectionView.ScrollToEnd(animate: false);
					collectionView.ScrollToStart(animate: true);
				}
				else
				{
					collectionView.ScrollToStart(animate: false);
					collectionView.ScrollToEnd(animate: true);
				}

				// The list can show the other edge first, and scroll back from there.
				await Task.Delay(300);
				var reached = await Wait(AtEdge, 5000);
				Assert.True(reached, $"The later animated scroll should win. Distance from the start: {GetHorizontalDistanceFromStart(handler)}, to the end: {GetHorizontalDistanceToEnd(handler)}.");

				// Nothing the earlier request left takes the list away again.
				await Task.Delay(500);
				Assert.True(AtEdge(), $"The list should stay at the edge. Distance from the start: {GetHorizontalDistanceFromStart(handler)}, to the end: {GetHorizontalDistanceToEnd(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

#if MACCATALYST
		// Mac Catalyst can move the content offset without telling the scroll view delegate or laying the view out, as when a
		// window is restored (see #34271): only an observer of the offset sees it. Following the end replaces the scroll back to
		// the item of an earlier ScrollTo that MAUI makes then.
		[Fact(DisplayName = "AnchorMode End follows the end again after Mac Catalyst moves the view silently")]
		public async Task AnchorModeEndFollowsTheEndAgainAfterMacCatalystMovesTheViewSilently()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				collectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End, animate: false);
				collectionView.AnchorMode = ScrollAnchorMode.End;
				await AssertAtEnd(handler, "AnchorMode End should show the end.");
				await AssertEventually(() => collectionView.IsAnchored, timeout: 3000, message: "The list should follow the end.");

				// Move the view as UIKit does then: its bounds change under the observer, and the delegate hears of no scroll.
				var platformView = handler.Controller.CollectionView;
				var scrolled = false;
				collectionView.Scrolled += (_, _) => scrolled = true;
				var bounds = platformView.Layer.Bounds;
				platformView.WillChangeValue("contentOffset");
				platformView.Layer.Bounds = new CoreGraphics.CGRect(bounds.X, bounds.Y - 200, bounds.Width, bounds.Height);
				platformView.DidChangeValue("contentOffset");
				Assert.False(scrolled, "The move should be one the scroll view delegate doesn't hear of.");
				Assert.True(GetDistanceToEnd(handler) > 100, $"The move should take the view away from the end. Distance to the end: {GetDistanceToEnd(handler)}.");

				await AssertAtEnd(handler, "The list should follow the end again after a move it wasn't told about.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		// After a ScrollTo that doesn't animate, MAUI scrolls back to its item when Mac Catalyst moves the view up by itself (see
		// #34271). Back at the end without a drag, as the keyboard or accessibility scroll, the list follows the end again: the
		// view moving up as the last item shrinks is the anchor keeping the end in view, not a move to undo.
		[Fact(DisplayName = "Back at the end after a ScrollTo away, the list keeps the end in view as the last item shrinks")]
		public async Task BackAtTheEndAfterAScrollToAwayTheListKeepsTheEndInViewAsTheLastItemShrinks()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			items[^1].Height = 120;
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "ScrollTo away from the end should stop following it.");

				// Back to the end in scrolls that aren't drags, as the keyboard or accessibility make.
				for (var attempt = 0; attempt < 10 && !collectionView.IsAnchored; attempt++)
				{
					ScrollPlatformViewBy(handler, GetDistanceToEnd(handler));
					await Task.Delay(100);
				}

				Assert.True(collectionView.IsAnchored, $"Back at its end, the list should follow it again. Distance to the end: {GetDistanceToEnd(handler)}.");

				items[^1].Height = 40;
				var shrunk = await Wait(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 40) <= AnchorTolerance, 3000);
				Assert.True(shrunk, $"The last item should shrink in view. Distance from the start: {GetDistanceFromStart(handler)}, to the end: {GetDistanceToEnd(handler)}, anchored: {collectionView.IsAnchored}.");
				await Task.Delay(500);
				await AssertAtEnd(handler, "Following the end, the list should keep the end in view as the last item shrinks.");
				Assert.True(collectionView.IsAnchored);
			});
		}
#endif

		// A scroll the user isn't dragging, as accessibility or the keyboard can make, right as the content changes size, is still
		// a scroll. RecyclerView applies the new item as the scroll starts, outside a layout: the look after the scroll counts the
		// move as the scroll's. UICollectionView resizes its content as the item is inserted, which the anchor looks at as the
		// update ends, before the scroll.
		[Fact(DisplayName = "Scrolling away as an item is added stops following the end")]
		public async Task ScrollingAwayAsAnItemIsAddedStopsFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The move is a scroll away from the end, not the platform laying out the new item.
				items.Add(new AnchorItem("Added", 40));
				ScrollPlatformViewBy(handler, -200);

				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the end should stop following it.");
				await Task.Delay(500);
				Assert.True(GetDistanceToEnd(handler) > 100, $"The list shouldn't be pulled back to its end. Position: {GetScrollPosition(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

#if ANDROID
		// Where the first row isn't laid out, RecyclerView only estimates how far the view is from the start, from the rows
		// that are. Tall rows followed by short ones make that estimate, at the end, a tenth of the distance.
		[Fact(DisplayName = "Scrolling back from the end past where an estimate puts the start doesn't follow the start")]
		public async Task ScrollingBackFromTheEndPastWhereAnEstimatePutsTheStartDoesNotFollowTheStart()
		{
			SetupBuilder();
			var items = new ObservableCollection<AnchorItem>(Enumerable.Range(0, 80).Select(index => new AnchorItem($"Item {index}", index < 60 ? 200 : 20)));
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start | ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// Back further than the estimate puts the start, in steps that each keep rows of the one before in view.
				for (var step = 0; step < 20; step++)
				{
					ScrollPlatformViewBy(handler, -100);
					await Task.Delay(16);
				}

				await Task.Delay(500);
				Assert.False(collectionView.IsAnchored, $"The list is far from both edges: it shouldn't follow either. Position: {GetScrollPosition(handler)}.");
				var distance = GetDistanceFromStart(handler);
				Assert.True(distance > 5000, $"The list shouldn't jump to its start. Distance from the start: {distance}.");
			});
		}

		// MandatorySingle snap points remember the item a fling snaps to, and snap the view back to it when a later scroll settles
		// with that item in view.
		[Theory(DisplayName = "An animated ScrollToStart or ScrollToEnd after a fling to a snap point stays at the edge")]
		[InlineData(ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.End)]
		public async Task AnAnimatedScrollToTheEdgeAfterAFlingToASnapPointStaysAtTheEdge(ScrollAnchorMode edge)
		{
			SetupBuilder();

			// The view shows 300 of a header of 50, six rows of 100 and a footer of 30.
			var items = CreateAnchorItems(6);
			foreach (var item in items)
			{
				item.Height = 100;
			}

			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);
			collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center,
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				var recyclerView = (RecyclerView)handler.PlatformView;
				var toEnd = edge == ScrollAnchorMode.End;
				double GetDistanceFromEdge() => toEnd ? GetDistanceToEnd(handler) : GetDistanceFromStart(handler);

				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");
				if (!toEnd)
				{
					collectionView.ScrollToEnd(animate: false);
					await AssertAtEnd(handler, "ScrollToEnd should show the end.");
				}

				// A fling from the other edge snaps the row past the middle of the view in its direction to the middle: that row
				// is in view at the edge the list then scrolls to.
				ScrollPlatformViewBy(handler, toEnd ? 160 : -160);
				var flungFrom = GetScrollPosition(handler);
				var velocity = (int)(2000 * recyclerView.Resources.DisplayMetrics.Density);
				Assert.True(recyclerView.Fling(0, toEnd ? velocity : -velocity), "The snap points should take the fling.");
				await AssertEventually(() => GetScrollPosition(handler) != flungFrom && recyclerView.ScrollState == RecyclerView.ScrollStateIdle, timeout: 3000, message: "The fling should snap a row to the middle.");
				Assert.True(GetDistanceFromEdge() > AnchorTolerance, $"The fling should leave the list away from the edge. Distance from the edge: {GetDistanceFromEdge()}.");

				if (toEnd)
				{
					collectionView.ScrollToEnd(animate: true);
				}
				else
				{
					collectionView.ScrollToStart(animate: true);
				}

				var distance = double.NaN;
				var reached = await Wait(() => Math.Abs(distance = GetDistanceFromEdge()) <= AnchorTolerance, 3000);
				Assert.True(reached, $"The scroll should reach the edge. Distance from the edge: {distance}.");
				await AssertEventually(() => recyclerView.ScrollState == RecyclerView.ScrollStateIdle, timeout: 3000, message: "The scroll should settle.");
				await Task.Delay(1000);
				distance = GetDistanceFromEdge();
				Assert.True(Math.Abs(distance) <= AnchorTolerance, $"The snap points shouldn't take the list back to the row the fling snapped to. Distance from the edge: {distance}.");
				Assert.False(collectionView.IsAnchored);
			});
		}
#endif

		[Fact(DisplayName = "A ScrollTo from Scrolled as ItemsSource is replaced wins")]
		public async Task AScrollToFromScrolledAsItemsSourceIsReplacedWins()
		{
			SetupBuilder();
			var collectionView = CreateAnchorCollectionView(CreateAnchorItems(40), ScrollAnchorMode.End);

			// ScrollTo(0) shows the first item at the top: with no header, that's the start of the content.
			collectionView.Header = null;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// Away from the end, so the new items scroll the view as they replace the old ones.
				ScrollPlatformViewBy(handler, -200);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the end should stop following it.");

				// The app asks for the start as soon as the new items scroll the view: the newer request wins over showing their
				// end.
				var scrolledToStart = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!scrolledToStart)
					{
						scrolledToStart = true;
						collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
					}
				};

				collectionView.ItemsSource = new ObservableCollection<AnchorItem>(Enumerable.Range(0, 40).Select(index => new AnchorItem($"New {index}", 50)));
				await AssertEventually(() => scrolledToStart, timeout: 3000, message: "Replacing the items should scroll the list.");
				await Task.Delay(1000);
				Assert.True(GetDistanceFromStart(handler) <= AnchorTolerance, $"The ScrollTo should win. Position: {GetScrollPosition(handler)}.");
				Assert.False(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "A ScrollTo before the list follows the end doesn't take it back to that item")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task AScrollToBeforeTheListFollowsTheEndDoesNotTakeItBackToThatItem(bool byAnchorMode)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, byAnchorMode ? ScrollAnchorMode.None : ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (!byAnchorMode)
				{
					await AssertAtEnd(handler, "The list should open at its end.");
				}

				// Mac Catalyst keeps the item a ScrollTo that doesn't animate scrolled to in view when the view is moved back
				// from it: once the list follows its end, that's over.
				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(() => GetDistanceToEnd(handler) > 100 && !collectionView.IsAnchored, timeout: 3000, message: "The list should show its first item.");

				if (byAnchorMode)
				{
					collectionView.AnchorMode = ScrollAnchorMode.End;
				}
				else
				{
					collectionView.ScrollToEnd(animate: false);
				}

				await AssertAtEnd(handler, "The list should follow its end.");

				// The end moves up by more than the distance that sets off that restore, and the items stay as many as before.
				items[^1].Height = 160;
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 160) <= AnchorTolerance, timeout: 3000, message: "The last item should grow.");
				await AssertAtEnd(handler, "The list should follow the end as the last item grows.");
				items[^1].Height = 40;
				await AssertEventually(() => !ShowsItemOfHeight(handler, 160), timeout: 3000, message: "The last item should shrink.");

				// Long enough for a restore the shrink set off to have run.
				await Task.Delay(500);
				await AssertAtEnd(handler, "The list should show its end, not the item it scrolled to before following it.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		[Theory(DisplayName = "Following the end from Scrolled as a ScrollTo scrolls the list doesn't take it back to that item")]
		[InlineData(true, false)]
		[InlineData(false, false)]
		[InlineData(false, true)]
		public async Task FollowingTheEndFromScrolledAsAScrollToScrollsTheListDoesNotTakeItBackToThatItem(bool byAnchorMode, bool animate)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, byAnchorMode ? ScrollAnchorMode.None : ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				if (!byAnchorMode)
				{
					await AssertAtEnd(handler, "The list should open at its end.");
				}

				// The app follows the end again as soon as a ScrollTo that doesn't animate scrolls the list: that's newer than the
				// ScrollTo, whose item Mac Catalyst would otherwise keep in view when the view is moved back from it.
				var followed = false;
				collectionView.Scrolled += (_, _) =>
				{
					if (!followed)
					{
						followed = true;
						if (byAnchorMode)
						{
							collectionView.AnchorMode = ScrollAnchorMode.End;
						}
						else
						{
							collectionView.ScrollToEnd(animate);
						}
					}
				};

				collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
				await AssertEventually(() => followed, timeout: 3000, message: "The ScrollTo should scroll the list.");
				await AssertAtEnd(handler, "The list should follow its end.", timeout: 5000);

				// The end moves up by more than the distance that sets off that restore, and the items stay as many as before.
				items[^1].Height = 160;
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 160) <= AnchorTolerance, timeout: 3000, message: "The last item should grow.");
				await AssertAtEnd(handler, "The list should follow the end as the last item grows.");
				items[^1].Height = 40;
				await AssertEventually(() => !ShowsItemOfHeight(handler, 160), timeout: 3000, message: "The last item should shrink.");

				// Long enough for a restore the shrink set off to have run.
				await Task.Delay(500);
				await AssertAtEnd(handler, "The list should show its end, not the item the ScrollTo scrolled to.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		[Fact(DisplayName = "Replacing ItemsSource follows the end of the new items")]
		public async Task ReplacingItemsSourceFollowsTheEndOfTheNewItems()
		{
			SetupBuilder();
			var collectionView = CreateAnchorCollectionView(CreateAnchorItems(40), ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");
				ScrollPlatformViewBy(handler, -200);
				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the end should stop following it.");

				// As many new items, of another height: new items show from their end, as a new list does.
				var items = new ObservableCollection<AnchorItem>(Enumerable.Range(0, 40).Select(index => new AnchorItem($"New {index}", 50)));
				collectionView.ItemsSource = items;
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 50) <= AnchorTolerance, timeout: 3000, message: "The new items should be laid out at the end.");
				await AssertAtEnd(handler, "The new items should show from their end.");
				Assert.True(collectionView.IsAnchored, "The list should follow the end of the new items.");

				items.Add(new AnchorItem("Added", 50));
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 50) <= AnchorTolerance, timeout: 3000, message: "The new item should be laid out in view.");
				await AssertAtEnd(handler, "A new item should keep the end in view.");
				Assert.True(collectionView.IsAnchored);
			});
		}

		static CollectionView CreateAnchorCollectionView(System.Collections.IEnumerable itemsSource, ScrollAnchorMode anchorMode) => new()
		{
			AnchorMode = anchorMode,
			HeightRequest = 300,
			ItemsSource = itemsSource,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, nameof(AnchorItem.Text));
				label.SetBinding(VisualElement.HeightRequestProperty, nameof(AnchorItem.Height));
				return label;
			}),
			Header = new Label { Text = "Header", HeightRequest = 50 },
			Footer = new Label { Text = "Footer", HeightRequest = 30 },
		};

		static ObservableCollection<AnchorItem> CreateAnchorItems(int count) =>
			new(Enumerable.Range(0, count).Select(index => new AnchorItem($"Item {index}", 40)));

		static CollectionView CreateHorizontalGroupedCollectionView(System.Collections.IEnumerable groups, bool groupHeaders) => new()
		{
			IsGrouped = true,
			ItemsLayout = LinearItemsLayout.Horizontal,
			HeightRequest = 100,
			WidthRequest = 300,
			ItemsSource = groups,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { WidthRequest = 100 };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),
			GroupHeaderTemplate = groupHeaders ? new DataTemplate(() => new Label { Text = "Group", WidthRequest = 60 }) : null,
		};

		// How far the content extends past the right of the viewport, in device-independent units.
		static double GetHorizontalDistanceToEnd(CollectionViewHandler handler)
		{
#if IOS || MACCATALYST
			var collectionView = handler.Controller.CollectionView;
			var inset = collectionView.AdjustedContentInset;
			var end = Math.Max(-inset.Left, collectionView.ContentSize.Width + inset.Right - collectionView.Bounds.Width);
			return end - collectionView.ContentOffset.X;
#else
			var recyclerView = (RecyclerView)handler.PlatformView;
			var layoutManager = recyclerView.GetLayoutManager();
			var last = layoutManager.ItemCount - 1;
			for (var index = 0; index < layoutManager.ChildCount; index++)
			{
				var child = layoutManager.GetChildAt(index);
				if (layoutManager.GetPosition(child) == last)
				{
					return (layoutManager.GetDecoratedRight(child) - (recyclerView.Width - recyclerView.PaddingRight)) / (double)recyclerView.Resources.DisplayMetrics.Density;
				}
			}

			return double.PositiveInfinity;
#endif
		}

		// How far the content extends left of the viewport, in device-independent units.
		static double GetHorizontalDistanceFromStart(CollectionViewHandler handler)
		{
#if IOS || MACCATALYST
			var collectionView = handler.Controller.CollectionView;
			return collectionView.ContentOffset.X + collectionView.AdjustedContentInset.Left;
#else
			var recyclerView = (RecyclerView)handler.PlatformView;
			var layoutManager = recyclerView.GetLayoutManager();
			for (var index = 0; index < layoutManager.ChildCount; index++)
			{
				var child = layoutManager.GetChildAt(index);
				if (layoutManager.GetPosition(child) == 0)
				{
					return (recyclerView.PaddingLeft - layoutManager.GetDecoratedLeft(child)) / (double)recyclerView.Resources.DisplayMetrics.Density;
				}
			}

			return double.PositiveInfinity;
#endif
		}

		// How many items the platform view shows, in all its sections.
		static int GetPlatformItemCount(CollectionViewHandler handler)
		{
#if IOS || MACCATALYST
			var collectionView = handler.Controller.CollectionView;
			var count = 0;
			for (var section = 0; section < (int)collectionView.NumberOfSections(); section++)
			{
				count += (int)collectionView.NumberOfItemsInSection(section);
			}

			return count;
#else
			return ((RecyclerView)handler.PlatformView).GetAdapter()?.ItemCount ?? 0;
#endif
		}

		static DataTemplate CreateAnchorGroupHeaderTemplate() => new(() =>
		{
			var label = new Label { HeightRequest = 24 };
			label.SetBinding(Label.TextProperty, nameof(AnchorGroup.Name));
			return label;
		});

		static async Task AssertAtEnd(CollectionViewHandler handler, string message, int timeout = 3000)
		{
			var distance = double.NaN;
			var atEnd = await Wait(() => Math.Abs(distance = GetDistanceToEnd(handler)) <= AnchorTolerance, timeout);
			Assert.True(atEnd, $"{message} Distance to the end: {distance}.");
		}

		// How far the content extends past the bottom of the viewport, in device-independent units.
		static double GetDistanceToEnd(CollectionViewHandler handler)
		{
#if IOS || MACCATALYST
			var collectionView = handler.Controller.CollectionView;
			var inset = collectionView.AdjustedContentInset;
			var end = Math.Max(-inset.Top, collectionView.ContentSize.Height + inset.Bottom - collectionView.Bounds.Height);
			return end - collectionView.ContentOffset.Y;
#else
			var recyclerView = (RecyclerView)handler.PlatformView;
			var layoutManager = recyclerView.GetLayoutManager();
			var last = layoutManager.ItemCount - 1;
			var lastLaidOut = false;
			var bottom = int.MinValue;
			for (var index = 0; index < layoutManager.ChildCount; index++)
			{
				var child = layoutManager.GetChildAt(index);
				lastLaidOut |= layoutManager.GetPosition(child) == last;
				bottom = Math.Max(bottom, layoutManager.GetDecoratedBottom(child));
			}

			return lastLaidOut
				? (bottom - (recyclerView.Height - recyclerView.PaddingBottom)) / (double)recyclerView.Resources.DisplayMetrics.Density
				: double.PositiveInfinity;
#endif
		}

		// How far the content extends above the top of the viewport, in device-independent units.
		static double GetDistanceFromStart(CollectionViewHandler handler)
		{
#if IOS || MACCATALYST
			var collectionView = handler.Controller.CollectionView;
			return collectionView.ContentOffset.Y + collectionView.AdjustedContentInset.Top;
#else
			var recyclerView = (RecyclerView)handler.PlatformView;
			var layoutManager = recyclerView.GetLayoutManager();
			for (var index = 0; index < layoutManager.ChildCount; index++)
			{
				var child = layoutManager.GetChildAt(index);
				if (layoutManager.GetPosition(child) == 0)
				{
					return (recyclerView.PaddingTop - layoutManager.GetDecoratedTop(child)) / (double)recyclerView.Resources.DisplayMetrics.Density;
				}
			}

			return double.PositiveInfinity;
#endif
		}

		// Where the view is scrolled to, to tell whether a change moved it.
		static string GetScrollPosition(CollectionViewHandler handler)
		{
#if IOS || MACCATALYST
			var collectionView = handler.Controller.CollectionView;
			return Math.Round((double)collectionView.ContentOffset.Y, 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
#else
			// The first row laid out, and where it is.
			var layoutManager = ((RecyclerView)handler.PlatformView).GetLayoutManager();
			var first = int.MaxValue;
			var top = 0;
			for (var index = 0; index < layoutManager.ChildCount; index++)
			{
				var child = layoutManager.GetChildAt(index);
				var position = layoutManager.GetPosition(child);
				if (position < first)
				{
					first = position;
					top = layoutManager.GetDecoratedTop(child);
				}
			}

			return $"{first}:{top}";
#endif
		}

		// The laid out height of the item at an index of an ungrouped list, in device-independent units, or NaN while it isn't
		// laid out.
		static double GetItemHeight(CollectionViewHandler handler, int index, bool hasHeader = true)
		{
#if IOS || MACCATALYST
			// The header is a supplementary view: the items are the first section's. Not disposed: the managed index path is
			// the one UIKit's equal index path maps to, which the handler can hold while this runs from Scrolled.
			var indexPath = Foundation.NSIndexPath.FromItemSection(index, 0);
			return handler.Controller.CollectionView.CellForItem(indexPath) is { } cell ? (double)cell.Frame.Height : double.NaN;
#else
			// The header is the adapter's first position.
			var recyclerView = (RecyclerView)handler.PlatformView;
			return recyclerView.FindViewHolderForAdapterPosition(hasHeader ? index + 1 : index)?.ItemView is { } view
				? view.Height / (double)recyclerView.Resources.DisplayMetrics.Density
				: double.NaN;
#endif
		}

		// Whether an item laid out in view is the given height, in device-independent units.
		static bool ShowsItemOfHeight(CollectionViewHandler handler, double height)
		{
#if IOS || MACCATALYST
			return handler.Controller.CollectionView.VisibleCells.Any(cell => Math.Abs((double)cell.Frame.Height - height) <= AnchorTolerance);
#else
			var recyclerView = (RecyclerView)handler.PlatformView;
			var layoutManager = recyclerView.GetLayoutManager();
			for (var index = 0; index < layoutManager.ChildCount; index++)
			{
				if (Math.Abs(layoutManager.GetChildAt(index).Height / (double)recyclerView.Resources.DisplayMetrics.Density - height) <= AnchorTolerance)
				{
					return true;
				}
			}

			return false;
#endif
		}

		// The laid out height of an item of a grouped list with a header and a group header template, in device-independent
		// units, or NaN while it isn't laid out.
		static double GetGroupedItemHeight(CollectionViewHandler handler, IList<AnchorGroup> groups, int groupIndex, int index)
		{
#if IOS || MACCATALYST
			// Each group is a section, and the headers are supplementary views. Not disposed (see GetItemHeight).
			var indexPath = Foundation.NSIndexPath.FromItemSection(index, groupIndex);
			return handler.Controller.CollectionView.CellForItem(indexPath) is { } cell ? (double)cell.Frame.Height : double.NaN;
#else
			// The header is the adapter's first position, and each group starts with its header.
			var position = 1 + groups.Take(groupIndex).Sum(group => 1 + group.Count) + 1 + index;
			var recyclerView = (RecyclerView)handler.PlatformView;
			return recyclerView.FindViewHolderForAdapterPosition(position)?.ItemView is { } view
				? view.Height / (double)recyclerView.Resources.DisplayMetrics.Density
				: double.NaN;
#endif
		}

		// Scrolls the platform view directly, as a scroll the app didn't ask CollectionView for does (in device-independent
		// units, toward the end when positive).
		static void ScrollPlatformViewBy(CollectionViewHandler handler, double distance)
		{
#if IOS || MACCATALYST
			var collectionView = handler.Controller.CollectionView;
			var offset = collectionView.ContentOffset;
			collectionView.SetContentOffset(new CoreGraphics.CGPoint(offset.X, offset.Y + distance), false);
#else
			var recyclerView = (RecyclerView)handler.PlatformView;
			recyclerView.ScrollBy(0, (int)Math.Round(distance * recyclerView.Resources.DisplayMetrics.Density));
#endif
		}

		class AnchorItem : INotifyPropertyChanged
		{
			double _height;

			public AnchorItem(string text, double height)
			{
				Text = text;
				_height = height;
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public string Text { get; }

			public double Height
			{
				get => _height;
				set
				{
					_height = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Height)));
				}
			}
		}

		// A sequence that gives its items only the first time it's read, as one that reads a stream does.
		sealed class ReadOnceSequence : System.Collections.IEnumerable
		{
			readonly IEnumerator<string> _items;

			public ReadOnceSequence(IEnumerable<string> items) => _items = items.GetEnumerator();

			public System.Collections.IEnumerator GetEnumerator()
			{
				while (_items.MoveNext())
				{
					yield return _items.Current;
				}
			}
		}

		class AnchorGroup : ObservableCollection<AnchorItem>
		{
			public AnchorGroup(string name, ObservableCollection<AnchorItem> items) : base(items)
			{
				Name = name;
			}

			public string Name { get; }
		}

#if ANDROID
		// Tells when a scroll of a RecyclerView ends. RecyclerView tells its listeners after the view itself, which tells the anchor.
		sealed class ScrollEndedListener : RecyclerView.OnScrollListener
		{
			readonly Action _scrollEnded;
			bool _scrolling;

			public ScrollEndedListener(Action scrollEnded) => _scrollEnded = scrollEnded;

			public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
			{
				if (newState != RecyclerView.ScrollStateIdle)
				{
					_scrolling = true;
				}
				else if (_scrolling)
				{
					_scrolling = false;
					_scrollEnded();
				}
			}
		}
#endif
	}
}
#endif
