using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.PerformanceCarouselViewSwipe)]
	public class CarouselViewSwipePerformanceTests : ControlsHandlerTestBase
	{
		const int EventsPerIteration = 100;
		const int WarmupCount = 2;
		const int IterationCount = 10;

		[Fact(DisplayName = "CarouselView disabled-swipe touch performance")]
		public async Task DisabledSwipeTouchHandling()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CarouselView, CarouselViewHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var carouselView = new CarouselView
			{
				HeightRequest = 300,
				WidthRequest = 390,
				IsSwipeEnabled = false,
				Loop = false,
				ItemsSource = CreateItems(),
				ItemTemplate = new DataTemplate(() => new Label
				{
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				})
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				if (handler.PlatformView is not MauiCarouselRecyclerView platformView)
					throw new XunitException("Expected a MauiCarouselRecyclerView.");

				await platformView.WaitForLayoutOrNonZeroSize();
				var counters = new Dictionary<string, double>(StringComparer.Ordinal)
				{
					["eventsPerIteration"] = EventsPerIteration,
					["interceptedTouchEventCount"] = 0,
					["finalPosition"] = -1
				};
				int completedOperations = 0;

				DevicePerformanceResult result = await DevicePerformanceMeasurement.MeasureAsync(
					"carouselview-swipe-disabled",
					WarmupCount,
					IterationCount,
					async _ =>
					{
						int interceptedEvents = await InvokeOnMainThreadAsync(() =>
							MeasureDisabledSwipeInterception(platformView));
						if (completedOperations >= WarmupCount)
							counters["interceptedTouchEventCount"] += interceptedEvents;
						completedOperations++;
					},
					counters);

				counters["finalPosition"] = await InvokeOnMainThreadAsync(() =>
				{
					if (platformView.GetLayoutManager() is not LinearLayoutManager layoutManager)
						return -1;

					return layoutManager.FindFirstCompletelyVisibleItemPosition();
				});
				result.Counters["interceptedTouchEventCount"] = counters["interceptedTouchEventCount"];
				result.Counters["finalPosition"] = counters["finalPosition"];

				DevicePerformanceReporter.Write(result);
			}, MauiContext, TimeSpan.FromMinutes(2));
		}

		static int MeasureDisabledSwipeInterception(MauiCarouselRecyclerView platformView)
		{
			long downTime = SystemClock.UptimeMillis();
			float startX = platformView.Width * 0.75f;
			float endX = platformView.Width * 0.25f;
			float y = platformView.Height * 0.5f;
			int interceptedEvents = 0;

			for (int index = 0; index < EventsPerIteration; index++)
			{
				long eventTime = downTime + index * 3;
				using var down = MotionEvent.Obtain(
					downTime,
					eventTime,
					MotionEventActions.Down,
					startX,
					y,
					MetaKeyStates.None);
				using var move = MotionEvent.Obtain(
					downTime,
					eventTime + 1,
					MotionEventActions.Move,
					endX,
					y,
					MetaKeyStates.None);
				using var up = MotionEvent.Obtain(
					downTime,
					eventTime + 2,
					MotionEventActions.Up,
					endX,
					y,
					MetaKeyStates.None);

				if (platformView.OnInterceptTouchEvent(down))
					interceptedEvents++;
				if (platformView.OnInterceptTouchEvent(move))
					interceptedEvents++;
				if (platformView.OnInterceptTouchEvent(up))
					interceptedEvents++;
			}

			return interceptedEvents;
		}

		static List<string> CreateItems()
		{
			var items = new List<string>(20);
			for (int index = 0; index < 20; index++)
				items.Add($"Item {index + 1}");
			return items;
		}
	}
}
