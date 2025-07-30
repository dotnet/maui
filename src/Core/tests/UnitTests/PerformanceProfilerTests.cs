using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Performance;
using Xunit;

namespace Microsoft.Maui.UnitTests
{

	[CollectionDefinition("PerformanceProfilerTests", DisableParallelization = true)]
	[Category(TestCategory.Core)]
	public class PerformanceProfilerTests
	{
		const double DurationTolerance = 0.15;

		const double ShortDelayMs = 50.0;
		const double MediumDelayMs = 100.0;
		const double LongDelayMs = 150.0;

		void ResetLayout()
		{
			var layoutProperty = typeof(PerformanceProfiler).GetProperty(
				"Layout",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			layoutProperty?.SetValue(null, null);

			Thread.Sleep(50);
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		async Task<bool> RetryUntilConditionAsync(Func<bool> condition, string errorMessage = null)
		{
			const int maxRetries = 20; // 20 retries * 50ms = 1 second
			const int retryDelayMs = 50;

			for (int i = 0; i < maxRetries; i++)
			{
				if (condition())
					return true;
				await Task.Delay(retryDelayMs);
			}

			if (!string.IsNullOrEmpty(errorMessage))
				Assert.Fail(errorMessage);
			return false;
		}

		[Fact]
		public void Initialize_WithNull_ThrowsArgumentNullException()
		{
			ResetLayout();

			var ex = Assert.Throws<ArgumentNullException>(() => PerformanceProfiler.Initialize(null));
			Assert.Equal("layout", ex.ParamName);
		}

		[Fact]
		public void GetStats_NotInitialized_ReturnsEmptyDurations()
		{
			ResetLayout();

			var stats = PerformanceProfiler.GetStats();

			Assert.Null(stats.Layout);
		}

		[Fact]
		public void Start_NotInitialized_DoesNotThrow()
		{
			ResetLayout();

			var tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Test");
			tracker.Stop();
		}

		[Fact]
		public void Start_NotInitialized_DoesNotRecord()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();

			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Test");
			perfTracker.Stop();

			Assert.Equal(0, tracker.MeasureCallCount);
			Assert.Equal(0, tracker.ArrangeCallCount);
		}

		[Fact]
		public async Task Start_LayoutMeasure_AfterInitialization_RecordsDurationAndElement()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = ShortDelayMs;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ElementA");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			perfTracker.Stop();

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => !string.IsNullOrEmpty(tracker.MeasuredElement?.ToString()) &&
				      tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredElement: ElementA, MeasuredDuration ≥ {minDuration:F2}ms");

			Assert.Equal("ElementA", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms (with {DurationTolerance:P} tolerance), " +
				$"got {tracker.MeasuredDuration:F2}ms. Delay was {expectedDelayMs}ms.");
		}

		[Fact]
		public async Task Start_LayoutArrange_AfterInitialization_RecordsDurationAndElement()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = ShortDelayMs;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "ElementB");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			perfTracker.Stop();

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => !string.IsNullOrEmpty(tracker.ArrangedElement?.ToString()) &&
				      tracker.ArrangedDuration >= minDuration,
				$"Expected ArrangedElement: ElementB, ArrangedDuration ≥ {minDuration:F2}ms");

			Assert.Equal("ElementB", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration >= minDuration,
				$"Expected ArrangedDuration ≥ {minDuration:F2}ms (with {DurationTolerance:P} tolerance), " +
				$"got {tracker.ArrangedDuration:F2}ms");
		}

		[Fact]
		public async Task GetStats_AfterTracking_ReturnsLatestDurations()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = ShortDelayMs;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			var measureTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "M");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			measureTracker.Stop();
			var statsAfterMeasure = PerformanceProfiler.GetStats();

			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "A");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			arrangeTracker.Stop();

			await tracker.WaitForUpdatesAsync();
			var statsAfterArrange = PerformanceProfiler.GetStats();

			await RetryUntilConditionAsync(
				() => statsAfterMeasure.Layout is not null &&
				      statsAfterMeasure.Layout.MeasureDuration >= minDuration &&
				      statsAfterArrange.Layout is not null &&
				      statsAfterArrange.Layout.ArrangeDuration >= minDuration &&
				      statsAfterArrange.Layout.MeasureDuration >= minDuration,
				$"Expected MeasureDuration ≥ {minDuration:F2}ms, ArrangeDuration ≥ {minDuration:F2}ms");

			Assert.NotNull(statsAfterMeasure.Layout);
			Assert.True(statsAfterMeasure.Layout.MeasureDuration >= minDuration,
				$"Expected MeasureDuration ≥ {minDuration:F2}ms, got {statsAfterMeasure.Layout.MeasureDuration:F2}ms");
			Assert.Equal(0, statsAfterMeasure.Layout.ArrangeDuration);

			Assert.NotNull(statsAfterArrange.Layout);
			Assert.True(statsAfterArrange.Layout.ArrangeDuration >= minDuration,
				$"Expected ArrangeDuration ≥ {minDuration:F2}ms, got {statsAfterArrange.Layout.ArrangeDuration:F2}ms");
			Assert.True(statsAfterArrange.Layout.MeasureDuration >= minDuration,
				$"Expected MeasureDuration still present, got {statsAfterArrange.Layout.MeasureDuration:F2}ms");
		}

		[Fact]
		public void Start_UnknownPerformanceType_DoesNotRecord()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const PerformanceCategory unknownType = (PerformanceCategory)99;

			var perfTracker = PerformanceProfiler.Start(unknownType, "TestElement");
			perfTracker.Stop();

			Assert.Equal(0, tracker.MeasureCallCount);
			Assert.Equal(0, tracker.ArrangeCallCount);
		}

		[Fact]
		public async Task Start_MultipleTrackers_RecordsEachPass()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = ShortDelayMs;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			var tracker1 = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			tracker1.Stop();

			var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			tracker2.Stop();

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => !string.IsNullOrEmpty(tracker.MeasuredElement?.ToString()) &&
				      !string.IsNullOrEmpty(tracker.ArrangedElement?.ToString()) &&
				      tracker.MeasuredDuration >= minDuration &&
				      tracker.ArrangedDuration >= minDuration,
				$"Expected MeasuredElement: Element1, ArrangedElement: Element2, " +
				$"MeasuredDuration ≥ {minDuration:F2}ms, ArrangedDuration ≥ {minDuration:F2}ms");

			Assert.Equal("Element1", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms, got {tracker.MeasuredDuration:F2}ms");

			Assert.Equal("Element2", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration >= minDuration,
				$"Expected ArrangedDuration ≥ {minDuration:F2}ms, got {tracker.ArrangedDuration:F2}ms");
		}

		[Fact]
		public async Task UsingVar_LayoutMeasure_AutomaticallyStops()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = ShortDelayMs;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			{
				using var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "UsingElement");
				await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			}

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => !string.IsNullOrEmpty(tracker.MeasuredElement?.ToString()) &&
				      tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredElement: UsingElement, MeasuredDuration ≥ {minDuration:F2}ms");

			Assert.Equal("UsingElement", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms, got {tracker.MeasuredDuration:F2}ms");
		}

		[Fact]
		public async Task UsingVar_LayoutArrange_AutomaticallyStops()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = ShortDelayMs;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			{
				using var perfTracker =
					PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "UsingArrangeElement");
				await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			}

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => !string.IsNullOrEmpty(tracker.ArrangedElement?.ToString()) &&
				      tracker.ArrangedDuration >= minDuration,
				$"Expected ArrangedElement: UsingArrangeElement, ArrangedDuration ≥ {minDuration:F2}ms");

			Assert.Equal("UsingArrangeElement", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration >= minDuration,
				$"Expected ArrangedDuration ≥ {minDuration:F2}ms, got {tracker.ArrangedDuration:F2}ms");
		}

		[Fact]
		public async Task UsingVar_NotInitialized_DoesNotThrow()
		{
			ResetLayout();

			using var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Test");
			await Task.Delay((int)ShortDelayMs);
		}

		[Fact]
		public async Task UsingVar_WithException_StillRecordsBeforeException()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = ShortDelayMs;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			try
			{
				using var perfTracker =
					PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ExceptionElement");
				await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
				throw new InvalidOperationException("Intentional test exception");
			}
			catch (InvalidOperationException)
			{
			}

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms");

			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms, got {tracker.MeasuredDuration:F2}ms");
		}

		[Fact]
		public async Task UsingVar_MultipleNestedScopes_RecordsEachCorrectly()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			{
				using var outerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Outer");
				await Task.Delay((int)ShortDelayMs);
			}
			{
				using var innerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Inner");
				await Task.Delay((int)ShortDelayMs);
			}

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => tracker.MeasuredElement?.ToString() == "Outer" && tracker.ArrangedElement?.ToString() == "Inner",
				$"Expected MeasuredElement: Outer, ArrangedElement: Inner");

			Assert.Equal("Outer", tracker.MeasuredElement);
			Assert.Equal("Inner", tracker.ArrangedElement);
		}

		[Fact]
		public async Task UsingVar_UnknownPerformanceType_DoesNotRecord()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			{
				using var perfTracker = PerformanceProfiler.Start((PerformanceCategory)99, "UnknownElement");
				await Task.Delay((int)ShortDelayMs);
			}

			await tracker.WaitForUpdatesAsync();
			Assert.Equal(0, tracker.MeasureCallCount);
			Assert.Equal(0, tracker.ArrangeCallCount);
		}

		[Fact]
		public void SubscribeToUpdates_NullCallback_ThrowsArgumentNullException()
		{
			ResetLayout();

			var ex = Assert.Throws<ArgumentNullException>(() => PerformanceProfiler.SubscribeToUpdates(null));
			Assert.Equal("callback", ex.ParamName);
		}

		[Fact]
		public void SubscribeToUpdates_NotInitialized_DoesNotThrow()
		{
			ResetLayout();

			var exception = Record.Exception(() =>
			{
				PerformanceProfiler.SubscribeToUpdates(update =>
				{
				});
			});
			Assert.Null(exception);
		}

		[Fact]
		public async Task SubscribeToUpdates_ReceivesLayoutUpdateEvents()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			var receivedUpdates = new ConcurrentBag<LayoutUpdate>();
			Action<LayoutUpdate> callback = update => receivedUpdates.Add(update);

			PerformanceProfiler.SubscribeToUpdates(callback);
			var measureTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			await Task.Delay((int)ShortDelayMs);
			measureTracker.Stop();
			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			await Task.Delay((int)ShortDelayMs);
			arrangeTracker.Stop();

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => receivedUpdates.Count >= 2,
				$"Expected at least 2 updates, got {receivedUpdates.Count}");

			var updates = receivedUpdates.ToArray();
			Assert.Equal(2, updates.Length);

			var measureUpdate = updates.FirstOrDefault(u => u.PassType == LayoutPassType.Measure);
			var arrangeUpdate = updates.FirstOrDefault(u => u.PassType == LayoutPassType.Arrange);

			Assert.NotNull(measureUpdate);
			Assert.NotNull(arrangeUpdate);
			Assert.Equal("Element1", measureUpdate.Element);
			Assert.Equal("Element2", arrangeUpdate.Element);
		}

		[Fact]
		public async Task SubscribeToUpdates_MultipleSubscribers_AllReceiveEvents()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			var subscriber1Updates = new List<LayoutUpdate>();
			var subscriber2Updates = new List<LayoutUpdate>();
			Action<LayoutUpdate> callback1 = update =>
			{
				lock (subscriber1Updates) { subscriber1Updates.Add(update); }
			};
			Action<LayoutUpdate> callback2 = update =>
			{
				lock (subscriber2Updates) { subscriber2Updates.Add(update); }
			};

			PerformanceProfiler.SubscribeToUpdates(callback1);
			PerformanceProfiler.SubscribeToUpdates(callback2);
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "MultiSubscriber");
			await Task.Delay((int)ShortDelayMs);
			perfTracker.Stop();

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => subscriber1Updates.Count >= 1 && subscriber2Updates.Count >= 1,
				$"Expected at least 1 update for each subscriber, got {subscriber1Updates.Count} and {subscriber2Updates.Count}");

			Assert.Single(subscriber1Updates);
			Assert.Single(subscriber2Updates);
			Assert.Equal("MultiSubscriber", subscriber1Updates[0].Element);
			Assert.Equal("MultiSubscriber", subscriber2Updates[0].Element);
			Assert.Equal(subscriber1Updates[0].TotalTime, subscriber2Updates[0].TotalTime);
			Assert.True(subscriber1Updates[0].TotalTime > 0);
		}

		[Fact]
		public async Task SubscribeToUpdates_SubscriberThrowsException_DoesNotAffectOtherSubscribers()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			var goodSubscriberUpdates = new List<LayoutUpdate>();
			Action<LayoutUpdate> badSubscriber = _ => throw new InvalidOperationException("Bad subscriber");
			Action<LayoutUpdate> goodSubscriber = update =>
			{
				lock (goodSubscriberUpdates) { goodSubscriberUpdates.Add(update); }
			};

			PerformanceProfiler.SubscribeToUpdates(badSubscriber);
			PerformanceProfiler.SubscribeToUpdates(goodSubscriber);
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "TestElement");
			await Task.Delay((int)ShortDelayMs);
			perfTracker.Stop();

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => goodSubscriberUpdates.Count >= 1,
				$"Expected at least 1 update for good subscriber, got {goodSubscriberUpdates.Count}");

			Assert.Single(goodSubscriberUpdates);
			Assert.Equal("TestElement", goodSubscriberUpdates[0].Element);
		}

		[Fact]
		public void GetHistory_NotInitialized_ReturnsEmpty()
		{
			ResetLayout();

			var history = PerformanceProfiler.GetHistory();

			Assert.Null(history.Layout);
			Assert.True((DateTime.UtcNow - history.TimestampUtc).TotalSeconds < 5,
				$"Timestamp too old: {history.TimestampUtc}");
		}

		[Fact]
		public async Task GetHistory_WithElement_FiltersCorrectly()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			var tracker1 = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			await Task.Delay((int)ShortDelayMs);
			tracker1.Stop();
			var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			await Task.Delay((int)ShortDelayMs);
			tracker2.Stop();

			await tracker.WaitForUpdatesAsync();

			IEnumerable<LayoutUpdate> filteredHistory = null;
			await RetryUntilConditionAsync(
				() =>
				{
					var history = PerformanceProfiler.GetHistory("Element1");
					if (history.Layout is not null)
					{
						filteredHistory = history.Layout;
						return filteredHistory.Any();
					}

					return false;
				},
				$"Expected at least one update for Element1 in history");

			var layoutHistory = new List<LayoutUpdate>(filteredHistory ?? Enumerable.Empty<LayoutUpdate>());
			Assert.Single(layoutHistory);
			Assert.Equal("Element1", layoutHistory[0].Element);
			Assert.Equal(LayoutPassType.Measure, layoutHistory[0].PassType);
		}

		[Fact]
		public async Task GetHistory_WithoutElement_ReturnsAllUpdates()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			{
				var tracker1 = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
				await Task.Delay((int)ShortDelayMs);
				tracker1.Stop();
			}
			{
				var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
				await Task.Delay((int)ShortDelayMs);
				tracker2.Stop();
			}

			await tracker.WaitForUpdatesAsync();

			List<LayoutUpdate> layoutHistory = null;
			await RetryUntilConditionAsync(
				() =>
				{
					var history = PerformanceProfiler.GetHistory();
					if (history.Layout is not null)
					{
						layoutHistory = new List<LayoutUpdate>(history.Layout);
						return layoutHistory.Count >= 2;
					}

					return false;
				},
				$"Expected at least 2 updates in history, got {layoutHistory?.Count ?? 0}");

			Assert.NotNull(layoutHistory);
			Assert.Equal(2, layoutHistory.Count);
			Assert.Contains(layoutHistory,
				u => u.Element.Equals("Element1") && u.PassType == LayoutPassType.Measure);
			Assert.Contains(layoutHistory,
				u => u.Element.Equals("Element2") && u.PassType == LayoutPassType.Arrange);
		}

		[Fact]
		public async Task RecordMeasurePass_ZeroDuration_RecordsCorrectly()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			{
				var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ZeroDuration");
				perfTracker.Stop();
			}

			await tracker.WaitForUpdatesAsync();

			await RetryUntilConditionAsync(
				() => tracker.MeasuredElement?.ToString() == "ZeroDuration" && tracker.MeasuredDuration > 0,
				$"Expected MeasuredElement: ZeroDuration, small positive duration");

			Assert.Equal("ZeroDuration", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration > 0,
				$"Expected small positive duration due to Stopwatch overhead, got {tracker.MeasuredDuration:F4}ms");
		}

		[Fact]
		public async Task RecordArrangePass_NullElement_RecordsEmptyString()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = ShortDelayMs;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange);
			await Task.Delay((int)ShortDelayMs);
			perfTracker.Stop();
			await tracker.WaitForUpdatesAsync();

			var history = PerformanceProfiler.GetHistory();
			var layoutHistory = new List<LayoutUpdate>(history.Layout);

			await RetryUntilConditionAsync(
				() => layoutHistory.Count == 1 && string.Equals(layoutHistory[0].Element?.ToString(), string.Empty, StringComparison.OrdinalIgnoreCase) &&
				      layoutHistory[0].PassType == LayoutPassType.Arrange && layoutHistory[0].TotalTime >= minDuration,
				$"Expected single update with empty Element, Arrange type, TotalTime ≥ {minDuration:F2}ms");

			Assert.Single(layoutHistory);
			Assert.Equal(string.Empty, layoutHistory[0].Element);
			Assert.Equal(LayoutPassType.Arrange, layoutHistory[0].PassType);
			Assert.True(layoutHistory[0].TotalTime >= minDuration,
				$"Expected TotalTime ≥ {minDuration:F2}ms, got {layoutHistory[0].TotalTime:F2}ms");
		}

		[Fact]
		public async Task RecordMeasurePass_VerticalStackLayoutWithLabel_ReportsStandaloneDuration()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			var parent = new MockVisualTreeElement(true);
			var child = new MockVisualTreeElement(false) { Parent = parent };
			parent.Children.Add(child);

			var childTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, child);
			await Task.Delay((int)LongDelayMs);
			childTracker.Stop();

			var parentTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, parent);
			await Task.Delay((int)MediumDelayMs);
			parentTracker.Stop();

			await tracker.WaitForUpdatesAsync();

			var stats = PerformanceProfiler.GetStats();
			var layoutHistory = new List<LayoutUpdate>(PerformanceProfiler.GetHistory().Layout);

			double parentMin = MediumDelayMs * (1 - DurationTolerance);
			double childMin = LongDelayMs * (1 - DurationTolerance);

			await RetryUntilConditionAsync(
				() => stats.Layout is not null && stats.Layout.MeasureDuration > 0.0 &&
				      layoutHistory.Count == 2 &&
				      layoutHistory.Any(u =>
					      u.Element == parent && u.PassType == LayoutPassType.Measure && u.TotalTime >= parentMin) &&
				      layoutHistory.Any(u =>
					      u.Element == child && u.PassType == LayoutPassType.Measure && u.TotalTime >= childMin),
				$"Expected 2 history updates, parent TotalTime ≥ {parentMin:F2}ms, child TotalTime ≥ {childMin:F2}ms");

			Assert.NotNull(stats.Layout);
			Assert.True(stats.Layout.MeasureDuration > 0.0,
				$"Expected non-zero MeasureDuration, got {stats.Layout.MeasureDuration:F2}ms");

			Assert.Equal(2, layoutHistory.Count);

			var parentUpdate = layoutHistory.Find(u => u.Element == parent);
			Assert.NotNull(parentUpdate);
			Assert.Equal(LayoutPassType.Measure, parentUpdate.PassType);
			Assert.True(parentUpdate.TotalTime >= parentMin,
				$"Expected parent TotalTime ≥ {parentMin:F2}ms, got {parentUpdate.TotalTime:F2}ms. " +
				$"Parent delay was {MediumDelayMs}ms with {DurationTolerance:P} tolerance.");

			var childUpdate = layoutHistory.Find(u => u.Element == child);
			Assert.NotNull(childUpdate);
			Assert.Equal(LayoutPassType.Measure, childUpdate.PassType);
			Assert.True(childUpdate.TotalTime >= childMin,
				$"Expected child TotalTime ≥ {childMin:F2}ms, got {childUpdate.TotalTime:F2}ms. " +
				$"Child delay was {LongDelayMs}ms with {DurationTolerance:P} tolerance.");
		}

		[Fact]
		public async Task RecordMeasurePass_DeepLayoutHierarchy_TracksEachElementIndependently()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			var root = new MockVisualTreeElement(true);
			var panel = new MockVisualTreeElement(true) { Parent = root };
			var label1 = new MockVisualTreeElement(false) { Parent = panel };
			var label2 = new MockVisualTreeElement(false) { Parent = panel };
			root.Children.Add(panel);
			panel.Children.Add(label1);
			panel.Children.Add(label2);

			var rootTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, root);
			await Task.Delay((int)ShortDelayMs);
			var panelTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, panel);
			await Task.Delay((int)ShortDelayMs);
			var label1Tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, label1);
			await Task.Delay((int)ShortDelayMs);
			label1Tracker.Stop();
			var label2Tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, label2);
			await Task.Delay((int)ShortDelayMs);
			label2Tracker.Stop();
			panelTracker.Stop();
			rootTracker.Stop();
			await tracker.WaitForUpdatesAsync();

			var stats = PerformanceProfiler.GetStats();
			var layoutHistory = new List<LayoutUpdate>(PerformanceProfiler.GetHistory().Layout);

			double minDuration = ShortDelayMs * (1 - DurationTolerance);

			await RetryUntilConditionAsync(
				() => stats.Layout is not null && stats.Layout.MeasureDuration > 0.0 &&
				      layoutHistory.Count == 4 &&
				      layoutHistory.Any(u =>
					      u.Element == root && u.PassType == LayoutPassType.Measure && u.TotalTime >= minDuration) &&
				      layoutHistory.Any(u =>
					      u.Element == panel && u.PassType == LayoutPassType.Measure && u.TotalTime >= minDuration) &&
				      layoutHistory.Any(u =>
					      u.Element == label1 && u.PassType == LayoutPassType.Measure && u.TotalTime >= minDuration) &&
				      layoutHistory.Any(u =>
					      u.Element == label2 && u.PassType == LayoutPassType.Measure && u.TotalTime >= minDuration),
				$"Expected 4 history updates, each TotalTime ≥ {minDuration:F2}ms");

			Assert.NotNull(stats.Layout);
			Assert.True(stats.Layout.MeasureDuration > 0.0,
				$"Expected non-zero MeasureDuration, got {stats.Layout.MeasureDuration:F2}ms");

			Assert.Equal(4, layoutHistory.Count);

			var rootUpdate = layoutHistory.Find(u => u.Element == root);
			Assert.NotNull(rootUpdate);
			Assert.Equal(LayoutPassType.Measure, rootUpdate.PassType);
			Assert.True(rootUpdate.TotalTime >= minDuration,
				$"Expected Root ≥ {minDuration:F2}ms, got {rootUpdate.TotalTime:F2}ms");

			var panelUpdate = layoutHistory.Find(u => u.Element == panel);
			Assert.NotNull(panelUpdate);
			Assert.Equal(LayoutPassType.Measure, panelUpdate.PassType);
			Assert.True(panelUpdate.TotalTime >= minDuration,
				$"Expected Panel ≥ {minDuration:F2}ms, got {panelUpdate.TotalTime:F2}ms");

			var label1Update = layoutHistory.Find(u => u.Element == label1);
			Assert.NotNull(label1Update);
			Assert.Equal(LayoutPassType.Measure, label1Update.PassType);
			Assert.True(label1Update.TotalTime >= minDuration,
				$"Expected Label1 ≥ {minDuration:F2}ms, got {label1Update.TotalTime:F2}ms");

			var label2Update = layoutHistory.Find(u => u.Element == label2);
			Assert.NotNull(label2Update);
			Assert.Equal(LayoutPassType.Measure, label2Update.PassType);
			Assert.True(label2Update.TotalTime >= minDuration,
				$"Expected Label2 ≥ {minDuration:F2}ms, got {label2Update.TotalTime:F2}ms");
		}
	}

	internal class MockVisualTreeElement : IVisualTreeElement
	{
		public bool IsLayout { get; }
		public List<MockVisualTreeElement> Children { get; } = new List<MockVisualTreeElement>();
		public MockVisualTreeElement Parent { get; set; }

		public MockVisualTreeElement(bool isLayout)
		{
			IsLayout = isLayout;
		}

		public IReadOnlyList<IVisualTreeElement> GetVisualChildren()
		{
			return Children.AsReadOnly();
		}

		public IVisualTreeElement GetVisualParent()
		{
			return Parent;
		}
	}

	internal class FakeLayoutTracker : ILayoutPerformanceTracker
	{
		const int TaskCleanupThreshold = 50;

		readonly object _lock = new();
		readonly List<LayoutUpdate> _history = new();
		readonly List<Action<LayoutUpdate>> _subscribers = new();

		readonly ConcurrentDictionary<(object Element, LayoutPassType PassType), double>
			_childDurations = new();

		readonly List<Task> _pendingTasks = new();

		private volatile int _activeOperations = 0;
		private volatile int _pendingUpdates = 0;

		public int MeasureCallCount { get; private set; }
		public double MeasuredDuration { get; private set; }
		public object MeasuredElement { get; private set; }
		public int ArrangeCallCount { get; private set; }
		public double ArrangedDuration { get; private set; }
		public object ArrangedElement { get; private set; }

		public LayoutStats GetStats()
		{
			lock (_lock)
			{
				return new LayoutStats { MeasureDuration = MeasuredDuration, ArrangeDuration = ArrangedDuration };
			}
		}

		public IEnumerable<LayoutUpdate> GetHistory(object element = null)
		{
			lock (_lock)
			{
				return element is null
					? new List<LayoutUpdate>(_history)
					: _history.FindAll(u => Equals(u.Element, element));
			}
		}

		public void SubscribeToLayoutUpdates(Action<LayoutUpdate> callback)
		{
			if (callback is null)
				throw new ArgumentNullException(nameof(callback));

			lock (_lock)
			{
				_subscribers.Add(callback);
			}
		}

		public void RecordMeasurePass(double duration, object element = null)
		{
			ProcessUpdate(LayoutPassType.Measure, duration, element);
		}

		public void RecordArrangePass(double duration, object element = null)
		{
			ProcessUpdate(LayoutPassType.Arrange, duration, element);
		}

		void ProcessUpdate(LayoutPassType passType, double duration, object element)
		{
			Task notificationTask = null;
			LayoutUpdate update;
			Action<LayoutUpdate>[] subscribersSnapshot = null;

			lock (_lock)
			{
				Interlocked.Increment(ref _pendingUpdates);

				double standaloneDuration = duration;

				if (element is not null)
				{
					object parent = element is IVisualTreeElement vteParent ? vteParent.GetVisualParent() : null;

					if (parent is not null)
					{
						_childDurations.AddOrUpdate(
							(parent, passType),
							duration,
							(key, oldValue) => oldValue + duration);
					}

					if (element is ILayout)
					{
						standaloneDuration = _childDurations.TryGetValue((element, passType), out var childTime)
							? duration - childTime
							: duration;
						_childDurations.TryRemove((element, passType), out _);
					}
				}

				if (passType == LayoutPassType.Measure)
				{
					MeasureCallCount++;
					MeasuredDuration = standaloneDuration;
					MeasuredElement = element;
				}
				else
				{
					ArrangeCallCount++;
					ArrangedDuration = standaloneDuration;
					ArrangedElement = element;
				}

				update = new LayoutUpdate(passType, standaloneDuration, element ?? string.Empty, DateTime.UtcNow);
				_history.Add(update);

				if (_subscribers.Count > 0)
				{
					subscribersSnapshot = _subscribers.ToArray();
					Interlocked.Increment(ref _activeOperations);

					notificationTask = new Task(() => PublishLayoutUpdateSafe(update, subscribersSnapshot));
					_pendingTasks.Add(notificationTask);

					if (_pendingTasks.Count > TaskCleanupThreshold)
					{
						_pendingTasks.RemoveAll(t => t.IsCompleted);
					}
				}

				// Always decrement pending updates after processing
				Interlocked.Decrement(ref _pendingUpdates);
			}

			// Start the notification task outside the lock
			notificationTask?.Start();
		}

		void PublishLayoutUpdateSafe(LayoutUpdate update, Action<LayoutUpdate>[] subscribers)
		{
			try
			{
				foreach (var subscriber in subscribers)
				{
					try
					{
						subscriber.Invoke(update);
					}
					catch
					{
						// Swallow subscriber exceptions
					}
				}
			}
			finally
			{
				Interlocked.Decrement(ref _activeOperations);
			}
		}

		public async Task WaitForUpdatesAsync(CancellationToken cancellationToken = default)
		{
			// First, wait for all pending updates to be processed
			var maxWaitTime = TimeSpan.FromSeconds(10);
			var startTime = DateTime.UtcNow;

			while ((_pendingUpdates > 0 || _activeOperations > 0) && (DateTime.UtcNow - startTime) < maxWaitTime)
			{
				await Task.Delay(5, cancellationToken);
			}

			// Then wait for all pending notification tasks
			Task[] tasksToWait;
			lock (_lock)
			{
				tasksToWait = _pendingTasks.Where(t => !t.IsCompleted).ToArray();
			}

			if (tasksToWait.Length > 0)
			{
				try
				{
					var timeoutTask = Task.Delay(maxWaitTime, cancellationToken);
					var completedTask = await Task.WhenAny(Task.WhenAll(tasksToWait), timeoutTask);

					if (completedTask == timeoutTask)
					{
						// Timeout occurred, but don't throw - just continue
					}
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					throw;
				}
				catch
				{
					// Swallow any non-cancellation exceptions from the subscriber notifications
				}
			}

			// Final cleanup
			lock (_lock)
			{
				_pendingTasks.RemoveAll(t => t.IsCompleted);
			}

			// Small final delay to ensure all operations are truly complete
			await Task.Delay(25, cancellationToken);
		}
	}
}