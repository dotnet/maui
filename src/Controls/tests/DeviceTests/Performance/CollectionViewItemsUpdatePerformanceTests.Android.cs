using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using Xunit.Sdk;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.PerformanceCollectionViewItemsUpdate)]
	public class CollectionViewItemsUpdatePerformanceTests : ControlsHandlerTestBase
	{
		const int InitialItemCount = 100;
		const int WarmupCount = 2;
		const int IterationCount = 10;

		[Fact(DisplayName = "CollectionView KeepItemsInView update performance")]
		public async Task KeepItemsInViewUpdate()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var items = new ObservableCollection<PerformanceItem>();
			for (int i = 0; i < InitialItemCount; i++)
				items.Add(new PerformanceItem($"Item {i + 1}"));

			var collectionView = new CollectionView
			{
				HeightRequest = 700,
				WidthRequest = 390,
				ItemsSource = items,
				ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HeightRequest = 60,
						Padding = new Thickness(12),
						VerticalTextAlignment = TextAlignment.Center
					};
					label.SetBinding(Label.TextProperty, nameof(PerformanceItem.Name));
					return label;
				})
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				RecyclerView recyclerView = handler.PlatformView;
				var counters = new Dictionary<string, double>(StringComparer.Ordinal)
				{
					["initialItemCount"] = InitialItemCount,
					["itemHeight"] = 60,
					["updatesPreservingFirstVisibleItem"] = 0,
					["lastExpectedFirstVisiblePosition"] = -1,
					["lastFirstVisiblePosition"] = -1
				};

				await AssertEventually(() => recyclerView.GetAdapter()?.ItemCount == InitialItemCount);

				DevicePerformanceResult result = await DevicePerformanceMeasurement.MeasureAsync(
					"collectionview-keepitemsinview-update",
					WarmupCount,
					IterationCount,
					async iteration =>
					{
						await InvokeOnMainThreadAsync(() =>
							collectionView.ScrollTo(index: 50, position: ScrollToPosition.Start, animate: false));
						int firstVisiblePositionBefore = await WaitForRecyclerViewIdle(
							recyclerView,
							expectedFirstVisiblePosition: 50);
						int expectedFirstVisiblePosition = firstVisiblePositionBefore + 1;

						await InvokeOnMainThreadAsync(() =>
							items.Insert(0, new PerformanceItem($"Inserted {iteration + 1}")));
						int firstVisiblePosition = await WaitForRecyclerViewIdle(recyclerView);
						counters["lastExpectedFirstVisiblePosition"] = expectedFirstVisiblePosition;
						counters["lastFirstVisiblePosition"] = firstVisiblePosition;
						if (firstVisiblePosition == expectedFirstVisiblePosition)
							counters["updatesPreservingFirstVisibleItem"]++;
					},
					counters);

				DevicePerformanceReporter.Write(result);
			}, MauiContext, TimeSpan.FromMinutes(2));
		}

		async Task<int> WaitForRecyclerViewIdle(RecyclerView recyclerView, int? expectedFirstVisiblePosition = null)
		{
			var timeout = System.Diagnostics.Stopwatch.StartNew();
			int stableSamples = 0;

			while (timeout.Elapsed < TimeSpan.FromSeconds(10))
			{
				await Task.Delay(50).ConfigureAwait(false);

				var state = await InvokeOnMainThreadAsync(() =>
				{
					var layoutManager = recyclerView.GetLayoutManager() as LinearLayoutManager;
					return new
					{
						FirstVisiblePosition = layoutManager?.FindFirstVisibleItemPosition() ?? -1,
						IsIdle = recyclerView.ScrollState == RecyclerView.ScrollStateIdle
					};
				});

				bool positionMatches = !expectedFirstVisiblePosition.HasValue
					|| state.FirstVisiblePosition == expectedFirstVisiblePosition.Value;
				stableSamples = state.IsIdle && positionMatches
					? stableSamples + 1
					: 0;

				if (stableSamples >= 3)
					return state.FirstVisiblePosition;
			}

			throw new XunitException(
				expectedFirstVisiblePosition.HasValue
					? $"RecyclerView did not settle at position {expectedFirstVisiblePosition.Value}."
					: "RecyclerView did not settle.");
		}

		sealed class PerformanceItem
		{
			public PerformanceItem(string name)
			{
				Name = name;
			}

			public string Name { get; }
		}
	}
}
