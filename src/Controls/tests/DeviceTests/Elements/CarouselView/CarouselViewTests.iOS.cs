using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CarouselViewTests
	{
		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 0)]
		[InlineData(false, 3)]
		[InlineData(true, 3)]
		public async Task RotationPreservesSelectionAndScrollRequestCustomization(bool loop, int position)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ScrollRequestCarouselView, ScrollRequestCarouselViewHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new ScrollRequestCarouselView
			{
				Loop = loop,
				Position = position,
				PeekAreaInsets = Thickness.Zero,
				WidthRequest = 200,
				HeightRequest = 300,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = false,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<ScrollRequestCarouselViewHandler>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				int nativeItem = loop ? position + 1 : position;

				bool IsSelectedItemVisible()
				{
					var templatedCell = collectionView.VisibleCells.OfType<TemplatedCell2>().FirstOrDefault(cell =>
						collectionView.IndexPathForCell(cell)?.Item == nativeItem);
					if (templatedCell is null ||
						templatedCell.PlatformHandler?.VirtualView is not View itemView ||
						!ReferenceEquals(itemView.BindingContext, items[position]) ||
						templatedCell.Hidden || templatedCell.Bounds.Width <= 0 || templatedCell.Bounds.Height <= 0)
					{
						return false;
					}

					var frame = templatedCell.ConvertRectToView(templatedCell.Bounds, collectionView);
					var bounds = collectionView.Bounds;
					return frame.X + frame.Width / 2 >= bounds.X &&
						frame.X + frame.Width / 2 <= bounds.X + bounds.Width &&
						frame.Y + frame.Height / 2 >= bounds.Y &&
						frame.Y + frame.Height / 2 <= bounds.Y + bounds.Height;
				}

				await AssertEventually(() => controller.InitialPositionSet &&
					carousel.Position == position &&
					ReferenceEquals(carousel.CurrentItem, items[position]) &&
					indicator.Position == position &&
					IsSelectedItemVisible());

				int positionChanges = 0;
				int currentItemChanges = 0;
				var requests = new List<ScrollToRequestEventArgs>();
				carousel.PositionChanged += (_, _) => positionChanges++;
				carousel.CurrentItemChanged += (_, _) => currentItemChanges++;
				carousel.ScrollToRequested += (_, args) => requests.Add(args);

				async Task RotateTo(CGSize size)
				{
					requests.Clear();
					carousel.ScrollRequests.Clear();
					handler.ScrollRequests.Clear();
					Foundation.NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIDevice.OrientationDidChangeNotification,
						UIDevice.CurrentDevice);
					carousel.WidthRequest = (double)size.Width;
					carousel.HeightRequest = (double)size.Height;
					var bounds = collectionView.Bounds;
					collectionView.Bounds = new CGRect(bounds.X, bounds.Y, size.Width, size.Height);
					controller.ViewWillLayoutSubviews();

					Assert.True(controller.IsRotatingOrResizing());
					Assert.Equal(position, controller.PositionBeforeBoundsChange);
					collectionView.SetNeedsLayout();
					collectionView.LayoutIfNeeded();
					controller.ViewDidLayoutSubviews();

					await AssertEventually(() => collectionView.Bounds.Size == size &&
						!controller.IsRotatingOrResizing() &&
						carousel.Position == position &&
						ReferenceEquals(carousel.CurrentItem, items[position]) &&
						indicator.Position == position &&
						IsSelectedItemVisible(),
						message: "The selection was not preserved across rotation.");

					var request = Assert.Single(requests);
					Assert.Equal(position, request.Index);
					Assert.Equal(ScrollToPosition.Center, request.ScrollToPosition);
					Assert.False(request.IsAnimated);
					Assert.Same(request, Assert.Single(carousel.ScrollRequests));
					Assert.Same(request, Assert.Single(handler.ScrollRequests));
					Assert.Equal(0, positionChanges);
					Assert.Equal(0, currentItemChanges);
				}

				await RotateTo(new CGSize(300, 200));
				await RotateTo(new CGSize(200, 300));
			});
		}

		public class ScrollRequestCarouselView : CarouselView
		{
			public List<ScrollToRequestEventArgs> ScrollRequests { get; } = new();

			protected override void OnScrollToRequested(ScrollToRequestEventArgs e)
			{
				ScrollRequests.Add(e);
				base.OnScrollToRequested(e);
			}
		}

		public class ScrollRequestCarouselViewHandler : CarouselViewHandler2
		{
			public List<ScrollToRequestEventArgs> ScrollRequests { get; } = new();

			protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
			{
				ScrollRequests.Add(args);
				base.ScrollToRequested(sender, args);
			}
		}

		[Theory]
		[InlineData(3, false)]
		[InlineData(4, false)]
		[InlineData(3, true)]
		[InlineData(4, true)]
		public async Task BoundsOnlyResizesPreserveIdleSelectionWithoutSelectionEvents(int position, bool loop)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = loop,
				Position = position,
				WidthRequest = 300,
				HeightRequest = 200,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = false,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				int nativeItem = loop ? position + 1 : position;

				TemplatedCell2 GetSelectedCell() => collectionView.VisibleCells.OfType<TemplatedCell2>().FirstOrDefault(cell =>
					collectionView.IndexPathForCell(cell)?.Item == nativeItem);

				bool IsSelectedItemVisible()
				{
					var cell = GetSelectedCell();
					if (cell is null || cell.Hidden || cell.Bounds.Width <= 0 || cell.Bounds.Height <= 0)
					{
						return false;
					}

					var frame = cell.ConvertRectToView(cell.Bounds, collectionView);
					var bounds = collectionView.Bounds;
					var centerX = frame.X + frame.Width / 2;
					var centerY = frame.Y + frame.Height / 2;
					return centerX >= bounds.X && centerX <= bounds.X + bounds.Width &&
						centerY >= bounds.Y && centerY <= bounds.Y + bounds.Height;
				}

				await AssertEventually(() => controller.InitialPositionSet &&
					ReferenceEquals(carousel.CurrentItem, items[position]) &&
					IsSelectedItemVisible());
				collectionView.LayoutIfNeeded();
				controller.ViewWillLayoutSubviews();
				controller.ViewDidLayoutSubviews();
				Assert.False(controller.IsRotatingOrResizing());

				int positionChanges = 0;
				int currentItemChanges = 0;
				carousel.PositionChanged += (_, _) => positionChanges++;
				carousel.CurrentItemChanged += (_, _) => currentItemChanges++;

				var sizes = new[] { new CGSize(240, 160), new CGSize(340, 240), new CGSize(300, 200) };
				foreach (var size in sizes)
				{
					Assert.False(controller.IsScrollInteractionActive());
					carousel.WidthRequest = (double)size.Width;
					carousel.HeightRequest = (double)size.Height;
					var bounds = collectionView.Bounds;
					collectionView.Bounds = new CGRect(bounds.X, bounds.Y, size.Width, size.Height);

					// Observe capture before layout finishes, so removing bounds detection cannot silently pass.
					controller.ViewWillLayoutSubviews();
					Assert.True(controller.IsRotatingOrResizing());
					Assert.Equal(position, controller.PositionBeforeBoundsChange);
					collectionView.SetNeedsLayout();
					collectionView.LayoutIfNeeded();
					controller.ViewDidLayoutSubviews();

					await AssertEventually(() => collectionView.Bounds.Size == size &&
						IsSelectedItemVisible() &&
						!controller.IsRotatingOrResizing(),
						message: "The selected item was not visible after the bounds-only resize.");

					Assert.Equal(position, carousel.Position);
					Assert.Same(items[position], carousel.CurrentItem);
					Assert.Equal(position, indicator.Position);
					Assert.Equal(0, positionChanges);
					Assert.Equal(0, currentItemChanges);

					var cell = Assert.IsAssignableFrom<TemplatedCell2>(GetSelectedCell());
					var label = Assert.IsType<Label>(cell.PlatformHandler.VirtualView);
					Assert.Same(items[position], label.BindingContext);
					var visibleFrame = cell.ConvertRectToView(cell.Bounds, collectionView);
					var visibleBounds = collectionView.Bounds;
					Assert.InRange((double)(visibleFrame.X + visibleFrame.Width / 2),
						(double)visibleBounds.X, (double)(visibleBounds.X + visibleBounds.Width));
					Assert.InRange((double)(visibleFrame.Y + visibleFrame.Height / 2),
						(double)visibleBounds.Y, (double)(visibleBounds.Y + visibleBounds.Height));
				}
			});
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public async Task SelectionChangeAfterResizeCaptureRestoresRequestedSelection(bool loop, bool useCurrentItem)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = loop,
				Position = 0,
				WidthRequest = 300,
				HeightRequest = 200,
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = true,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				await AssertEventually(() => controller.InitialPositionSet &&
					carousel.Position == 0 &&
					ReferenceEquals(carousel.CurrentItem, items[0]));

				var pendingScroll = typeof(CarouselViewController2).GetField("_gotoPosition", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(pendingScroll);

				controller.UpdateResizeState();
				var bounds = collectionView.Bounds;
				var resizedWidth = bounds.Width - 40;
				carousel.WidthRequest = (double)resizedWidth;
				collectionView.Bounds = new CGRect(bounds.X, bounds.Y, resizedWidth, bounds.Height);
				controller.UpdateResizeState();
				Assert.True(controller.IsRotatingOrResizing());
				Assert.Equal(0, controller.PositionBeforeBoundsChange);

				if (useCurrentItem)
				{
					carousel.CurrentItem = items[3];
				}
				else
				{
					carousel.Position = 3;
				}

				Assert.Same(items[useCurrentItem ? 3 : 0], carousel.CurrentItem);
				Assert.Equal(useCurrentItem ? 0 : 3, carousel.Position);
				Assert.True(controller.IsRotatingOrResizing());
				Assert.Equal(3, controller.PositionBeforeBoundsChange);
				Assert.Equal(-1, (int)pendingScroll.GetValue(controller));

				collectionView.SetNeedsLayout();
				collectionView.LayoutIfNeeded();
				controller.ViewDidLayoutSubviews();
				collectionView.CollectionViewLayout.InvalidateLayout();
				collectionView.LayoutIfNeeded();

				if (!useCurrentItem && OperatingSystem.IsIOSVersionAtLeast(26))
				{
					// Verify the selection also survives the deferred Position mapper.
					await Task.Delay(200);
					collectionView.LayoutIfNeeded();
				}

				int nativeItem = loop ? 4 : 3;
				await AssertEventually(() => controller.PositionBeforeBoundsChange == -1 &&
					(int)pendingScroll.GetValue(controller) == -1 &&
					carousel.Position == 3 &&
					ReferenceEquals(carousel.CurrentItem, items[3]) &&
					indicator.Position == 3 &&
					Array.Exists(collectionView.IndexPathsForVisibleItems, path => (int)path.Item == nativeItem),
					message: "The selection requested after resize capture was not restored with matching Position.");
			});
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public async Task ConfirmedBoundsRestorationCompletesPendingAnimatedSelectionChange(bool loop, bool useCurrentItem)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = loop,
				WidthRequest = 300,
				HeightRequest = 200,
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = true,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				await AssertEventually(() => controller.InitialPositionSet && carousel.CurrentItem is "First");

				var pendingScroll = typeof(CarouselViewController2).GetField("_gotoPosition", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(pendingScroll);

				if (useCurrentItem)
				{
					carousel.CurrentItem = items[3];
				}
				else
				{
					carousel.Position = 3;
				}
				// Observe the actual in-flight scroll, including the deferred Position start on iOS 26.
				await AssertEventually(() => (int)pendingScroll.GetValue(controller) == 3 &&
					carousel.Position == (useCurrentItem ? 0 : 3) &&
					ReferenceEquals(carousel.CurrentItem, items[useCurrentItem ? 3 : 0]),
					interval: 1, message: "The animated selection change did not enter the pending-scroll state.");

				controller.UpdateResizeState();
				var bounds = collectionView.Bounds;
				var resizedWidth = bounds.Width - 40;
				carousel.WidthRequest = (double)resizedWidth;
				collectionView.Bounds = new CGRect(bounds.X, bounds.Y, resizedWidth, bounds.Height);
				controller.UpdateResizeState();
				Assert.Equal(3, controller.PositionBeforeBoundsChange);
				Assert.Equal(useCurrentItem ? 0 : 3, carousel.Position);
				Assert.Same(items[useCurrentItem ? 3 : 0], carousel.CurrentItem);
				Assert.Equal(3, (int)pendingScroll.GetValue(controller));

				Assert.True(controller.ShouldIgnorePositionUpdateAfterBoundsChange(2, true));
				Assert.True(controller.ShouldIgnorePositionUpdateAfterBoundsChange(3, false));
				Assert.Equal(3, controller.PositionBeforeBoundsChange);

				// Let UIKit report the restored cell after the resize gate is lifted.
				collectionView.LayoutIfNeeded();
				controller.ViewDidLayoutSubviews();
				collectionView.CollectionViewLayout.InvalidateLayout();
				collectionView.LayoutIfNeeded();
				using var restoredIndexPath = controller.GetScrollToIndexPath(3);
				await AssertEventually(() => controller.PositionBeforeBoundsChange == -1 &&
					(int)pendingScroll.GetValue(controller) == -1 &&
					carousel.Position == 3 && ReferenceEquals(carousel.CurrentItem, items[3]) &&
					indicator.Position == 3 &&
					Array.Exists(collectionView.IndexPathsForVisibleItems, path => path.Equals(restoredIndexPath)),
					message: "The layout callback did not complete the interrupted animated selection.");

				carousel.IsScrollAnimated = false;
				carousel.Position = 1;
				using var nextIndexPath = controller.GetScrollToIndexPath(1);
				await AssertEventually(() => carousel.Position == 1 && carousel.CurrentItem is "Second" &&
					collectionView.CellForItem(nextIndexPath) is not null,
					message: "A subsequent Position request was blocked after restoration.");
				Assert.Equal(1, indicator.Position);

				carousel.CurrentItem = items[4];
				using var finalIndexPath = controller.GetScrollToIndexPath(4);
				await AssertEventually(() => carousel.Position == 4 && carousel.CurrentItem is "Fifth" &&
					collectionView.CellForItem(finalIndexPath) is not null,
					message: "A subsequent CurrentItem request was blocked after restoration.");
				Assert.Equal(4, indicator.Position);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task PropertySelectionAfterClampedBoundsRestorationScrollsToRequestedItem(bool useCurrentItem)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = false,
				Position = 4,
				WidthRequest = 200,
				HeightRequest = 240,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { SnapPointsType = SnapPointsType.None },
				PeekAreaInsets = new Thickness(0, 20, 0, 20),
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = false,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				await AssertEventually(() => controller.InitialPositionSet &&
					carousel.Position == 4 &&
					ReferenceEquals(carousel.CurrentItem, items[4]));

				controller.UpdateResizeState();
				carousel.HeightRequest = 300;
				var bounds = collectionView.Bounds;
				collectionView.Bounds = new CGRect(bounds.X, bounds.Y, bounds.Width, 300);
				controller.ViewWillLayoutSubviews();
				Assert.True(controller.IsRotatingOrResizing());
				Assert.Equal(4, controller.PositionBeforeBoundsChange);

				collectionView.SetNeedsLayout();
				collectionView.LayoutIfNeeded();
				controller.ViewDidLayoutSubviews();
				collectionView.CollectionViewLayout.InvalidateLayout();
				collectionView.LayoutIfNeeded();

				await AssertEventually(() => collectionView.Bounds.Height == 300 &&
					!controller.IsRotatingOrResizing() &&
					controller.PositionBeforeBoundsChange == 4 &&
					carousel.Position == 4 &&
					ReferenceEquals(carousel.CurrentItem, items[4]) &&
					indicator.Position == 4 &&
					Math.Abs((double)(collectionView.ContentOffset.Y -
						(collectionView.ContentSize.Height - collectionView.Bounds.Height))) <= 1,
					message: "The last item did not finish restoration at the clamped edge with its saved target remaining.");

				if (useCurrentItem)
				{
					carousel.CurrentItem = items[2];
				}
				else
				{
					carousel.Position = 2;
				}

				bool IsRequestedItemCentered()
				{
					var cell = collectionView.VisibleCells.OfType<TemplatedCell2>().FirstOrDefault(c =>
						c.PlatformHandler?.VirtualView is View view && ReferenceEquals(view.BindingContext, items[2]));
					if (cell is null || cell.Hidden)
					{
						return false;
					}

					var frame = cell.ConvertRectToView(cell.Bounds, collectionView);
					var visibleBounds = collectionView.Bounds;
					return Math.Abs((double)(frame.Y + frame.Height / 2 -
						(visibleBounds.Y + visibleBounds.Height / 2))) <= 1;
				}

				await AssertEventually(() => controller.PositionBeforeBoundsChange == -1 &&
					carousel.Position == 2 &&
					ReferenceEquals(carousel.CurrentItem, items[2]) &&
					indicator.Position == 2 &&
					IsRequestedItemCentered(),
					message: "A property-driven selection was deferred after clamped bounds restoration had finished.");
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ExplicitScrollToSupersedesPendingBoundsRestoration(bool loop)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = loop,
				Position = 4,
				WidthRequest = 200,
				HeightRequest = 240,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { SnapPointsType = SnapPointsType.None },
				PeekAreaInsets = new Thickness(0, 20, 0, 20),
				ItemsSource = items,
				IndicatorView = indicator,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				await AssertEventually(() => controller.InitialPositionSet && ReferenceEquals(carousel.CurrentItem, items[4]));

				controller.UpdateResizeState();
				var bounds = collectionView.Bounds;
				carousel.HeightRequest = 300;
				collectionView.Bounds = new CGRect(bounds.X, bounds.Y, bounds.Width, 300);
				controller.UpdateResizeState();
				Assert.Equal(4, controller.PositionBeforeBoundsChange);

				carousel.ScrollTo(2, position: ScrollToPosition.Center, animate: false);
				Assert.Equal(-1, controller.PositionBeforeBoundsChange);
				collectionView.LayoutIfNeeded();

				using var targetIndexPath = controller.GetScrollToIndexPath(2);
				await AssertEventually(() => carousel.Position == 2 &&
					ReferenceEquals(carousel.CurrentItem, items[2]) &&
					indicator.Position == 2 &&
					Array.Exists(collectionView.IndexPathsForVisibleItems, path => path.Equals(targetIndexPath)),
					message: "The explicit ScrollTo target did not replace the saved resize position.");
			});
		}

#if MACCATALYST
		[Fact]
		public async Task VerticalBoundsRestorationRearmsNativeScrollRestoreForSelectedItem()
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = false,
				Position = 0,
				WidthRequest = 300,
				HeightRequest = 300,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
				{
					SnapPointsType = SnapPointsType.MandatorySingle,
					SnapPointsAlignment = SnapPointsAlignment.Center
				},
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = false,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = Assert.IsAssignableFrom<Microsoft.Maui.Controls.Handlers.Items.MauiCollectionView>(controller.CollectionView);
				var trackingField = typeof(Microsoft.Maui.Controls.Handlers.Items.MauiCollectionView).GetField("_isTrackingScrollRestore", BindingFlags.Instance | BindingFlags.NonPublic);
				var pendingItemField = typeof(Microsoft.Maui.Controls.Handlers.Items.MauiCollectionView).GetField("_pendingScrollRestoreItem", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(trackingField);
				Assert.NotNull(pendingItemField);

				// Store plain item numbers: small NSIndexPath values can share a managed wrapper that
				// MauiCollectionView disposes when it runs a queued restore.
				var firstItem = (int)controller.GetScrollToIndexPath(0).Item;
				var selectedItem = (int)controller.GetScrollToIndexPath(2).Item;
				await AssertEventually(() => controller.InitialPositionSet && carousel.CurrentItem is "First" &&
					(bool)trackingField.GetValue(collectionView) &&
					(int)pendingItemField.GetValue(collectionView) == firstItem,
					message: "The initial position did not arm the native scroll restore for the first item.");

				// The carousel delegator does not clear the native restore on drag, so the item-0 target stays armed.
				var delegator = Assert.IsAssignableFrom<CarouselViewDelegator2>(collectionView.Delegate);
				delegator.DraggingStarted(collectionView);
				collectionView.ScrollToItem(Foundation.NSIndexPath.FromItemSection(selectedItem, 0), UICollectionViewScrollPosition.CenteredVertically, false);
				delegator.DraggingEnded(collectionView, false);
				collectionView.LayoutIfNeeded();

				await AssertEventually(() => carousel.Position == 2 && carousel.CurrentItem is "Third" && indicator.Position == 2,
					message: "The simulated swipe did not select the third item.");
				Assert.True((bool)trackingField.GetValue(collectionView));
				Assert.Equal(firstItem, (int)pendingItemField.GetValue(collectionView));

				int positionChanges = 0;
				carousel.PositionChanged += (_, _) => positionChanges++;

				controller.UpdateResizeState();
				carousel.HeightRequest = 260;
				var bounds = collectionView.Bounds;
				collectionView.Bounds = new CGRect(bounds.X, bounds.Y, bounds.Width, 260);
				controller.UpdateResizeState();
				Assert.True(controller.IsRotatingOrResizing());
				Assert.Equal(2, controller.PositionBeforeBoundsChange);

				collectionView.SetNeedsLayout();
				collectionView.LayoutIfNeeded();
				controller.ViewDidLayoutSubviews();

				bool IsSelectedItemVisible()
				{
					// Locate the cell by its bound item: the runtime can hand back an already-disposed
					// wrapper for this NSIndexPath after MauiCollectionView's queued restore disposes it.
					var cell = collectionView.VisibleCells.OfType<TemplatedCell2>().FirstOrDefault(c =>
						c.PlatformHandler?.VirtualView is View view && Equals(view.BindingContext, "Third"));
					if (cell is null || cell.Hidden)
					{
						return false;
					}

					var frame = cell.ConvertRectToView(cell.Bounds, collectionView);
					var visibleBounds = collectionView.Bounds;
					var centerY = frame.Y + frame.Height / 2;
					return Math.Abs((double)(centerY - (visibleBounds.Y + visibleBounds.Height / 2))) <= 1;
				}

				bool IsSettled() => collectionView.Bounds.Height == 260 &&
					!controller.IsRotatingOrResizing() &&
					carousel.Position == 2 &&
					carousel.CurrentItem is "Third" &&
					indicator.Position == 2 &&
					IsSelectedItemVisible();

				await AssertEventually(IsSettled, message: "The selected item was not restored after shrinking the vertical carousel.");

				// Let queued native restore callbacks run; a stale item-0 target would roll the carousel back here.
				await Task.Delay(500);
				collectionView.LayoutIfNeeded();
				await AssertEventually(IsSettled, message: "A queued native restore rolled the vertical carousel back to an older target.");

				Assert.Equal(selectedItem, (int)pendingItemField.GetValue(collectionView));
				Assert.Equal(0, positionChanges);
			});
		}
#endif

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task AttachedItemsSourceReplacementClearsPendingBoundsRestoration(bool loop)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = loop,
				Position = 4,
				WidthRequest = 300,
				HeightRequest = 200,
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = false,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				await AssertEventually(() => controller.InitialPositionSet && ReferenceEquals(carousel.CurrentItem, items[4]));

				controller.UpdateResizeState();
				var bounds = collectionView.Bounds;
				var resizedWidth = bounds.Width - 40;
				carousel.WidthRequest = (double)resizedWidth;
				collectionView.Bounds = new CGRect(bounds.X, bounds.Y, resizedWidth, bounds.Height);
				controller.UpdateResizeState();
				Assert.True(controller.IsRotatingOrResizing());
				Assert.Equal(4, controller.PositionBeforeBoundsChange);

				// Replace the source while still attached, before layout consumes the saved target.
				var replacement = new ObservableCollection<string> { "New First", "New Second", "New Third" };
				carousel.ItemsSource = replacement;

				Assert.Equal(-1, controller.PositionBeforeBoundsChange);
				Assert.False(controller.IsRotatingOrResizing());
				Assert.False(controller.ShouldIgnorePositionUpdateAfterBoundsChange(0, true));

				collectionView.LayoutIfNeeded();
				controller.ViewDidLayoutSubviews();

				var pendingScroll = typeof(CarouselViewController2).GetField("_gotoPosition", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(pendingScroll);

				// The distinct items ensure the leading loop placeholder cannot satisfy the first-item assertion.
				// Match by binding context instead of holding NSIndexPath wrappers, which a queued restore can dispose.
				bool IsItemCentered(string item) => collectionView.VisibleCells.OfType<TemplatedCell2>().Any(cell =>
				{
					if (cell.Hidden || cell.PlatformHandler?.VirtualView is not View view || !Equals(view.BindingContext, item))
					{
						return false;
					}

					var frame = cell.ConvertRectToView(cell.Bounds, collectionView);
					var visibleBounds = collectionView.Bounds;
					var centerX = frame.X + frame.Width / 2;
					return Math.Abs((double)(centerX - (visibleBounds.X + visibleBounds.Width / 2))) <= 1;
				});

				await AssertEventually(() => carousel.Position == 0 &&
					carousel.CurrentItem is "New First" &&
					indicator.Position == 0 &&
					(int)pendingScroll.GetValue(controller) == -1 &&
					IsItemCentered("New First"),
					message: "The replacement source's first item was not synchronized after clearing the saved resize target.");
				Assert.Equal(-1, controller.PositionBeforeBoundsChange);

				carousel.Position = 2;
				await AssertEventually(() => carousel.Position == 2 &&
					carousel.CurrentItem is "New Third" &&
					indicator.Position == 2 &&
					IsItemCentered("New Third"),
					message: "A subsequent selection was blocked after replacing the source during resize restoration.");
			});
		}

		[Theory]
		[InlineData(false, "Tracking")]
		[InlineData(false, "Dragging")]
		[InlineData(false, "Decelerating")]
		[InlineData(true, "Tracking")]
		[InlineData(true, "Dragging")]
		[InlineData(true, "Decelerating")]
		public async Task ResizeDuringOrthogonalScrollCancelsPositionRestoration(bool loop, string interactionState)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third" };
			var carousel = new CarouselView
			{
				Loop = loop,
				Position = 1,
				WidthRequest = 300,
				HeightRequest = 200,
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				await AssertEventually(() => controller.InitialPositionSet && carousel.CurrentItem is "Second");

				using var container = new UIView();
				using var scroller = new InteractingCarouselScrollView();
				container.AddSubview(scroller);
				collectionView.AddSubview(container);

				try
				{
					void BeginResize()
					{
						scroller.InteractionState = null;
						controller.UpdateResizeState();
						var bounds = collectionView.Bounds;
						collectionView.Bounds = new CGRect(bounds.X, bounds.Y, bounds.Width - 10, bounds.Height);
						controller.UpdateResizeState();
						Assert.Equal(1, controller.PositionBeforeBoundsChange);
						Assert.True(controller.IsRotatingOrResizing());
						scroller.InteractionState = interactionState;
						Assert.False(collectionView.Tracking || collectionView.Dragging || collectionView.Decelerating);
					}

					BeginResize();
					Assert.False(controller.IsRotatingOrResizing());
					Assert.Equal(-1, controller.PositionBeforeBoundsChange);

					BeginResize();
					Assert.False(controller.ShouldIgnorePositionUpdateAfterBoundsChange(2, false));
					Assert.Equal(-1, controller.PositionBeforeBoundsChange);

					BeginResize();
					controller.ViewDidLayoutSubviews();
					Assert.Equal(-1, controller.PositionBeforeBoundsChange);

					// Cancellation must survive finger-up/deceleration ending, not just bypass one callback.
					scroller.InteractionState = null;
					Assert.False(controller.ShouldIgnorePositionUpdateAfterBoundsChange(2, false));
					controller.SetPosition(2);
					Assert.Equal(2, carousel.Position);
					Assert.Equal("Third", carousel.CurrentItem);
				}
				finally
				{
					container.RemoveFromSuperview();
				}
			});
		}

		[Fact]
		public async Task ScrollingItemContentDoesNotCancelPositionRestoration()
		{
			SetupBuilder();

			var carousel = new CarouselView
			{
				Loop = false,
				WidthRequest = 300,
				HeightRequest = 200,
				ItemsSource = new[] { "First", "Second" },
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				using var indexPath = Foundation.NSIndexPath.FromItemSection(0, 0);
				await AssertEventually(() => controller.InitialPositionSet && collectionView.CellForItem(indexPath) is not null);
				var cell = collectionView.CellForItem(indexPath);
				using var scroller = new InteractingCarouselScrollView { InteractionState = "Dragging" };
				cell.ContentView.AddSubview(scroller);

				try
				{
					controller.UpdateResizeState();
					var bounds = collectionView.Bounds;
					collectionView.Bounds = new CGRect(bounds.X, bounds.Y, bounds.Width - 10, bounds.Height);
					controller.UpdateResizeState();

					Assert.True(controller.IsRotatingOrResizing());
					Assert.True(controller.ShouldIgnorePositionUpdateAfterBoundsChange(1, false));
					Assert.Equal(0, controller.PositionBeforeBoundsChange);
				}
				finally
				{
					scroller.RemoveFromSuperview();
				}
			});
		}

		sealed class InteractingCarouselScrollView : UIScrollView
		{
			public string InteractionState { get; set; }
			public override bool Tracking => InteractionState == "Tracking";
			public override bool Dragging => InteractionState == "Dragging";
			public override bool Decelerating => InteractionState == "Decelerating";
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task TemplateSelectorPreservesCellWhenMeasureIsInvalidated(bool loop, bool createsNewTemplates)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CarouselView, CarouselViewHandler2>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var carouselView = new CarouselView
			{
				Loop = loop,
				HeightRequest = 200,
				WidthRequest = 300,
				ItemsSource = new[] { 1, 2 },
				ItemTemplate = createsNewTemplates
					? new NewCarouselTemplateSelector()
					: new CustomDataTemplateSelectorSelector
					{
						Template1 = new DataTemplate(() => new Label { Text = "Item 1" }),
						Template2 = new DataTemplate(() => new Label { Text = "Item 2" })
					}
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carouselView, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				using var indexPath = Foundation.NSIndexPath.FromItemSection(loop ? 1 : 0, 0);

				await AssertEventually(() => controller.InitialPositionSet &&
					collectionView.CellForItem(indexPath) is TemplatedCell2,
					message: "The first carousel item was not displayed.");

				var cell = Assert.IsAssignableFrom<TemplatedCell2>(collectionView.CellForItem(indexPath));
				var label = Assert.IsType<Label>(cell.PlatformHandler.VirtualView);
				Assert.Equal("Item 1", label.Text);
				var reuseIdentifier = cell.ReuseIdentifier;

				label.Text = "Item 1 with updated content";
				((IPlatformMeasureInvalidationController)cell).InvalidateMeasure();
				((IPlatformMeasureInvalidationController)collectionView).InvalidateMeasure(isPropagating: true);
				Assert.True(cell.MeasureInvalidated);

				controller.ViewWillLayoutSubviews();
				collectionView.LayoutIfNeeded();

				Assert.Same(cell, collectionView.CellForItem(indexPath));
				Assert.Equal(reuseIdentifier, cell.ReuseIdentifier);
				Assert.Equal("Item 1 with updated content", label.Text);
				Assert.Equal(1, label.BindingContext);

				carouselView.Position = 1;
				using var nextIndexPath = Foundation.NSIndexPath.FromItemSection(loop ? 2 : 1, 0);
				await AssertEventually(() =>
					collectionView.CellForItem(nextIndexPath) is TemplatedCell2 nextCell &&
					nextCell.PlatformHandler?.VirtualView is Label { Text: "Item 2" } &&
					carouselView.CurrentItem is 2,
					message: "The second carousel template was not displayed after scrolling.");
			});
		}

		sealed class NewCarouselTemplateSelector : DataTemplateSelector
		{
			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
				=> new DataTemplate(() => new Label { Text = $"Item {item}" });
		}

		void SetupBuilderForDetachedItemsSourceReplacement()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<CarouselView, CarouselViewHandler2>();
					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		// Reproduces https://github.com/dotnet/maui/issues/36602:
		// While a CarouselView using CarouselViewHandler2 is detached (navigated away from),
		// replacing its ItemsSource and setting a new CurrentItem must NOT be overridden by the
		// stale Position value once the CarouselView is reattached.
		[Fact(DisplayName = "CarouselView Honors CurrentItem Over Stale Position After Detached ItemsSource Replacement")]
		public async Task CarouselViewHonorsCurrentItemOverStalePositionAfterDetachedItemsSourceReplacement()
		{
			SetupBuilderForDetachedItemsSourceReplacement();

			var catalogA = new List<string> { "A0", "A1", "A2", "A3", "A4" };
			var catalogB = new List<string> { "B0", "B1", "B2", "B3", "B4" };

			var indicator = new IndicatorView();
			var carouselView = new CarouselView
			{
				ItemsSource = catalogA,
				ItemTemplate = new DataTemplate(() => new Label()),
				IndicatorView = indicator
			};

			var navPage = new NavigationPage(new ContentPage { Content = carouselView });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async _ =>
			{
				// Establish the initial, valid selection: Position 3 / CurrentItem "A3".
				carouselView.Position = 3;
				carouselView.CurrentItem = "A3";

				await AssertEventually(
					() => carouselView.Position == 3 && (string)carouselView.CurrentItem == "A3",
					message: "Initial CarouselView state failed to synchronize to Position 3 / CurrentItem A3");

				// Navigate away — this detaches the CarouselView's native view from the window.
				await navPage.PushAsync(new ContentPage { Content = new Label { Text = "Away" } });

				// While detached, replace ItemsSource with catalog B and select "B1" via CurrentItem.
				// Position is intentionally left untouched (still 3), mirroring the real-world scenario.
				carouselView.ItemsSource = catalogB;
				carouselView.CurrentItem = "B1";

				// Navigate back — this reattaches the CarouselView's native view and triggers position sync.
				await navPage.PopAsync();

				await AssertEventually(
					() => carouselView.Position == 1 && (string)carouselView.CurrentItem == "B1",
					timeout: 3000,
					message: "CarouselView did not resolve CurrentItem 'B1' at Position 1 after reattaching; " +
						$"actual Position={carouselView.Position}, CurrentItem={carouselView.CurrentItem}");

				// Expected (fixed behavior): CurrentItem "B1" wins, Position resolves to 1, and the
				// linked IndicatorView stays in sync. Regressed behavior restores stale Position 3,
				// which would make CurrentItem resolve to "B3" instead.
				Assert.Equal("B1", carouselView.CurrentItem);
				Assert.Equal(1, carouselView.Position);
				Assert.Equal(1, indicator.Position);
			});
		}

		[Fact(DisplayName = "CarouselView Does Not Leak With Default ItemsLayout")]
		public async Task CarouselViewDoesNotLeakWithDefaultItemsLayout()
		{
			SetupBuilder();

			WeakReference weakCarouselView = null;
			WeakReference weakHandler = null;

			await InvokeOnMainThreadAsync(async () =>
			{
				var carouselView = new CarouselView
				{
					ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
					ItemTemplate = new DataTemplate(() => new Label())
					// Note: Not setting ItemsLayout - using the default
				};

				weakCarouselView = new WeakReference(carouselView);

				var handler = await CreateHandlerAsync<CarouselViewHandler2>(carouselView);

				// Verify handler is created
				Assert.NotNull(handler);

				// Store weak reference to the handler
				weakHandler = new WeakReference(handler);

				// Disconnect the handler
				((IElementHandler)handler).DisconnectHandler();
			});

			// Allow UIKit to release controller-owned references before forcing managed GC.
			await Task.Delay(100);

			// Force garbage collection
			await AssertionExtensions.WaitForGC(weakCarouselView, weakHandler);

			// Verify the CarouselView was collected
			Assert.False(weakCarouselView.IsAlive, "CarouselView should have been garbage collected");

			// Verify the handler was collected
			Assert.False(weakHandler.IsAlive, "CarouselViewHandler2 should have been garbage collected");
		}

		[Fact(DisplayName = "IsEnabled Updates Native Interaction State On Both Container And CollectionView")]
		public async Task IsEnabledUpdatesNativeInteractionStateOnBothContainerAndCollectionView()
		{
			SetupBuilder();

			var carouselView = new CarouselView
			{
				IsEnabled = false,
				ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
				ItemTemplate = new DataTemplate(() => new Label())
			};

			var handler = await CreateHandlerAsync<CarouselViewHandler2>(carouselView);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.False(handler.Controller.CollectionView.UserInteractionEnabled);
				Assert.False(handler.PlatformView.UserInteractionEnabled);

				carouselView.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				// Both the inner CollectionView and the outer Controller.View (handler.PlatformView)
				// must become interactive - if only the inner one updates, swiping stays blocked
				// because Controller.View is the outer view that gates touch delivery.
				Assert.True(handler.Controller.CollectionView.UserInteractionEnabled);
				Assert.True(handler.PlatformView.UserInteractionEnabled);
			});
		}
	}
}