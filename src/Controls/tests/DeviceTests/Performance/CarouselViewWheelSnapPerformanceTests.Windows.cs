using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Xunit;
using Xunit.Sdk;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.PerformanceCarouselViewWheelSnap)]
	public class CarouselViewWheelSnapPerformanceTests : ControlsHandlerTestBase
	{
		const int WarmupCount = 2;
		const int IterationCount = 10;
		const double CenterTolerance = 1;

		[Fact(DisplayName = "Windows CarouselView wheel-scroll centering performance")]
		public async Task WheelScrollCentersClosestItem()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CarouselView, CarouselViewHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var items = new List<string>();
			for (var index = 0; index < 20; index++)
				items.Add($"Item {index}");

			var carouselView = new CarouselView
			{
				HeightRequest = 240,
				WidthRequest = 640,
				ItemsSource = items,
				Loop = false,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					SnapPointsAlignment = Microsoft.Maui.Controls.SnapPointsAlignment.Center,
					SnapPointsType = Microsoft.Maui.Controls.SnapPointsType.Mandatory
				},
				ItemTemplate = new Microsoft.Maui.Controls.DataTemplate(() => new Label
				{
					HeightRequest = 200,
					WidthRequest = 240,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				})
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				ListViewBase listView = handler.PlatformView;
				await AssertEventually(() => listView.Items.Count == items.Count);

				ScrollViewer scrollViewer = await InvokeOnMainThreadAsync(
					() => listView.GetFirstDescendant<ScrollViewer>());
				if (scrollViewer is null)
					throw new XunitException("CarouselView ScrollViewer was not realized.");

				var counters = new Dictionary<string, double>(StringComparer.Ordinal)
				{
					["maximumCenterError"] = 0,
					["positionsOutsideTolerance"] = 0,
					["positionMismatchCount"] = 0
				};
				var completedOperations = 0;

				DevicePerformanceResult result = await DevicePerformanceMeasurement.MeasureAsync(
					"carouselview-wheel-snap-windows",
					WarmupCount,
					IterationCount,
					async iteration =>
					{
						var target = 4 + (iteration % 8);
						await InvokeOnMainThreadAsync(
							() => carouselView.ScrollTo(target, position: ScrollToPosition.Center, animate: false));
						await WaitForSettledCenter(listView, scrollViewer);

						await InvokeOnMainThreadAsync(() =>
							scrollViewer.ChangeView(
								scrollViewer.HorizontalOffset + 120,
								null,
								null,
								disableAnimation: true));

						double centerError = await WaitForSettledCenter(listView, scrollViewer);
						if (completedOperations >= WarmupCount)
						{
							counters["maximumCenterError"] =
								Math.Max(counters["maximumCenterError"], centerError);
							if (centerError > CenterTolerance)
								counters["positionsOutsideTolerance"]++;

							await InvokeOnMainThreadAsync(() =>
							{
								if (carouselView.Position < 0 ||
									carouselView.Position >= items.Count ||
									!Equals(carouselView.CurrentItem, items[carouselView.Position]))
								{
									counters["positionMismatchCount"]++;
								}
							});
						}
						completedOperations++;
					},
					counters);

				DevicePerformanceReporter.Write(result);
			}, MauiContext, TimeSpan.FromMinutes(2));
		}

		async Task<double> WaitForSettledCenter(ListViewBase listView, ScrollViewer scrollViewer)
		{
			var timeout = System.Diagnostics.Stopwatch.StartNew();
			var stableSamples = 0;
			var previousOffset = double.NaN;
			var lastError = double.PositiveInfinity;

			while (timeout.Elapsed < TimeSpan.FromSeconds(10))
			{
				await Task.Delay(50).ConfigureAwait(false);
				var state = await InvokeOnMainThreadAsync(() =>
				{
					var error = GetClosestCenterError(listView, scrollViewer);
					return (scrollViewer.HorizontalOffset, error);
				});

				stableSamples = Math.Abs(state.HorizontalOffset - previousOffset) < 0.1
					? stableSamples + 1
					: 0;
				previousOffset = state.HorizontalOffset;
				lastError = state.error;
				if (stableSamples >= 3)
					return lastError;
			}

			throw new XunitException("CarouselView did not settle after the native offset change.");
		}

		static double GetClosestCenterError(ListViewBase listView, ScrollViewer scrollViewer)
		{
			var viewportCenter = scrollViewer.ViewportWidth / 2;
			var closest = double.PositiveInfinity;
			for (var index = 0; index < listView.Items.Count; index++)
			{
				if (listView.ContainerFromIndex(index) is not FrameworkElement container)
					continue;

				Point center = container
					.TransformToVisual(scrollViewer)
					.TransformPoint(new Point(container.ActualWidth / 2, container.ActualHeight / 2));
				closest = Math.Min(closest, Math.Abs(center.X - viewportCenter));
			}

			return closest;
		}
	}
}
