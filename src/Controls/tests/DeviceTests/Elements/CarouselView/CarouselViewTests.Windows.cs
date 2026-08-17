using System;
using System.Linq;
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

		static object[] CreateItems() =>
			[new object(), new object(), new object(), new object(), new object()];

		static CarouselView CreateCarouselView(object[] items, SnapPointsType snapPointsType, bool loop)
		{
			return new CarouselView
			{
				HeightRequest = 200,
				IsScrollAnimated = true,
				ItemTemplate = new DataTemplate(() => new Grid { new Label() }),
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
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
			bool isReady = await Wait(
				() => scrollViewer.ViewportWidth > 0
					&& GetCenteredPhysicalPosition(handler, scrollViewer, logicalPosition) >= 0,
				timeout: 3000);

			var physicalPosition = GetCenteredPhysicalPosition(handler, scrollViewer, logicalPosition);
			Assert.True(
				isReady,
				$"CarouselView did not finish its initial layout and centering. " +
				$"ViewportWidth={scrollViewer.ViewportWidth}, ScrollableWidth={scrollViewer.ScrollableWidth}, " +
				$"HorizontalOffset={scrollViewer.HorizontalOffset}, " +
				$"Position={handler.VirtualView.Position}, PhysicalPosition={physicalPosition}.");

			return physicalPosition;
		}

		static async Task ChangeViewAndWaitForSettleAsync(WScrollViewer scrollViewer, double horizontalOffset)
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
				Assert.True(
					scrollViewer.ChangeView(horizontalOffset, null, null, true),
					"Native ScrollViewer rejected the requested offset.");

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
			var itemWidth = GetItemContainer(handler, initialPhysicalPosition).ActualWidth;
			var targetOffset = scrollViewer.HorizontalOffset + itemWidth * 0.25;
			Assert.True(
				scrollViewer.ChangeView(targetOffset, null, null, true),
				"Native ScrollViewer rejected the initial wheel-equivalent offset.");

			await AssertEventually(
				() => Math.Abs(scrollViewer.HorizontalOffset - targetOffset) <= 1
					&& handler.PlatformView.ItemsPanelRoot is Microsoft.UI.Xaml.Controls.ItemsStackPanel itemsPanel
					&& itemsPanel.FirstVisibleIndex <= initialPhysicalPosition
					&& itemsPanel.LastVisibleIndex >= initialPhysicalPosition + 1,
				timeout: 3000,
				message: "CarouselView did not realize the adjacent item after the native scroll.");

			return (targetOffset, itemWidth);
		}

		static int GetCenteredPhysicalPosition(
			CarouselViewHandler handler,
			WScrollViewer scrollViewer,
			int logicalPosition)
		{
			if (handler.PlatformView.ItemsPanelRoot is not Microsoft.UI.Xaml.Controls.ItemsStackPanel itemsPanel)
				return -1;

			for (int index = itemsPanel.FirstVisibleIndex; index <= itemsPanel.LastVisibleIndex; index++)
			{
				if (index >= 0
					&& index % 5 == logicalPosition
					&& handler.PlatformView.ContainerFromIndex(index) is WFrameworkElement container
					&& container.ActualWidth > 0
					&& GetHorizontalCenterError(handler, scrollViewer, index) <= 1)
				{
					return index;
				}
			}

			return -1;
		}

		static WFrameworkElement GetItemContainer(CarouselViewHandler handler, int index) =>
			Assert.IsType<Microsoft.UI.Xaml.Controls.ListViewItem>(handler.PlatformView.ContainerFromIndex(index));

		static double GetCenteredHorizontalOffset(CarouselViewHandler handler, WScrollViewer scrollViewer, int index)
		{
			var container = GetItemContainer(handler, index);
			var position = container.TransformToVisual((WUIElement)scrollViewer.Content).TransformPoint(new WPoint());
			return position.X - ((scrollViewer.ViewportWidth - container.ActualWidth) / 2);
		}

		static double GetHorizontalCenterError(CarouselViewHandler handler, WScrollViewer scrollViewer, int index) =>
			Math.Abs(scrollViewer.HorizontalOffset - GetCenteredHorizontalOffset(handler, scrollViewer, index));

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
