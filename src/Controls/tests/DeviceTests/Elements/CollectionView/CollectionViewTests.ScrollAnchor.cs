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

		[Theory(DisplayName = "A ScrollToStart or ScrollToEnd right after an animated ScrollTo wins")]
		[InlineData(ScrollAnchorMode.None, ScrollAnchorMode.End)]
		[InlineData(ScrollAnchorMode.End, ScrollAnchorMode.End)]
		[InlineData(ScrollAnchorMode.None, ScrollAnchorMode.Start)]
		[InlineData(ScrollAnchorMode.Start, ScrollAnchorMode.Start)]
		public async Task AScrollToTheEdgeRightAfterAnAnimatedScrollToWins(ScrollAnchorMode anchorMode, ScrollAnchorMode edge)
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
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
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.Start);
			if (!hasHeader)
			{
				collectionView.Header = null;
			}

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				// A feed that keeps as many items as it had: the content stays the size it was, while the platform keeps the
				// row it showed first where it is, which can leave the new first item above the view.
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

#if ANDROID
		// RecyclerView applies the new item as the scroll starts, outside a layout: the look after the scroll still counts the
		// move as the scroll's. On iOS and Mac Catalyst, UICollectionView resizes its content as the item is inserted, and an
		// offset change outside a drag that comes with a size change is the platform laying out (as when the keyboard resizes
		// the viewport): the anchor keeps following the end there.
		[Fact(DisplayName = "Scrolling away as an item is added stops following the end")]
		public async Task ScrollingAwayAsAnItemIsAddedStopsFollowingTheEnd()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.End);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The list should open at its end.");

				// The scroll applies the new item before it moves the view: the move is still a scroll away from the end, not the
				// platform laying out the new item.
				items.Add(new AnchorItem("Added", 40));
				ScrollPlatformViewBy(handler, -200);

				await AssertEventually(() => !collectionView.IsAnchored, timeout: 3000, message: "Scrolling away from the end should stop following it.");
				await Task.Delay(500);
				Assert.True(GetDistanceToEnd(handler) > 100, $"The list shouldn't be pulled back to its end. Position: {GetScrollPosition(handler)}.");
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
			// The header is a supplementary view: the items are the first section's.
			using var indexPath = Foundation.NSIndexPath.FromItemSection(index, 0);
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
			// Each group is a section, and the headers are supplementary views.
			using var indexPath = Foundation.NSIndexPath.FromItemSection(index, groupIndex);
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

		class AnchorGroup : ObservableCollection<AnchorItem>
		{
			public AnchorGroup(string name, ObservableCollection<AnchorItem> items) : base(items)
			{
				Name = name;
			}

			public string Name { get; }
		}
	}
}
#endif
