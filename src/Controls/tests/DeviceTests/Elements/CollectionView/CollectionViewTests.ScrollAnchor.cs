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

				await AssertAtEnd(handler, "New items should keep the end in view.");

				items[^1].Height = 160;

				// Until the platform lays the item out at its new size, the view still shows the end it had.
				await AssertEventually(() => Math.Abs(GetItemHeight(handler, items.Count - 1) - 160) <= AnchorTolerance, timeout: 3000, message: "The last item should grow.");
				await AssertAtEnd(handler, "The last item growing should keep the end in view.");

				items.RemoveAt(items.Count - 1);
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
			collectionView.GroupHeaderTemplate = new DataTemplate(() =>
			{
				var label = new Label { HeightRequest = 24 };
				label.SetBinding(Label.TextProperty, nameof(AnchorGroup.Name));
				return label;
			});

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertAtEnd(handler, "The grouped list should open at its end.");

				groups[^1].Add(new AnchorItem("Added to the last group", 40));
				await AssertAtEnd(handler, "An item added to the last group should keep the end in view.");

				groups.Add(new AnchorGroup("New group", CreateAnchorItems(3)));
				await AssertAtEnd(handler, "A new group should keep the end in view.");
				Assert.True(collectionView.IsAnchored);
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
				await AssertAtEnd(handler, "New items should keep the end in view again.");
				Assert.True(collectionView.IsAnchored, "The list should still follow its end.");
			});
		}

		[Fact(DisplayName = "ScrollToEnd without AnchorMode scrolls past the footer once")]
		public async Task ScrollToEndWithoutAnchorModeScrollsToTheEndOnce()
		{
			SetupBuilder();
			var items = CreateAnchorItems(40);
			var collectionView = CreateAnchorCollectionView(items, ScrollAnchorMode.None);

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await AssertEventually(() => GetDistanceFromStart(handler) <= AnchorTolerance, timeout: 3000, message: "The list should open at its start.");

				collectionView.ScrollToEnd(animate: false);
				await AssertAtEnd(handler, "ScrollToEnd should reach the end of the content, footer included.");
				Assert.False(collectionView.IsAnchored);

				items.Add(new AnchorItem("Added", 40));
				await Task.Delay(500);
				Assert.True(GetDistanceToEnd(handler) > AnchorTolerance, "Without AnchorMode, the list shouldn't follow its end.");
			});
		}

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

		// The laid out height of the item at an index of an ungrouped list with a header, in device-independent units, or NaN
		// while it isn't laid out.
		static double GetItemHeight(CollectionViewHandler handler, int index)
		{
#if IOS || MACCATALYST
			// The header is a supplementary view: the items are the first section's.
			using var indexPath = Foundation.NSIndexPath.FromItemSection(index, 0);
			return handler.Controller.CollectionView.CellForItem(indexPath) is { } cell ? (double)cell.Frame.Height : double.NaN;
#else
			// The header is the adapter's first position.
			var recyclerView = (RecyclerView)handler.PlatformView;
			return recyclerView.FindViewHolderForAdapterPosition(index + 1)?.ItemView is { } view
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
