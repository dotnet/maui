using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
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
	[Category(TestCategory.PerformanceCollectionViewScroll)]
	public class CollectionViewScrollPerformanceTests : ControlsHandlerTestBase
	{
		const int GroupCount = 5;
		const int ItemsPerGroup = 20;
		const int WarmupCount = 2;
		const int IterationCount = 10;
		const int TargetSection = 1;
		const int TargetItem = 2;
		const double PositionTolerance = 30;

		[Fact(DisplayName = "Grouped CollectionView2 ScrollTo MakeVisible performance")]
		public async Task GroupedScrollToMakeVisible()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var groups = CreateGroups();
			var collectionView = new CollectionView
			{
				HeightRequest = 700,
				WidthRequest = 390,
				IsGrouped = true,
				ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
				ItemsSource = groups,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HeightRequest = 80,
						Padding = new Thickness(12),
						VerticalTextAlignment = TextAlignment.Center
					};
					label.SetBinding(Label.TextProperty, nameof(PerformanceItem.Name));
					return label;
				}),
				GroupHeaderTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HeightRequest = 32,
						FontAttributes = FontAttributes.Bold
					};
					label.SetBinding(Label.TextProperty, nameof(PerformanceGroup.Name));
					return label;
				})
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
			{
				UICollectionView platformView = handler.Controller.CollectionView;
				var measuredTargetPositions = new List<double>(IterationCount);
				int completedOperations = 0;

				await AssertEventually(
					() => platformView.NumberOfSections() == GroupCount
						&& platformView.NumberOfItemsInSection(GroupCount - 1) == ItemsPerGroup);

				PerformanceGroup targetGroup = groups[TargetSection];
				PerformanceItem targetItem = targetGroup[TargetItem];

				DevicePerformanceResult result = await DevicePerformanceMeasurement.MeasureAsync(
					"collectionview-grouped-scrollto-makevisible",
					WarmupCount,
					IterationCount,
					async _ =>
					{
						await InvokeOnMainThreadAsync(() =>
						{
							var topOffset = new CoreGraphics.CGPoint(
								-platformView.AdjustedContentInset.Left,
								-platformView.AdjustedContentInset.Top);
							platformView.SetContentOffset(topOffset, animated: false);
						});
						await WaitForScrollToSettle(platformView);

						await InvokeOnMainThreadAsync(() =>
							collectionView.ScrollTo(
								targetItem,
								targetGroup,
								ScrollToPosition.MakeVisible,
								animate: true));

						double targetPosition = await WaitForScrollToSettle(
							platformView,
							TargetItem,
							TargetSection);
						if (completedOperations >= WarmupCount)
							measuredTargetPositions.Add(targetPosition);
						completedOperations++;
					},
					new Dictionary<string, double>(StringComparer.Ordinal)
					{
						["groupCount"] = GroupCount,
						["itemsPerGroup"] = ItemsPerGroup,
						["itemHeight"] = 80,
						["targetSection"] = TargetSection,
						["targetItem"] = TargetItem
					});

				if (measuredTargetPositions.Count != IterationCount)
					throw new XunitException($"Expected {IterationCount} target positions, found {measuredTargetPositions.Count}.");

				double baselinePosition = measuredTargetPositions[0];
				result.Counters["targetPositionMinimum"] = measuredTargetPositions.Min();
				result.Counters["targetPositionMaximum"] = measuredTargetPositions.Max();
				result.Counters["targetPositionSpread"] =
					result.Counters["targetPositionMaximum"] - result.Counters["targetPositionMinimum"];
				result.Counters["positionsOutsideTolerance"] = measuredTargetPositions.Count(
					position => Math.Abs(position - baselinePosition) > PositionTolerance);

				DevicePerformanceReporter.Write(result);
			}, MauiContext, TimeSpan.FromMinutes(2));
		}

		async Task<double> WaitForScrollToSettle(
			UICollectionView collectionView,
			int? expectedItem = null,
			int? expectedSection = null)
		{
			var timeout = System.Diagnostics.Stopwatch.StartNew();
			double previousX = double.NaN;
			double previousY = double.NaN;
			int stableSamples = 0;

			while (timeout.Elapsed < TimeSpan.FromSeconds(10))
			{
				await Task.Delay(50).ConfigureAwait(false);

				var state = await InvokeOnMainThreadAsync(() =>
				{
					var offset = collectionView.ContentOffset;
					using NSIndexPath targetIndexPath = expectedItem is null
						? null
						: NSIndexPath.FromItemSection(expectedItem.Value, expectedSection.Value);
					var targetAttributes = targetIndexPath is null
						? null
						: collectionView.GetLayoutAttributesForItem(targetIndexPath);
					bool expectedItemVisible = targetIndexPath is null
						|| targetAttributes is not null
						&& collectionView.IndexPathsForVisibleItems.Any(indexPath =>
							indexPath.Section == expectedSection.Value
							&& indexPath.Item == expectedItem.Value);

					return new
					{
						X = (double)offset.X,
						Y = (double)offset.Y,
						IsMoving = collectionView.Dragging || collectionView.Decelerating || collectionView.Tracking,
						ExpectedItemVisible = expectedItemVisible,
						TargetPosition = targetAttributes is null
							? double.NaN
							: (double)(targetAttributes.Frame.Y - offset.Y)
					};
				});

				bool offsetStable = !double.IsNaN(previousX)
					&& Math.Abs(state.X - previousX) < 0.5
					&& Math.Abs(state.Y - previousY) < 0.5;

				stableSamples = offsetStable && !state.IsMoving ? stableSamples + 1 : 0;
				previousX = state.X;
				previousY = state.Y;

				if (stableSamples >= 3 && state.ExpectedItemVisible)
					return state.TargetPosition;
			}

			throw new XunitException(
				expectedItem is null
					? "CollectionView did not settle."
					: $"CollectionView did not settle with item {expectedItem} in section {expectedSection} visible.");
		}

		static List<PerformanceGroup> CreateGroups()
		{
			var groups = new List<PerformanceGroup>(GroupCount);
			for (int groupIndex = 0; groupIndex < GroupCount; groupIndex++)
			{
				var items = new List<PerformanceItem>(ItemsPerGroup);
				for (int itemIndex = 0; itemIndex < ItemsPerGroup; itemIndex++)
					items.Add(new PerformanceItem($"Group {groupIndex + 1}, Item {itemIndex + 1}"));

				groups.Add(new PerformanceGroup($"Group {groupIndex + 1}", items));
			}

			return groups;
		}

		sealed class PerformanceGroup : List<PerformanceItem>
		{
			public PerformanceGroup(string name, IEnumerable<PerformanceItem> items)
				: base(items)
			{
				Name = name;
			}

			public string Name { get; }
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
