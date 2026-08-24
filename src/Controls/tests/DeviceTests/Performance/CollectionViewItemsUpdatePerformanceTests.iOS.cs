using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
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
				UICollectionView platformView = handler.PlatformView as UICollectionView
					?? handler.PlatformView.FindDescendantView<UICollectionView>()
					?? throw new XunitException("Could not locate the native UICollectionView.");
				var counters = new Dictionary<string, double>(StringComparer.Ordinal)
				{
					["initialItemCount"] = InitialItemCount,
					["itemHeight"] = 60,
					["updatesPreservingFirstVisibleItem"] = 0,
					["lastExpectedFirstVisiblePosition"] = -1,
					["lastFirstVisiblePosition"] = -1
				};

				await AssertEventually(() => platformView.NumberOfItemsInSection(0) == InitialItemCount);

				DevicePerformanceResult result = await DevicePerformanceMeasurement.MeasureAsync(
					"collectionview-keepitemsinview-update",
					WarmupCount,
					IterationCount,
					async iteration =>
					{
						await InvokeOnMainThreadAsync(() =>
							collectionView.ScrollTo(index: 50, position: ScrollToPosition.Start, animate: false));
						int firstVisiblePositionBefore = await WaitForCollectionViewToSettle(
							platformView,
							expectedVisiblePosition: 50);
						int expectedFirstVisiblePosition = firstVisiblePositionBefore + 1;

						await InvokeOnMainThreadAsync(() =>
							items.Insert(0, new PerformanceItem($"Inserted {iteration + 1}")));
						int firstVisiblePosition = await WaitForCollectionViewToSettle(platformView);
						counters["lastExpectedFirstVisiblePosition"] = expectedFirstVisiblePosition;
						counters["lastFirstVisiblePosition"] = firstVisiblePosition;
						if (firstVisiblePosition == expectedFirstVisiblePosition)
							counters["updatesPreservingFirstVisibleItem"]++;
					},
					counters);

				DevicePerformanceReporter.Write(result);
			}, MauiContext, TimeSpan.FromMinutes(2));
		}

		async Task<int> WaitForCollectionViewToSettle(
			UICollectionView collectionView,
			int? expectedVisiblePosition = null)
		{
			var timeout = System.Diagnostics.Stopwatch.StartNew();
			double previousOffset = double.NaN;
			int stableSamples = 0;

			while (timeout.Elapsed < TimeSpan.FromSeconds(10))
			{
				await Task.Delay(50).ConfigureAwait(false);

				var state = await InvokeOnMainThreadAsync(() =>
				{
					int[] visiblePositions = collectionView.IndexPathsForVisibleItems
						.Where(indexPath => indexPath.Section == 0)
						.Select(indexPath => (int)indexPath.Item)
						.ToArray();
					return new
					{
						Offset = (double)collectionView.ContentOffset.Y,
						IsMoving = collectionView.Dragging || collectionView.Decelerating || collectionView.Tracking,
						FirstVisiblePosition = visiblePositions.Length == 0 ? -1 : visiblePositions.Min(),
						ExpectedItemVisible = !expectedVisiblePosition.HasValue
							|| visiblePositions.Contains(expectedVisiblePosition.Value)
					};
				});

				bool offsetStable = !double.IsNaN(previousOffset)
					&& Math.Abs(state.Offset - previousOffset) < 0.5;
				stableSamples = offsetStable && !state.IsMoving && state.ExpectedItemVisible
					? stableSamples + 1
					: 0;
				previousOffset = state.Offset;

				if (stableSamples >= 3)
					return state.FirstVisiblePosition;
			}

			throw new XunitException(
				expectedVisiblePosition.HasValue
					? $"CollectionView did not settle with item {expectedVisiblePosition.Value} visible."
					: "CollectionView did not settle.");
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
