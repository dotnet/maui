using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;
using Xunit.Sdk;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.PerformanceCarouselViewSwipe)]
	public class CarouselViewSwipePerformanceTests : ControlsHandlerTestBase
	{
		const int LayoutsPerIteration = 50;
		const int WarmupCount = 2;
		const int IterationCount = 10;

		[Fact(DisplayName = "CarouselView disabled-swipe layout performance")]
		public async Task DisabledSwipeLayoutStateReapplication()
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
				HeightRequest = 300,
				WidthRequest = 390,
				IsBounceEnabled = false,
				IsSwipeEnabled = false,
				Loop = false,
				ItemsSource = CreateItems(),
				ItemTemplate = new DataTemplate(() => new Label
				{
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				})
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carouselView, async handler =>
			{
				if (handler.Controller.CollectionView is not MauiCollectionView platformView)
					throw new XunitException("Expected a MauiCollectionView.");

				await AssertEventually(() => GetEmbeddedScrollViews(platformView).Length > 0);
				var counters = new Dictionary<string, double>(StringComparer.Ordinal)
				{
					["layoutsPerIteration"] = LayoutsPerIteration,
					["embeddedScrollViewCount"] = 0,
					["stateReapplicationFailures"] = 0
				};
				int completedOperations = 0;

				DevicePerformanceResult result = await DevicePerformanceMeasurement.MeasureAsync(
					"carouselview-swipe-disabled",
					WarmupCount,
					IterationCount,
					async _ =>
					{
						var state = await InvokeOnMainThreadAsync(() =>
							ApplyLayoutAfterNativeStateReset(platformView));
						if (completedOperations >= WarmupCount)
						{
							counters["embeddedScrollViewCount"] = Math.Max(
								counters["embeddedScrollViewCount"],
								state.EmbeddedScrollViewCount);
							counters["stateReapplicationFailures"] += state.Failures;
						}
						completedOperations++;
					},
					counters);

				result.Counters["embeddedScrollViewCount"] = counters["embeddedScrollViewCount"];
				result.Counters["stateReapplicationFailures"] = counters["stateReapplicationFailures"];

				DevicePerformanceReporter.Write(result);
			}, MauiContext, TimeSpan.FromMinutes(2));
		}

		static (int EmbeddedScrollViewCount, int Failures) ApplyLayoutAfterNativeStateReset(
			MauiCollectionView collectionView)
		{
			int failures = 0;
			int embeddedScrollViewCount = 0;

			for (int index = 0; index < LayoutsPerIteration; index++)
			{
				UIScrollView[] embeddedScrollViews = GetEmbeddedScrollViews(collectionView);
				embeddedScrollViewCount = Math.Max(embeddedScrollViewCount, embeddedScrollViews.Length);

				foreach (UIScrollView scrollView in embeddedScrollViews)
				{
					scrollView.ScrollEnabled = true;
					scrollView.Bounces = true;
					scrollView.AlwaysBounceHorizontal = true;
					scrollView.AlwaysBounceVertical = true;
				}

				collectionView.LayoutSubviews();

				foreach (UIScrollView scrollView in GetEmbeddedScrollViews(collectionView))
				{
					if (scrollView.ScrollEnabled
						|| scrollView.Bounces
						|| scrollView.AlwaysBounceHorizontal
						|| scrollView.AlwaysBounceVertical)
					{
						failures++;
					}
				}
			}

			return (embeddedScrollViewCount, failures);
		}

		static UIScrollView[] GetEmbeddedScrollViews(MauiCollectionView collectionView) =>
			collectionView.Subviews
				.OfType<UIScrollView>()
				.Where(view => view is not UICollectionView)
				.ToArray();

		static List<string> CreateItems()
		{
			var items = new List<string>(20);
			for (int index = 0; index < 20; index++)
				items.Add($"Item {index + 1}");
			return items;
		}
	}
}
