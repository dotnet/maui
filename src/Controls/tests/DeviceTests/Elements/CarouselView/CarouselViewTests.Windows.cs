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
		public async Task NativeScrollMapsLoopedPhysicalPositionWithoutProgrammaticFeedback()
		{
			SetupBuilder();
			var items = CreateItems();
			var carouselView = CreateCarouselView(items, SnapPointsType.MandatorySingle, loop: true);

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var scrollViewer = handler.PlatformView.GetChildren<WScrollViewer>().Single();
				await WaitForInitialPositionAsync(handler, scrollViewer);

				carouselView.Position = items.Length - 1;
				var initialPhysicalPosition = await WaitForCenteredPositionAsync(
					handler,
					scrollViewer,
					items.Length - 1);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;
				var (targetOffset, itemWidth) = await PrimeNativeScrollAsync(handler, scrollViewer, initialPhysicalPosition);
				Assert.Equal(items.Length - 1, carouselView.Position);

				carouselView.ItemsLayout.SnapPointsAlignment = SnapPointsAlignment.Center;
				scrollViewer.HorizontalSnapPointsType = WSnapPointsType.None;

				int positionChanges = 0;
				int scrollToRequests = 0;
				carouselView.PositionChanged += (_, _) => positionChanges++;
				carouselView.ScrollToRequested += (_, _) => scrollToRequests++;

				targetOffset += itemWidth * 0.5 + 1;
				await ChangeViewAndWaitForSettleAsync(scrollViewer, targetOffset);

				Assert.Equal(0, carouselView.Position);
				Assert.Same(items[0], carouselView.CurrentItem);
				Assert.Equal(1, positionChanges);
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
					InvokePrivateMethod(handler, "CenterCarouselItem", 1);
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
				var arguments = new object[] { distances[index], closestDistance, true };
				if ((bool)InvokePrivateStaticMethod("TrySelectCandidate", arguments))
					selectedIndex = index;

				closestDistance = (double)arguments[1];
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
				void CoerceRecentering(object sender, Microsoft.UI.Xaml.Controls.ScrollViewerViewChangedEventArgs args)
				{
					if (args.IsIntermediate)
						return;

					terminalEvents++;
					if (coercions >= 2 || !GetPrivateField<bool>(handler, "_isRecentering"))
						return;

					var coercedOffset = conflictingOffset + (coercions == 0 ? -20 : 20);
					coercions++;
					Assert.True(scrollViewer.ChangeView(coercedOffset, null, null, true));
				}

				scrollViewer.ViewChanged += CoerceRecentering;
				try
				{
					await ChangeViewAndWaitForSettleAsync(
						scrollViewer,
						conflictingOffset);
				}
				finally
				{
					scrollViewer.ViewChanged -= CoerceRecentering;
				}

				var recenteringAttemptCount = GetPrivateField<int>(handler, "_recenteringAttemptCount");
				Assert.True(
					recenteringAttemptCount == 2,
					$"Expected two bounded recenter attempts. Attempts={recenteringAttemptCount}, " +
					$"TerminalEvents={terminalEvents}, HorizontalOffset={scrollViewer.HorizontalOffset}, " +
					$"TargetOffset={targetOffset}, ItemWidth={itemWidth}, " +
					$"CenterError={GetHorizontalCenterError(handler, scrollViewer, initialPhysicalPosition)}.");
				Assert.Equal(2, coercions);
				Assert.InRange(terminalEvents, 3, 6);
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
				SetPrivateField(handler, "_hasCurrentItemUpdateFromScroll", true);
				SetPrivateField(handler, "_currentItemUpdateFromScroll", new object());
				SetPrivateField(handler, "_isRecentering", true);
				SetPrivateField(handler, "_recenteringHorizontalOffset", 10d);
				SetPrivateField(handler, "_recenteringVerticalOffset", 20d);
				SetPrivateField(handler, "_failedRecenteringOffset", (WPoint?)new WPoint(1, 2));
				SetPrivateField(handler, "_failedRecenteringTarget", (WPoint?)new WPoint(3, 4));
				SetPrivateField(handler, "_recenteringAttemptCount", 2);
				SetPrivateField(handler, "_isScrollingForward", true);
				SetPrivateField(handler, "_centerItemIndexFromScroll", 1);
				SetPrivateField(handler, "_gotoPosition", 1);
				SetPrivateField(handler, "_isCollectionChanged", true);
				SetPrivateField(handler, "_isCollectionChangeScrollPending", true);

				((IElementHandler)handler).DisconnectHandler();

				Assert.Equal(-1, GetPrivateField<int>(handler, "_positionUpdateFromScroll"));
				Assert.False(GetPrivateField<bool>(handler, "_hasCurrentItemUpdateFromScroll"));
				Assert.Null(GetPrivateField<object>(handler, "_currentItemUpdateFromScroll"));
				Assert.False(GetPrivateField<bool>(handler, "_isRecentering"));
				Assert.Equal(0, GetPrivateField<double>(handler, "_recenteringHorizontalOffset"));
				Assert.Equal(0, GetPrivateField<double>(handler, "_recenteringVerticalOffset"));
				Assert.Null(GetPrivateField<WPoint?>(handler, "_failedRecenteringOffset"));
				Assert.Null(GetPrivateField<WPoint?>(handler, "_failedRecenteringTarget"));
				Assert.Equal(0, GetPrivateField<int>(handler, "_recenteringAttemptCount"));
				Assert.False(GetPrivateField<bool>(handler, "_isScrollingForward"));
				Assert.Equal(-1, GetPrivateField<int>(handler, "_centerItemIndexFromScroll"));
				Assert.Equal(-1, GetPrivateField<int>(handler, "_gotoPosition"));
				Assert.False(GetPrivateField<bool>(handler, "_isCollectionChanged"));
				Assert.False(GetPrivateField<bool>(handler, "_isCollectionChangeScrollPending"));

				return Task.CompletedTask;
			});
		}

		static object[] CreateItems() =>
			[new object(), new object(), new object(), new object(), new object()];

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
			=> await WaitForCenteredPositionAsync(handler, scrollViewer, logicalPosition: 0);

		static async Task<int> WaitForCenteredPositionAsync(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			int logicalPosition)
		{
			var orientation = handler.VirtualView.ItemsLayout.Orientation;
			bool isReady = await Wait(
				() => GetViewportLength(scrollViewer, orientation) > 0
					&& GetCenteredPhysicalPosition(handler, scrollViewer, logicalPosition) >= 0,
				timeout: 3000);

			var physicalPosition = GetCenteredPhysicalPosition(handler, scrollViewer, logicalPosition);
			Assert.True(
				isReady,
				$"CarouselView did not finish its initial layout and centering. " +
				$"ViewportLength={GetViewportLength(scrollViewer, orientation)}, " +
				$"ScrollableLength={GetScrollableLength(scrollViewer, orientation)}, " +
				$"Offset={GetOffset(scrollViewer, orientation)}, " +
				$"Position={handler.VirtualView.Position}, PhysicalPosition={physicalPosition}.");

			return physicalPosition;
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
				() => Math.Abs(GetOffset(scrollViewer, orientation) - targetOffset) <= 1
					&& handler.PlatformView.ItemsPanelRoot is Microsoft.UI.Xaml.Controls.ItemsStackPanel itemsPanel
					&& itemsPanel.FirstVisibleIndex <= initialPhysicalPosition
					&& itemsPanel.LastVisibleIndex >= initialPhysicalPosition + 1,
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

		static object InvokePrivateMethod(CarouselViewHandler handler, string methodName, params object[] arguments)
		{
			var method = typeof(CarouselViewHandler).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(method);
			return method.Invoke(handler, arguments);
		}

		static object InvokePrivateStaticMethod(string methodName, params object[] arguments)
		{
			var method = typeof(CarouselViewHandler).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(method);
			return method.Invoke(null, arguments);
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
			return $"Native scroll did not select item 1. Position={handler.VirtualView.Position}, " +
				$"HorizontalOffset={scrollViewer.HorizontalOffset}, TargetOffset={targetOffset}, " +
				$"FirstVisibleIndex={itemsPanel.FirstVisibleIndex}, LastVisibleIndex={itemsPanel.LastVisibleIndex}, " +
				$"PositionChanges={positionChanges}, CurrentItemChanges={currentItemChanges}, " +
				$"ScrollToRequests={scrollToRequests}.";
		}

	}
}
