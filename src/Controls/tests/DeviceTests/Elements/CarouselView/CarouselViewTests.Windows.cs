using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WPoint = Windows.Foundation.Point;
using WScrollViewer = Microsoft.UI.Xaml.Controls.ScrollViewer;
using WSnapPointsAlignment = Microsoft.UI.Xaml.Controls.Primitives.SnapPointsAlignment;
using WSnapPointsType = Microsoft.UI.Xaml.Controls.SnapPointsType;
using WUIElement = Microsoft.UI.Xaml.UIElement;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CarouselViewTests
	{
		[Theory]
		[InlineData(SnapPointsType.Mandatory)]
		[InlineData(SnapPointsType.MandatorySingle)]
		public async Task NativeScrollUpdatesSelectionWithoutProgrammaticFeedbackAndSettlesCentered(SnapPointsType snapPointsType)
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, snapPointsType, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				// WinUI can satisfy ChangeView from its own snap points before the handler's
				// terminal correction runs. Disable only the native snap coercion here so this
				// test specifically exercises CarouselViewHandler's ViewChanged settle path.
				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;

				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				Assert.Equal(0, carouselView.Position);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;

				int currentItemChanges = 0;
				int positionChanges = 0;
				int scrollToRequests = 0;
				carouselView.CurrentItemChanged += (_, _) => currentItemChanges++;
				carouselView.PositionChanged += (_, _) => positionChanges++;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				targetOffset += itemWidth * 0.5 + 1;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				Assert.True(
					carouselView.Position == 1,
					GetScrollState(handler, scrollViewer, targetOffset, positionChanges, currentItemChanges, scrollToRequests));
				Assert.Same(items[1], carouselView.CurrentItem);
				Assert.Equal(1, positionChanges);
				Assert.Equal(1, currentItemChanges);
				Assert.Equal(0, scrollToRequests);
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition + 1), 0, 1);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				targetOffset = scrollViewer.HorizontalOffset - itemWidth * 0.75;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				targetOffset -= 1;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				Assert.True(
					carouselView.Position == 0,
					GetScrollState(handler, scrollViewer, targetOffset, positionChanges, currentItemChanges, scrollToRequests));
				Assert.Same(items[0], carouselView.CurrentItem);
				Assert.Equal(2, positionChanges);
				Assert.Equal(2, currentItemChanges);
				Assert.Equal(0, scrollToRequests);
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition), 0, 1);
			});
		}

		[Fact]
		public async Task VerticalNativeScrollUpdatesSelectionWithoutProgrammaticFeedbackAndSettlesCentered()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(
				items,
				SnapPointsType.MandatorySingle,
				loop: false,
				ItemsLayoutOrientation.Vertical);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.VerticalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemHeight) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.VerticalSnapPointsType = WSnapPointsType.None;

				int positionChanges = 0;
				int scrollToRequests = 0;
				carouselView.PositionChanged += (_, _) => positionChanges++;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				targetOffset += itemHeight * 0.5 + 1;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset, ItemsLayoutOrientation.Vertical);

				Assert.True(
					carouselView.Position == 1,
					GetScrollState(
						handler,
						scrollViewer,
						targetOffset,
						positionChanges,
						scrollToRequests: scrollToRequests));
				Assert.Same(items[1], carouselView.CurrentItem);
				Assert.Equal(1, positionChanges);
				Assert.Equal(0, scrollToRequests);
				Assert.InRange(
					GetCenterError(handler, scrollViewer, initialPhysicalPosition + 1, ItemsLayoutOrientation.Vertical),
					0,
					1);
			});
		}

		[Fact]
		public async Task DefaultNativeSnapUpdatesSelectionWithoutProgrammaticFeedback()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsAlignment = WSnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.MandatorySingle;

				int positionChanges = 0;
				int scrollToRequests = 0;
				carouselView.PositionChanged += (_, _) => positionChanges++;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				await ChangeViewAndWaitForSettleAsync(
					scrollViewer,
					targetOffset + itemWidth,
					disableAnimation: false);

				Assert.True(
					carouselView.Position == 1,
					GetScrollState(
						handler,
						scrollViewer,
						targetOffset + itemWidth,
						positionChanges,
						scrollToRequests: scrollToRequests));
				Assert.Same(items[1], carouselView.CurrentItem);
				Assert.Equal(1, positionChanges);
				Assert.Equal(0, scrollToRequests);
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition + 1), 0, 1);
				Assert.Equal(WSnapPointsType.MandatorySingle, scrollViewer.HorizontalSnapPointsType);
				Assert.Equal(WSnapPointsAlignment.Center, scrollViewer.HorizontalSnapPointsAlignment);
			});
		}

		[Fact]
		public async Task LoopedPhysicalPositionMapsWithoutProgrammaticFeedback()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: true);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);
				carouselView.Position = items.Length - 1;
				await WaitForCenteredPositionAsync(handler, scrollViewer, items.Length - 1);
				await DrainDispatcherQueueAsync(scrollViewer);
				Assert.Equal(items.Length - 1, carouselView.Position);

				int positionChanges = 0;
				int scrollToRequests = 0;
				carouselView.PositionChanged += (_, _) => positionChanges++;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				handler.SetPositionFromScroll(items.Length);

				Assert.Equal(0, carouselView.Position);
				Assert.Same(items[0], carouselView.CurrentItem);
				Assert.Equal(1, positionChanges);
				Assert.Equal(0, scrollToRequests);
			});
		}

		[Fact]
		public async Task LoopedNativeScrollUpdatesSelectionAndSettlesCentered()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: true);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;

				int scrollToRequests = 0;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				targetOffset += itemWidth * 0.5 + 1;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				var expectedPosition = (initialPhysicalPosition + 1) % items.Length;
				Assert.Equal(expectedPosition, carouselView.Position);
				Assert.Same(items[expectedPosition], carouselView.CurrentItem);
				Assert.Equal(0, scrollToRequests);
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition + 1), 0, 1);
			});
		}

		[Fact]
		public async Task NewNativeScrollOverridesPendingRecentering()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;

				bool injectedNewestScroll = false;
				int scrollToRequests = 0;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				void InjectNewestScroll(object sender, Microsoft.UI.Xaml.Controls.ScrollViewerViewChangedEventArgs args)
				{
					if (args.IsIntermediate || injectedNewestScroll)
						return;

					injectedNewestScroll = true;
					scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
					Assert.True(scrollViewer.ChangeView(0, null, null, true));
				}

				scrollViewer.ViewChanged += InjectNewestScroll;
				try
				{
					await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset + itemWidth * 0.5 + 1);
				}
				finally
				{
					scrollViewer.ViewChanged -= InjectNewestScroll;
				}

				Assert.True(injectedNewestScroll);
				Assert.Equal(0, carouselView.Position);
				Assert.Same(items[0], carouselView.CurrentItem);
				Assert.Equal(0, scrollToRequests);
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition), 0, 1);
			});
		}

		[Fact]
		public async Task NewNativeScrollRecoversFromUnacknowledgedRecentering()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (_, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				SetPrivateField(handler, "_isRecentering", true);
				SetPrivateField(handler, "_recenteringHorizontalOffset", scrollViewer.ScrollableWidth);
				SetPrivateField(handler, "_recenteringVerticalOffset", 0d);
				SetPrivateField(handler, "_recenteringAttemptCount", 1);

				await ChangeViewAndWaitForSettleAsync(scrollViewer, itemWidth);

				Assert.False(GetPrivateField<bool>(handler, "_isRecentering"));
				Assert.Equal(1, GetPrivateField<int>(handler, "_recenteringAttemptCount"));
				Assert.False(carouselView.IsDragging);
				Assert.False(carouselView.IsScrolling);
			});
		}

		[Fact]
		public async Task NewNativeScrollOverridesAcknowledgedRecenteringAndReportsDrag()
		{
			SetupBuilder();
			var carouselView = CreateCarouselView(CreateItems(), SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;

				bool injectedNewestScroll = false;
				bool observedDragging = false;
				carouselView.PropertyChanged += (_, args) =>
				{
					if (injectedNewestScroll
						&& args.PropertyName == nameof(CarouselView.IsDragging)
						&& carouselView.IsDragging)
					{
						observedDragging = true;
					}
				};

				void InjectNewestScroll(object sender, Microsoft.UI.Xaml.Controls.ScrollViewerViewChangingEventArgs args)
				{
					if (injectedNewestScroll || !GetPrivateField<bool>(handler, "_isRecentering"))
						return;

					injectedNewestScroll = true;
					Assert.True(scrollViewer.ChangeView(0, null, null, true));
				}

				scrollViewer.ViewChanging += InjectNewestScroll;
				try
				{
					await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset + itemWidth * 0.5 + 1);
				}
				finally
				{
					scrollViewer.ViewChanging -= InjectNewestScroll;
				}

				Assert.True(injectedNewestScroll);
				Assert.True(observedDragging);
				Assert.False(GetPrivateField<bool>(handler, "_isRecentering"));
				Assert.False(carouselView.IsDragging);
				Assert.False(carouselView.IsScrolling);
			});
		}

		[Fact]
		public async Task NativeScrollWithNoSnapPointsIsNotForciblyCentered()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.None, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				int scrollToRequests = 0;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				targetOffset += itemWidth * 0.5 + 1;

				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				Assert.True(
					carouselView.Position == 1,
					GetScrollState(handler, scrollViewer, targetOffset, scrollToRequests: scrollToRequests));
				Assert.Same(items[1], carouselView.CurrentItem);
				Assert.InRange(Math.Abs(scrollViewer.HorizontalOffset - targetOffset), 0, 1);
				Assert.True(
					GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition + 1) > 10,
					"SnapPointsType.None should preserve the native off-center offset.");
				Assert.Equal(0, scrollToRequests);
			});
		}

		[Fact]
		public async Task CollectionChangeWithKeepScrollOffsetPreservesCurrentItem()
		{
			SetupBuilder();
			var originalItem = new object();
			var items = new ObservableCollection<object> { originalItem, new object(), new object() };
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);

				var positionChanged = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
				var layoutUpdated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
				int positionChanges = 0;
				int scrollToRequests = 0;
				carouselView.PositionChanged += (_, _) =>
				{
					positionChanges++;
					if (carouselView.Position == 1)
						positionChanged.TrySetResult(true);
				};
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				void OnLayoutUpdated(object sender, object args) => layoutUpdated.TrySetResult(true);

				scrollViewer.LayoutUpdated += OnLayoutUpdated;
				try
				{
					items.Insert(0, new object());
					await Task.WhenAll(positionChanged.Task, layoutUpdated.Task).WaitAsync(TimeSpan.FromSeconds(3));
					await DrainDispatcherQueueAsync(scrollViewer);
				}
				finally
				{
					scrollViewer.LayoutUpdated -= OnLayoutUpdated;
				}

				Assert.Equal(1, carouselView.Position);
				Assert.Same(originalItem, carouselView.CurrentItem);
				Assert.Equal(1, positionChanges);
				Assert.Equal(0, scrollToRequests);
			});
		}

		[Fact]
		public async Task CollectionChangeTerminalScrollRecentersCapturedItem()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				SetPrivateField(handler, "_isCollectionChangeScrollPending", true);

				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset + itemWidth * 0.5 + 1);

				Assert.Equal(1, carouselView.Position);
				Assert.Same(items[1], carouselView.CurrentItem);
				Assert.False(GetPrivateField<bool>(handler, "_isCollectionChangeScrollPending"));
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition + 1), 0, 1);
			});
		}

		[Fact]
		public async Task CollectionChangeDuringProgrammaticScrollPreservesReconciliation()
		{
			SetupBuilder();
			var items = new ObservableCollection<object>(CreateItems());
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);

				SetPrivateField(handler, "_isCollectionChanged", true);
				SetPrivateField(handler, "_gotoPosition", 1);
				carouselView.SendScrolled(new ItemsViewScrolledEventArgs { CenterItemIndex = 1 });

				Assert.False(GetPrivateField<bool>(handler, "_isCollectionChanged"));
				Assert.True(GetPrivateField<bool>(handler, "_isCollectionChangeScrollPending"));
				Assert.Equal(0, GetPrivateField<int>(handler, "_centerItemIndexFromScroll"));

				SetPrivateField(handler, "_gotoPosition", -1);
			});
		}

		[Fact]
		public async Task CollectionChangeWithoutNativeScrollDoesNotSuppressNextScroll()
		{
			SetupBuilder();
			var items = new ObservableCollection<object>(CreateItems());
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				items.Add(new object());
				await DrainDispatcherQueueAsync(scrollViewer);
				Assert.True(GetPrivateField<bool>(handler, "_isCollectionChanged"));
				SetPrivateField(handler, "_isPointerPressed", true);

				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var itemWidth = GetItemLength(
					GetItemContainer(handler, initialPhysicalPosition),
					ItemsLayoutOrientation.Horizontal);
				var targetOffset = scrollViewer.HorizontalOffset + itemWidth * 0.5 + 1;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				Assert.Equal(1, carouselView.Position);
				Assert.Same(items[1], carouselView.CurrentItem);
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition + 1), 0, 1);
			});
		}

		[Fact]
		public async Task RemovingCurrentItemDoesNotSuppressNextScroll()
		{
			SetupBuilder();
			var items = new ObservableCollection<object>(CreateItems());
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);
				int scrollToRequests = 0;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				items.RemoveAt(0);
				await DrainDispatcherQueueAsync(scrollViewer);

				Assert.Equal(0, carouselView.Position);
				Assert.Same(items[0], carouselView.CurrentItem);

				SetPrivateField(handler, "_isPointerPressed", true);
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var itemWidth = GetItemLength(
					GetItemContainer(handler, initialPhysicalPosition),
					ItemsLayoutOrientation.Horizontal);
				var targetOffset = scrollViewer.HorizontalOffset + itemWidth * 0.5 + 1;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				Assert.Equal(1, carouselView.Position);
				Assert.Same(items[1], carouselView.CurrentItem);
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition + 1), 0, 1);
				Assert.Equal(0, scrollToRequests);
			});
		}

		[Fact]
		public async Task PointerInputWithoutNativeScrollDoesNotLeaveDraggingState()
		{
			SetupBuilder();
			var carouselView = CreateCarouselView(CreateItems(), SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);

				InvokePrivateMethod(handler, "OnScrollViewerPointerPressed", scrollViewer, null);
				Assert.True(GetPrivateField<bool>(handler, "_isPointerPressed"));
				Assert.False(carouselView.IsDragging);

				InvokePrivateMethod(handler, "OnScrollViewerPointerReleased", scrollViewer, null);
				Assert.False(GetPrivateField<bool>(handler, "_isPointerPressed"));

				InvokePrivateMethod(handler, "OnScrollViewerPointerWheelChanged", scrollViewer, null);
				Assert.True(GetPrivateField<bool>(handler, "_hasPendingPointerWheelInput"));
				Assert.False(carouselView.IsDragging);

				await Task.Delay(100);
				await DrainDispatcherQueueAsync(scrollViewer);

				Assert.False(GetPrivateField<bool>(handler, "_hasPendingPointerWheelInput"));
				Assert.False(carouselView.IsDragging);
			});
		}

		[Fact]
		public async Task CollectionChangePreservesSynchronousCurrentItemOverride()
		{
			SetupBuilder();
			var firstItem = new object();
			var replacementItem = new object();
			var overrideItem = new object();
			var items = new ObservableCollection<object> { firstItem, replacementItem, overrideItem };
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);
				int scrollToRequests = 0;
				ScrollToRequestEventArgs scrollToRequest = null;
				carouselView.ScrollToRequested += (_, args) =>
				{
					scrollToRequests++;
					scrollToRequest = args;
				};

				carouselView.CurrentItemChanged += (_, args) =>
				{
					if (ReferenceEquals(args.CurrentItem, replacementItem))
						carouselView.CurrentItem = overrideItem;
				};

				items.RemoveAt(0);
				Assert.Equal(1, carouselView.Position);
				Assert.Same(overrideItem, carouselView.CurrentItem);
				await WaitForCenteredPositionAsync(handler, scrollViewer, logicalPosition: 1);

				Assert.Equal(1, carouselView.Position);
				Assert.Same(overrideItem, carouselView.CurrentItem);
				Assert.Equal(1, scrollToRequests);
				Assert.Equal(ScrollToMode.Position, scrollToRequest.Mode);
				Assert.Equal(1, scrollToRequest.Index);
				Assert.Equal(ScrollToPosition.Center, scrollToRequest.ScrollToPosition);
				Assert.False(scrollToRequest.IsAnimated);
			});
		}

		[Fact]
		public async Task CollectionChangePreservesCurrentItemOverrideFromPositionChanged()
		{
			SetupBuilder();
			var firstItem = new object();
			var secondItem = new object();
			var overrideItem = new object();
			var items = new ObservableCollection<object> { firstItem, secondItem, overrideItem };
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);
				int overrideScrollToRequests = 0;
				ScrollToRequestEventArgs scrollToRequest = null;
				carouselView.ScrollToRequested += (_, args) =>
				{
					if (args.Mode == ScrollToMode.Position && args.Index == 3)
					{
						overrideScrollToRequests++;
						scrollToRequest = args;
					}
				};

				carouselView.PositionChanged += (_, args) =>
				{
					if (args.CurrentPosition == 1)
						carouselView.CurrentItem = overrideItem;
				};

				items.Insert(0, new object());
				await WaitForCenteredPositionAsync(handler, scrollViewer, logicalPosition: 3);

				Assert.Equal(3, carouselView.Position);
				Assert.Same(overrideItem, carouselView.CurrentItem);
				await AssertEventually(
					() => overrideScrollToRequests == 1,
					timeout: 2000,
					message: "The deferred current-item override did not request its final centered scroll.");
				Assert.Equal(1, overrideScrollToRequests);
				Assert.Equal(ScrollToMode.Position, scrollToRequest.Mode);
				Assert.Equal(3, scrollToRequest.Index);
				Assert.Equal(ScrollToPosition.Center, scrollToRequest.ScrollToPosition);
				Assert.False(scrollToRequest.IsAnimated);
			});
		}

		[Fact]
		public async Task CollectionChangePreservesPositionOverrideFromCurrentItemChanged()
		{
			SetupBuilder();
			var firstItem = new object();
			var secondItem = new object();
			var overrideItem = new object();
			var items = new ObservableCollection<object> { firstItem, secondItem, overrideItem };
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.CurrentItemChanged += (_, args) =>
				{
					if (ReferenceEquals(args.CurrentItem, secondItem))
						carouselView.Position = 1;
				};

				items.RemoveAt(0);
				await WaitForCenteredPositionAsync(handler, scrollViewer, logicalPosition: 1);

				Assert.Equal(1, carouselView.Position);
				Assert.Same(overrideItem, carouselView.CurrentItem);
			});
		}

		[Fact]
		public async Task CompletedCollectionOverrideDoesNotSuppressLaterCurrentItemUpdate()
		{
			SetupBuilder();
			var firstItem = new object();
			var replacementItem = new object();
			var overrideItem = new object();
			var items = new ObservableCollection<object> { firstItem, replacementItem, overrideItem };
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);
				int overrideScrollToRequests = 0;
				carouselView.ScrollToRequested += (_, args) =>
				{
					if (args.Mode == ScrollToMode.Position && args.Index == 1)
						overrideScrollToRequests++;
				};

				bool overrideApplied = false;
				carouselView.CurrentItemChanged += (_, args) =>
				{
					if (!overrideApplied && ReferenceEquals(args.CurrentItem, replacementItem))
					{
						overrideApplied = true;
						carouselView.CurrentItem = overrideItem;
					}
				};

				items.RemoveAt(0);
				await WaitForCenteredPositionAsync(handler, scrollViewer, logicalPosition: 1);
				await AssertEventually(
					() => overrideScrollToRequests == 1,
					timeout: 2000,
					message: "The completed current-item override did not request its final centered scroll.");
				Assert.Equal(1, overrideScrollToRequests);

				handler.SetPositionFromScroll(0);
				Assert.Same(replacementItem, carouselView.CurrentItem);

				carouselView.CurrentItem = overrideItem;

				Assert.Equal(2, overrideScrollToRequests);
			});
		}

		[Fact]
		public async Task CollectionChangeDefersNewCurrentItemOverrideUntilNativeViewUpdates()
		{
			SetupBuilder();
			var firstItem = new object();
			var secondItem = new object();
			var overrideItem = new object();
			var items = new ObservableCollection<object> { firstItem, secondItem };
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);
				carouselView.Position = 1;
				await WaitForCenteredPositionAsync(handler, scrollViewer, logicalPosition: 1);
				await DrainDispatcherQueueAsync(scrollViewer);
				Assert.Same(secondItem, carouselView.CurrentItem);

				int overrideScrollToRequests = 0;
				ScrollToRequestEventArgs scrollToRequest = null;
				carouselView.ScrollToRequested += (_, args) =>
				{
					if (args.Mode == ScrollToMode.Position && args.Index == 2)
					{
						overrideScrollToRequests++;
						scrollToRequest = args;
					}
				};

				carouselView.CurrentItemChanged += (_, args) =>
				{
					if (ReferenceEquals(args.CurrentItem, firstItem))
					{
						carouselView.CurrentItem = overrideItem;
					}
				};

				items.Add(overrideItem);
				Assert.Equal(0, overrideScrollToRequests);
				handler.SetPositionFromScroll(1);
				Assert.Equal(2, carouselView.Position);
				Assert.Same(overrideItem, carouselView.CurrentItem);
				await WaitForCenteredPositionAsync(handler, scrollViewer, logicalPosition: 2);

				Assert.Equal(2, carouselView.Position);
				Assert.Same(overrideItem, carouselView.CurrentItem);
				await AssertEventually(
					() => overrideScrollToRequests == 1,
					timeout: 2000,
					message: "The realized current-item override did not request its final centered scroll.");
				Assert.Equal(1, overrideScrollToRequests);
				Assert.Equal(ScrollToMode.Position, scrollToRequest.Mode);
				Assert.Equal(2, scrollToRequest.Index);
				Assert.Equal(ScrollToPosition.Center, scrollToRequest.ScrollToPosition);
				Assert.False(scrollToRequest.IsAnimated);
			});
		}

		[Fact]
		public async Task UnrealizedCollectionOverrideStopsWaitingAfterBoundedRetries()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);
				var pendingItem = new object();
				carouselView.CurrentItem = pendingItem;
				SetPrivateField(handler, "_collectionCurrentItemOverride", pendingItem);

				InvokePrivateMethod(
					handler,
					"QueueCollectionCurrentItemOverrideScroll",
					pendingItem,
					carouselView.Position,
					GetPrivateField<int>(handler, "_collectionChangeVersion"));

				await AssertEventually(
					() => GetPrivateField<object>(handler, "_collectionCurrentItemOverride") is null,
					timeout: 2000,
					message: "The unrealized current-item override remained subscribed after bounded retries.");

				Assert.Null(GetPrivateField<object>(handler, "_collectionCurrentItemOverride"));
				Assert.Null(GetPrivateField<object>(handler, "_collectionCurrentItemOverrideItems"));
			});
		}

		[Fact]
		public async Task ReplacingCurrentItemDoesNotRequestNativeScroll()
		{
			SetupBuilder();
			var items = new ObservableCollection<object>(CreateItems());
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);
				int scrollToRequests = 0;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				var replacementItem = new object();
				items[0] = replacementItem;
				await DrainDispatcherQueueAsync(scrollViewer);

				Assert.Equal(0, carouselView.Position);
				Assert.Same(replacementItem, carouselView.CurrentItem);
				Assert.Equal(0, scrollToRequests);
			});
		}

		[Fact]
		public async Task CollectionChangePreservesDuplicateCurrentItemPosition()
		{
			SetupBuilder();
			var sharedItem = new object();
			var items = new ObservableCollection<object> { sharedItem, sharedItem, new object() };
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var itemWidth = GetItemLength(
					GetItemContainer(handler, initialPhysicalPosition),
					ItemsLayoutOrientation.Horizontal);
				var targetOffset = scrollViewer.HorizontalOffset + itemWidth * 0.5 + 1;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				Assert.Equal(1, carouselView.Position);
				Assert.Same(sharedItem, carouselView.CurrentItem);

				items.RemoveAt(2);
				await DrainDispatcherQueueAsync(scrollViewer);

				Assert.Equal(1, carouselView.Position);
				Assert.Same(sharedItem, carouselView.CurrentItem);
				Assert.InRange(GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition + 1), 0, 1);
			});
		}

		[Fact]
		public async Task CollectionResetWithStalePositionDoesNotCrash()
		{
			SetupBuilder();
			var items = new ResettableObservableCollection<object>(CreateItems());
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);
			carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.Position = 4;
				Assert.Equal(4, carouselView.Position);

				items.Reset(new object(), new object());
				await DrainDispatcherQueueAsync(scrollViewer);

				Assert.NotNull(handler.PlatformView);
			});
		}

		[Fact]
		public async Task NativeScrollWithNullItemsLayoutUpdatesSelectionWithoutCrash()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout = null;
				carouselView.SendScrolled(new ItemsViewScrolledEventArgs
				{
					CenterItemIndex = 1,
					HorizontalDelta = 100,
				});

				Assert.Equal(1, carouselView.Position);
				Assert.Same(items[1], carouselView.CurrentItem);
			});
		}

		[Fact]
		public async Task TerminalCenterIndexUpdatesSelectionWhenGeometryIsUnavailable()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);

				int scrollToRequests = 0;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				SetPrivateField<WScrollViewer>(handler, "_scrollViewer", null);
				try
				{
					handler.CenterCarouselItem(1);
				}
				finally
				{
					SetPrivateField(handler, "_scrollViewer", scrollViewer);
				}

				Assert.Equal(1, carouselView.Position);
				Assert.Same(items[1], carouselView.CurrentItem);
				Assert.Equal(0, scrollToRequests);
			});
		}

		[Fact]
		public void ForwardTieBreakDoesNotCompoundAcrossCandidates()
		{
			var distances = new[] { 0d, 0.9d, 1.8d, 2.7d };
			var closestDistance = double.MaxValue;
			int selectedIndex = -1;

			for (int index = 0; index < distances.Length; index++)
			{
				if (CarouselViewHandler.TrySelectCandidate(distances[index], ref closestDistance, true))
					selectedIndex = index;
			}

			Assert.Equal(1, selectedIndex);
			Assert.Equal(0, closestDistance);
		}

		[Fact]
		public async Task ConflictingNativeSnapCoercionDoesNotRepeatRecentering()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: true);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var conflictingOffset = targetOffset + itemWidth * 0.5 + 1;

				int terminalEvents = 0;
				int coercions = 0;
				int lastCoercedAttempt = 0;
				bool observedRecenteringDrag = false;
				void CountTerminalEvents(object sender, Microsoft.UI.Xaml.Controls.ScrollViewerViewChangedEventArgs args)
				{
					if (!args.IsIntermediate)
						terminalEvents++;
				}

				void ObserveRecenteringDrag(object sender, System.ComponentModel.PropertyChangedEventArgs args)
				{
					if (args.PropertyName == nameof(CarouselView.IsDragging)
						&& GetPrivateField<bool>(handler, "_isRecentering")
						&& carouselView.IsDragging)
					{
						observedRecenteringDrag = true;
					}
				}

				void CoerceRecentering(object sender, Microsoft.UI.Xaml.Controls.ScrollViewerViewChangingEventArgs args)
				{
					var attempt = GetPrivateField<int>(handler, "_recenteringAttemptCount");
					if (!GetPrivateField<bool>(handler, "_isRecentering")
						|| attempt <= lastCoercedAttempt)
					{
						return;
					}

					lastCoercedAttempt = attempt;
					var coercedOffset = conflictingOffset + (attempt == 1 ? -20 : 20);
					coercions++;
					Assert.False(carouselView.IsDragging);
					Assert.False(carouselView.IsScrolling);
					Assert.True(scrollViewer.ChangeView(coercedOffset, null, null, true));
				}

				scrollViewer.ViewChanged += CountTerminalEvents;
				scrollViewer.ViewChanging += CoerceRecentering;
				carouselView.PropertyChanged += ObserveRecenteringDrag;
				try
				{
					await ChangeViewAndWaitForSettleAsync(
						scrollViewer,
						conflictingOffset);
				}
				finally
				{
					scrollViewer.ViewChanged -= CountTerminalEvents;
					scrollViewer.ViewChanging -= CoerceRecentering;
					carouselView.PropertyChanged -= ObserveRecenteringDrag;
				}

				var recenteringAttemptCount = GetPrivateField<int>(handler, "_recenteringAttemptCount");
				Assert.True(
					recenteringAttemptCount == 2,
					$"Expected two bounded recenter attempts. Attempts={recenteringAttemptCount}, " +
					$"TerminalEvents={terminalEvents}, HorizontalOffset={scrollViewer.HorizontalOffset}, " +
					$"TargetOffset={targetOffset}, ItemWidth={itemWidth}, " +
					$"CenterError={GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition)}.");
				Assert.Equal(2, coercions);
				Assert.False(observedRecenteringDrag);
				Assert.InRange(terminalEvents, 3, 6);
			});
		}

		[Fact]
		public async Task DisconnectFromPositionChangedDuringCenteringDoesNotCrash()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				var initialPhysicalPosition = await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset + itemWidth * 0.5 + 1);
				Assert.Equal(1, carouselView.Position);
				await ChangeViewAndWaitForSettleAsync(
					scrollViewer,
					GetCenteredOffset(
						handler,
						scrollViewer,
						initialPhysicalPosition + 1,
						ItemsLayoutOrientation.Horizontal));

				SetPrivateField(handler, "_isInternalPositionUpdate", true);
				try
				{
					carouselView.Position = 0;
				}
				finally
				{
					SetPrivateField(handler, "_isInternalPositionUpdate", false);
				}

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				bool disconnected = false;
				carouselView.PositionChanged += (_, args) =>
				{
					if (args.CurrentPosition != 1)
						return;

					disconnected = true;
					((IElementHandler)handler).DisconnectHandler();
				};

				handler.CenterCarouselItem(initialPhysicalPosition + 1);

				Assert.True(disconnected);
				Assert.Null(((IElementHandler)handler).VirtualView);
			});
		}

		[Fact]
		public async Task DisconnectFromCurrentItemChangedDuringNativeScrollDoesNotCrash()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);
				bool disconnected = false;
				carouselView.CurrentItemChanged += (_, args) =>
				{
					if (!ReferenceEquals(args.CurrentItem, items[1]))
						return;

					disconnected = true;
					((IElementHandler)handler).DisconnectHandler();
				};

				handler.SetPositionFromScroll(1);

				Assert.True(disconnected);
				Assert.Null(((IElementHandler)handler).VirtualView);
			});
		}

		[Fact]
		public async Task DisconnectClearsTransientScrollState()
		{
			SetupBuilder();
			var carouselView = CreateCarouselView(CreateItems(), SnapPointsType.MandatorySingle, loop: false);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, handler =>
			{
				SetPrivateField(handler, "_positionUpdateFromScroll", 1);
				SetPrivateField(handler, "_isPositionUpdateFromCollection", true);
				SetPrivateField(handler, "_hasCurrentItemUpdateFromScroll", true);
				SetPrivateField(handler, "_currentItemUpdateFromScroll", new object());
				SetPrivateField(handler, "_hasCurrentItemUpdateFromCollection", true);
				SetPrivateField(handler, "_currentItemUpdateFromCollection", new object());
				SetPrivateField(handler, "_collectionCurrentItemOverride", new object());
				SetPrivateField(handler, "_collectionItemsSource", new ArrayList());
				SetPrivateField(handler, "_isRecentering", true);
				SetPrivateField(handler, "_recenteringHorizontalOffset", 10d);
				SetPrivateField(handler, "_recenteringVerticalOffset", 20d);
				SetPrivateField(handler, "_failedRecenteringOffset", (WPoint?)new WPoint(1, 2));
				SetPrivateField(handler, "_failedRecenteringTarget", (WPoint?)new WPoint(3, 4));
				SetPrivateField(handler, "_recenteringAttemptCount", 2);
				SetPrivateField(handler, "_isScrollingForward", true);
				SetPrivateField(handler, "_centerItemIndexFromScroll", 1);
				SetPrivateField(handler, "_centerRequestVersion", 1);
				SetPrivateField(handler, "_gotoPosition", 1);
				SetPrivateField(handler, "_isCollectionChanged", true);
				SetPrivateField(handler, "_isCollectionChangeScrollPending", true);
				SetPrivateField(handler, "_collectionChangeVersion", 1);
				SetPrivateField(handler, "_isPointerPressed", true);
				SetPrivateField(handler, "_hasPendingPointerWheelInput", true);

				((IElementHandler)handler).DisconnectHandler();

				Assert.Equal(-1, GetPrivateField<int>(handler, "_positionUpdateFromScroll"));
				Assert.False(GetPrivateField<bool>(handler, "_isPositionUpdateFromCollection"));
				Assert.False(GetPrivateField<bool>(handler, "_hasCurrentItemUpdateFromScroll"));
				Assert.Null(GetPrivateField<object>(handler, "_currentItemUpdateFromScroll"));
				Assert.False(GetPrivateField<bool>(handler, "_hasCurrentItemUpdateFromCollection"));
				Assert.Null(GetPrivateField<object>(handler, "_currentItemUpdateFromCollection"));
				Assert.Null(GetPrivateField<object>(handler, "_collectionCurrentItemOverride"));
				Assert.Null(GetPrivateField<IList>(handler, "_collectionItemsSource"));
				Assert.False(GetPrivateField<bool>(handler, "_isRecentering"));
				Assert.Equal(0, GetPrivateField<double>(handler, "_recenteringHorizontalOffset"));
				Assert.Equal(0, GetPrivateField<double>(handler, "_recenteringVerticalOffset"));
				Assert.Null(GetPrivateField<WPoint?>(handler, "_failedRecenteringOffset"));
				Assert.Null(GetPrivateField<WPoint?>(handler, "_failedRecenteringTarget"));
				Assert.Equal(0, GetPrivateField<int>(handler, "_recenteringAttemptCount"));
				Assert.False(GetPrivateField<bool>(handler, "_isScrollingForward"));
				Assert.Equal(-1, GetPrivateField<int>(handler, "_centerItemIndexFromScroll"));
				Assert.Equal(2, GetPrivateField<int>(handler, "_centerRequestVersion"));
				Assert.Equal(-1, GetPrivateField<int>(handler, "_gotoPosition"));
				Assert.False(GetPrivateField<bool>(handler, "_isCollectionChanged"));
				Assert.False(GetPrivateField<bool>(handler, "_isCollectionChangeScrollPending"));
				Assert.Equal(2, GetPrivateField<int>(handler, "_collectionChangeVersion"));
				Assert.False(GetPrivateField<bool>(handler, "_isPointerPressed"));
				Assert.False(GetPrivateField<bool>(handler, "_hasPendingPointerWheelInput"));

				return Task.CompletedTask;
			});
		}

		static object[] CreateItems() =>
			[new object(), new object(), new object(), new object(), new object()];

		sealed class ResettableObservableCollection<T> : ObservableCollection<T>
		{
			public ResettableObservableCollection(IEnumerable items)
			{
				foreach (T item in items)
					Items.Add(item);
			}

			public void Reset(params T[] items)
			{
				Items.Clear();
				foreach (var item in items)
					Items.Add(item);

				OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
					System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
			}
		}

		static CarouselView CreateCarouselView(
			IEnumerable items,
			SnapPointsType snapPointsType,
			bool loop,
			ItemsLayoutOrientation orientation = ItemsLayoutOrientation.Horizontal)
		{
			return new CarouselView
			{
				HeightRequest = 200,
				IsScrollAnimated = true,
				ItemTemplate = new DataTemplate(() => new Grid { new Label() }),
				ItemsLayout = new LinearItemsLayout(orientation)
				{
					SnapPointsAlignment = SnapPointsAlignment.Center,
					SnapPointsType = snapPointsType,
				},
				Loop = loop,
				ItemsSource = items,
				WidthRequest = 400,
			};
		}

		static async Task<int> WaitForInitialPositionAsync(CarouselViewHandler handler, WScrollViewer scrollViewer)
		{
			var orientation = handler.VirtualView.ItemsLayout.Orientation;
			bool nativeViewReady = await Wait(
				() =>
				{
					handler.PlatformView.UpdateLayout();
					return GetViewportLength(scrollViewer, orientation) > 0
						&& handler.PlatformView.Items.Count > 0
						&& handler.PlatformView.ItemsPanelRoot is Microsoft.UI.Xaml.Controls.ItemsStackPanel itemsPanel
						&& itemsPanel.FirstVisibleIndex >= 0
						&& handler.PlatformView.ContainerFromIndex(itemsPanel.FirstVisibleIndex) is WFrameworkElement container
						&& GetItemLength(container, orientation) > 0;
				},
				timeout: 10000);

			Assert.True(nativeViewReady, "CarouselView did not realize its initial native item.");
			return await WaitForCenteredPositionAsync(handler, scrollViewer, logicalPosition: 0);
		}

		static async Task<int> WaitForCenteredPositionAsync(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			int logicalPosition)
		{
			var orientation = handler.VirtualView.ItemsLayout.Orientation;
			bool isReady = await Wait(
				() =>
				{
					handler.PlatformView.UpdateLayout();
					return GetViewportLength(scrollViewer, orientation) > 0
						&& GetCenteredPhysicalPosition(handler, scrollViewer, logicalPosition) >= 0;
				},
				timeout: 3000);

			var physicalPosition = GetCenteredPhysicalPosition(handler, scrollViewer, logicalPosition);
			Assert.True(
				isReady,
				$"CarouselView did not finish its layout and centering. " +
				$"ViewportLength={GetViewportLength(scrollViewer, orientation)}, " +
				$"ScrollableLength={GetScrollableLength(scrollViewer, orientation)}, " +
				$"Offset={GetOffset(scrollViewer, orientation)}, " +
				$"Position={handler.VirtualView.Position}, PhysicalPosition={physicalPosition}, " +
				$"NativeState={GetNativeLayoutState(handler, scrollViewer, orientation)}.");

			return physicalPosition;
		}

		static string GetNativeLayoutState(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			ItemsLayoutOrientation orientation)
		{
			var itemsPanel = handler.PlatformView.ItemsPanelRoot as Microsoft.UI.Xaml.Controls.ItemsStackPanel;
			var containers = Enumerable.Range(0, handler.PlatformView.Items.Count)
				.Select(index =>
				{
					if (handler.PlatformView.ContainerFromIndex(index) is not WFrameworkElement container)
						return $"{index}:unrealized";

					return $"{index}:length={GetItemLength(container, orientation)},center={GetCenteredOffset(handler, scrollViewer, index, orientation)}";
				});

			return $"Items={handler.PlatformView.Items.Count},Source={handler.VirtualView.ItemsSource.Cast<object>().Count()}," +
				$"Visible={itemsPanel?.FirstVisibleIndex}-{itemsPanel?.LastVisibleIndex}," +
				$"Containers=[{string.Join(";", containers)}]";
		}

		static Task ChangeViewAndWaitForSettleAsync(
			WScrollViewer scrollViewer,
			double offset,
			ItemsLayoutOrientation orientation = ItemsLayoutOrientation.Horizontal,
			bool disableAnimation = true)
		{
			return WaitForSettleAsync(
				scrollViewer,
				() => Assert.True(
					ChangeView(scrollViewer, offset, orientation, disableAnimation),
					"Native ScrollViewer rejected the requested offset."));
		}

		static async Task WaitForSettleAsync(WScrollViewer scrollViewer, Action action)
		{
			var settled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			int terminalViewChangedVersion = 0;

			void OnViewChanged(object sender, Microsoft.UI.Xaml.Controls.ScrollViewerViewChangedEventArgs args)
			{
				if (args.IsIntermediate)
					return;

				int observedVersion = ++terminalViewChangedVersion;
				_ = CompleteAfterQuietPeriodAsync(observedVersion);
			}

			async Task CompleteAfterQuietPeriodAsync(int observedVersion)
			{
				await Task.Delay(100);
				await DrainDispatcherQueueAsync(scrollViewer);
				if (observedVersion == terminalViewChangedVersion)
					settled.TrySetResult(true);
			}

			scrollViewer.ViewChanged += OnViewChanged;
			try
			{
				action();
				await settled.Task.WaitAsync(TimeSpan.FromSeconds(3));
			}
			finally
			{
				scrollViewer.ViewChanged -= OnViewChanged;
			}
		}

		static async Task<(double Offset, double ItemWidth)> PrimeNativeScrollAsync(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			int initialPhysicalPosition)
		{
			var orientation = handler.VirtualView.ItemsLayout.Orientation;
			var itemLength = GetItemLength(GetItemContainer(handler, initialPhysicalPosition), orientation);
			var targetOffset = GetOffset(scrollViewer, orientation) + itemLength * 0.25;
			Assert.True(
				ChangeView(scrollViewer, targetOffset, orientation),
				"Native ScrollViewer rejected the initial wheel-equivalent offset.");

			await AssertEventually(
				() =>
				{
					handler.PlatformView.UpdateLayout();
					return Math.Abs(GetOffset(scrollViewer, orientation) - targetOffset) <= 1
						&& handler.PlatformView.ItemsPanelRoot is Microsoft.UI.Xaml.Controls.ItemsStackPanel itemsPanel
						&& itemsPanel.FirstVisibleIndex <= initialPhysicalPosition
						&& itemsPanel.LastVisibleIndex >= initialPhysicalPosition + 1;
				},
				timeout: 3000,
				message: "CarouselView did not realize the adjacent item after the native scroll.");

			return (targetOffset, itemLength);
		}

		static int GetCenteredPhysicalPosition(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			int logicalPosition)
		{
			if (handler.PlatformView.ItemsPanelRoot is not Microsoft.UI.Xaml.Controls.ItemsStackPanel itemsPanel)
				return -1;

			var itemCount = handler.VirtualView.ItemsSource.Cast<object>().Count();
			var orientation = handler.VirtualView.ItemsLayout.Orientation;
			for (int index = itemsPanel.FirstVisibleIndex; index <= itemsPanel.LastVisibleIndex; index++)
			{
				if (index >= 0
					&& itemCount > 0
					&& index % itemCount == logicalPosition
					&& handler.PlatformView.ContainerFromIndex(index) is WFrameworkElement container
					&& GetItemLength(container, orientation) > 0
					&& GetCenterError(handler, scrollViewer, index, orientation) <= 1)
				{
					return index;
				}
			}

			return -1;
		}

		static WFrameworkElement GetItemContainer(CarouselViewHandler handler, int index) =>
			Assert.IsType<Microsoft.UI.Xaml.Controls.ListViewItem>(handler.PlatformView.ContainerFromIndex(index));

		static double GetItemLength(WFrameworkElement container, ItemsLayoutOrientation orientation) =>
			orientation == ItemsLayoutOrientation.Horizontal ? container.ActualWidth : container.ActualHeight;

		static double GetViewportLength(WScrollViewer scrollViewer, ItemsLayoutOrientation orientation) =>
			orientation == ItemsLayoutOrientation.Horizontal ? scrollViewer.ViewportWidth : scrollViewer.ViewportHeight;

		static double GetScrollableLength(WScrollViewer scrollViewer, ItemsLayoutOrientation orientation) =>
			orientation == ItemsLayoutOrientation.Horizontal ? scrollViewer.ScrollableWidth : scrollViewer.ScrollableHeight;

		static double GetOffset(WScrollViewer scrollViewer, ItemsLayoutOrientation orientation) =>
			orientation == ItemsLayoutOrientation.Horizontal ? scrollViewer.HorizontalOffset : scrollViewer.VerticalOffset;

		static bool ChangeView(
			WScrollViewer scrollViewer,
			double offset,
			ItemsLayoutOrientation orientation,
			bool disableAnimation = true) =>
			scrollViewer.ChangeView(
				orientation == ItemsLayoutOrientation.Horizontal ? offset : null,
				orientation == ItemsLayoutOrientation.Vertical ? offset : null,
				null,
				disableAnimation);

		static double GetCenteredOffset(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			int index,
			ItemsLayoutOrientation orientation)
		{
			var container = GetItemContainer(handler, index);
			var position = container.TransformToVisual((WUIElement)scrollViewer.Content).TransformPoint(new WPoint());
			return orientation == ItemsLayoutOrientation.Horizontal
				? position.X - ((scrollViewer.ViewportWidth - container.ActualWidth) / 2)
				: position.Y - ((scrollViewer.ViewportHeight - container.ActualHeight) / 2);
		}

		static double GetHorizontalCenterError(CarouselViewHandler handler, WScrollViewer scrollViewer, int index) =>
			GetCenterError(handler, scrollViewer, index, ItemsLayoutOrientation.Horizontal);

		static double GetCenterError(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			int index,
			ItemsLayoutOrientation orientation) =>
			Math.Abs(GetOffset(scrollViewer, orientation) - GetCenteredOffset(handler, scrollViewer, index, orientation));

		static T GetPrivateField<T>(CarouselViewHandler handler, string fieldName)
		{
			var field = typeof(CarouselViewHandler).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(field);
			return (T)field.GetValue(handler);
		}

		static void SetPrivateField<T>(CarouselViewHandler handler, string fieldName, T value)
		{
			var field = typeof(CarouselViewHandler).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(field);
			field.SetValue(handler, value);
		}

		static void InvokePrivateMethod(CarouselViewHandler handler, string methodName, params object[] arguments)
		{
			var method = typeof(CarouselViewHandler).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(method);
			method.Invoke(handler, arguments);
		}

		static async Task DrainDispatcherQueueAsync(WUIElement element)
		{
			var drained = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			Assert.True(element.DispatcherQueue.TryEnqueue(
				Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
				() => drained.TrySetResult(true)));
			await drained.Task.WaitAsync(TimeSpan.FromSeconds(3));
		}

		static string GetScrollState(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			double targetOffset,
			int positionChanges = 0,
			int currentItemChanges = 0,
			int scrollToRequests = 0)
		{
			var itemsPanel = Assert.IsType<Microsoft.UI.Xaml.Controls.ItemsStackPanel>(handler.PlatformView.ItemsPanelRoot);
			return $"Native scroll did not reach the expected state. Position={handler.VirtualView.Position}, " +
				$"HorizontalOffset={scrollViewer.HorizontalOffset}, VerticalOffset={scrollViewer.VerticalOffset}, " +
				$"TargetOffset={targetOffset}, " +
				$"FirstVisibleIndex={itemsPanel.FirstVisibleIndex}, LastVisibleIndex={itemsPanel.LastVisibleIndex}, " +
				$"PositionChanges={positionChanges}, CurrentItemChanges={currentItemChanges}, " +
				$"ScrollToRequests={scrollToRequests}.";
		}

	}
}
