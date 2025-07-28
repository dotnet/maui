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
		const double DurationTolerance = 0.1;

		void ResetLayout()
		{
			var layoutProperty = typeof(PerformanceProfiler).GetProperty(
				"Layout",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			layoutProperty?.SetValue(null, null);
		}

		[Fact]
		public void Initialize_WithNull_ThrowsArgumentNullException()
		{
			// Arrange
			ResetLayout();

			// Act & Assert
			var ex = Assert.Throws<ArgumentNullException>(() => PerformanceProfiler.Initialize(null));
			Assert.Equal("layout", ex.ParamName);
		}

		[Fact]
		public void GetStats_NotInitialized_ReturnsEmptyDurations()
		{
			// Arrange
			ResetLayout();

			// Act
			var stats = PerformanceProfiler.GetStats();

			// Assert
			Assert.Null(stats.Layout);
		}

		[Fact]
		public void Start_NotInitialized_DoesNotThrow()
		{
			// Arrange
			ResetLayout();

			// Act & Assert
			var tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Test");
			tracker.Stop(); // Should complete safely
		}

		[Fact]
		public void Start_NotInitialized_DoesNotRecord()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Test");
			perfTracker.Stop();

			// Assert
			Assert.Equal(0, tracker.MeasureCallCount);
			Assert.Equal(0, tracker.ArrangeCallCount);
		}

		[Fact]
		public async Task Start_LayoutMeasure_AfterInitialization_RecordsDurationAndElement()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ElementA");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs)); // Simulate layout work
			perfTracker.Stop();

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("ElementA", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms (with {DurationTolerance:P} tolerance), " +
				$"got {tracker.MeasuredDuration:F2}ms");
		}

		[Fact]
		public async Task Start_LayoutArrange_AfterInitialization_RecordsDurationAndElement()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "ElementB");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs)); // Simulate layout work
			perfTracker.Stop();

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.Equal("ElementB", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration >= minDuration,
				$"Expected ArrangedDuration ≥ {minDuration:F2}ms (with {DurationTolerance:P} tolerance), " +
				$"got {tracker.ArrangedDuration:F2}ms");
		}

		[Fact]
		public async Task GetStats_AfterTracking_ReturnsLatestDurations()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			// Track measure
			var measureTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "M");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs)); // Simulate layout work
			measureTracker.Stop();
			var statsAfterMeasure = PerformanceProfiler.GetStats();

			// Track arrange
			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "A");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs)); // Simulate layout work
			arrangeTracker.Stop();

			await tracker.WaitForUpdatesAsync();
			var statsAfterArrange = PerformanceProfiler.GetStats();

			// Assert measure stats
			Assert.NotNull(statsAfterMeasure.Layout);
			Assert.True(statsAfterMeasure.Layout.MeasureDuration >= minDuration,
				$"Expected MeasureDuration ≥ {minDuration:F2}ms, got {statsAfterMeasure.Layout.MeasureDuration:F2}ms");

			Assert.Equal(0, statsAfterMeasure.Layout.ArrangeDuration);

			// Assert arrange stats
			Assert.NotNull(statsAfterArrange.Layout);
			Assert.True(statsAfterArrange.Layout.ArrangeDuration >= minDuration,
				$"Expected ArrangeDuration ≥ {minDuration:F2}ms, got {statsAfterArrange.Layout.ArrangeDuration:F2}ms");

			Assert.True(statsAfterArrange.Layout.MeasureDuration >= minDuration,
				$"Expected MeasureDuration still present, got {statsAfterArrange.Layout.MeasureDuration:F2}ms");
		}

		[Fact]
		public void Start_UnknownPerformanceType_DoesNotRecord()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const PerformanceCategory unknownType = (PerformanceCategory)99;

			// Act
			var perfTracker = PerformanceProfiler.Start(unknownType, "TestElement");
			perfTracker.Stop();

			// Assert
			Assert.Equal(0, tracker.MeasureCallCount);
			Assert.Equal(0, tracker.ArrangeCallCount);
		}

		[Fact]
		public async Task Start_MultipleTrackers_RecordsEachPass()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			// Act
			var tracker1 = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs)); // Simulate layout work
			tracker1.Stop();

			var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs)); // Simulate layout work
			tracker2.Stop();

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("Element1", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms, got {tracker.MeasuredDuration:F2}ms");

			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.Equal("Element2", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration >= minDuration,
				$"Expected ArrangedDuration ≥ {minDuration:F2}ms, got {tracker.ArrangedDuration:F2}ms");
		}

		[Fact]
		public async Task Start_WithoutElement_RecordsDuration()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure);
			await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs)); // Simulate layout work
			perfTracker.Stop();

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms, got {tracker.MeasuredDuration:F2}ms");
		}

		[Fact]
		public async Task UsingVar_LayoutMeasure_AutomaticallyStops()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			// Act
			{
				using var perfTracker =
					PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "UsingElement");
				await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			}

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("UsingElement", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms, got {tracker.MeasuredDuration:F2}ms");
		}

		[Fact]
		public async Task UsingVar_LayoutArrange_AutomaticallyStops()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			// Act
			{
				using var perfTracker =
					PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "UsingArrangeElement");
				await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			}

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.Equal("UsingArrangeElement", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration >= minDuration,
				$"Expected ArrangedDuration ≥ {minDuration:F2}ms, got {tracker.ArrangedDuration:F2}ms");
		}

		[Fact]
		public async Task UsingVar_NotInitialized_DoesNotThrow()
		{
			// Arrange
			ResetLayout();

			// Act & Assert
			using var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Test");
			await Task.Delay(5);
		}

		[Fact]
		public async Task UsingVar_WithException_StillRecordsBeforeException()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance);

			// Act
			try
			{
				using var perfTracker =
					PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ExceptionElement");
				await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
				throw new InvalidOperationException("Intentional test exception");
			}
			catch (InvalidOperationException)
			{
				// Expected
			}

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration >= minDuration,
				$"Expected MeasuredDuration ≥ {minDuration:F2}ms, got {tracker.MeasuredDuration:F2}ms");
		}

		[Fact]
		public async Task UsingVar_MultipleNestedScopes_RecordsEachCorrectly()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			{
				using var outerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Outer");
				await Task.Delay(5);
			}
			{
				using var innerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Inner");
				await Task.Delay(5);
			}

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("Outer", tracker.MeasuredElement);
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.Equal("Inner", tracker.ArrangedElement);
		}

		[Fact]
		public async Task UsingVar_UnknownPerformanceType_DoesNotRecord()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			{
				using var perfTracker = PerformanceProfiler.Start((PerformanceCategory)99, "UnknownElement");
				await Task.Delay(5);
			}

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(0, tracker.MeasureCallCount);
			Assert.Equal(0, tracker.ArrangeCallCount);
		}

		[Fact]
		public void SubscribeToUpdates_NullCallback_ThrowsArgumentNullException()
		{
			// Arrange
			ResetLayout();

			// Act & Assert
			var ex = Assert.Throws<ArgumentNullException>(() => PerformanceProfiler.SubscribeToUpdates(null));
			Assert.Equal("callback", ex.ParamName);
		}

		[Fact]
		public void SubscribeToUpdates_NotInitialized_DoesNotThrow()
		{
			// Arrange
			ResetLayout();

			// Act & Assert
			var exception = Record.Exception(() =>
			{
				PerformanceProfiler.SubscribeToUpdates(update =>
				{
					/* no-op */
				});
			});
			Assert.Null(exception);
		}

		[Fact]
		public async Task SubscribeToUpdates_ReceivesLayoutUpdateEvents()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			var receivedUpdates = new List<LayoutUpdate>();
			Action<LayoutUpdate> callback = update =>
			{
				lock (receivedUpdates)
				{
					receivedUpdates.Add(update);
				}
			};

			// Act
			PerformanceProfiler.SubscribeToUpdates(callback);
			var measureTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			await Task.Delay(5);
			measureTracker.Stop();
			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			await Task.Delay(5);
			arrangeTracker.Stop();

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(2, receivedUpdates.Count);
			Assert.Equal(LayoutPassType.Measure, receivedUpdates[0].PassType);
			Assert.Equal("Element1", receivedUpdates[0].Element);
			Assert.Equal(LayoutPassType.Arrange, receivedUpdates[1].PassType);
			Assert.Equal("Element2", receivedUpdates[1].Element);
		}

		[Fact]
		public async Task SubscribeToUpdates_MultipleSubscribers_AllReceiveEvents()
		{
			// Arrange
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

			// Act
			PerformanceProfiler.SubscribeToUpdates(callback1);
			PerformanceProfiler.SubscribeToUpdates(callback2);
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "MultiSubscriber");
			await Task.Delay(5);
			perfTracker.Stop();

			// Assert
			await tracker.WaitForUpdatesAsync();
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
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			var goodSubscriberUpdates = new List<LayoutUpdate>();
			Action<LayoutUpdate> badSubscriber = _ => throw new InvalidOperationException("Bad subscriber");
			Action<LayoutUpdate> goodSubscriber = update =>
			{
				lock (goodSubscriberUpdates) { goodSubscriberUpdates.Add(update); }
			};

			// Act
			PerformanceProfiler.SubscribeToUpdates(badSubscriber);
			PerformanceProfiler.SubscribeToUpdates(goodSubscriber);
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "TestElement");
			await Task.Delay(5);
			perfTracker.Stop();

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Single(goodSubscriberUpdates);
			Assert.Equal("TestElement", goodSubscriberUpdates[0].Element);
		}

		[Fact]
		public void GetHistory_NotInitialized_ReturnsEmpty()
		{
			// Arrange
			ResetLayout();

			// Act
			var history = PerformanceProfiler.GetHistory();

			// Assert
			Assert.Null(history.Layout);
			Assert.True((DateTime.UtcNow - history.TimestampUtc).TotalSeconds < 5,
				$"Timestamp too old: {history.TimestampUtc}");
		}

		[Fact]
		public async Task GetHistory_WithElement_FiltersCorrectly()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			var tracker1 = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			await Task.Delay(5);
			tracker1.Stop();
			var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			await Task.Delay(5);
			tracker2.Stop();

			await tracker.WaitForUpdatesAsync();
			var history = PerformanceProfiler.GetHistory("Element1");
			var layoutHistory = new List<LayoutUpdate>(history.Layout);

			// Assert
			Assert.Single(layoutHistory);
			Assert.Equal("Element1", layoutHistory[0].Element);
			Assert.Equal(LayoutPassType.Measure, layoutHistory[0].PassType);
		}

		[Fact]
		public async Task GetHistory_WithoutElement_ReturnsAllUpdates()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			{
				var tracker1 = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
				await Task.Delay(10); // Simulate layout work
				tracker1.Stop();
			}
			{
				var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
				await Task.Delay(10); // Simulate layout work
				tracker2.Stop();
			}

			// Assert
			await tracker.WaitForUpdatesAsync();
			var layoutHistory = new List<LayoutUpdate>(PerformanceProfiler.GetHistory().Layout);
			Assert.Equal(2, layoutHistory.Count);
			Assert.Contains(layoutHistory,
				u => u.Element.Equals("Element1") && u.PassType == LayoutPassType.Measure);
			Assert.Contains(layoutHistory,
				u => u.Element.Equals("Element2") && u.PassType == LayoutPassType.Arrange);
		}

		[Fact]
		public async Task RecordMeasurePass_ZeroDuration_RecordsCorrectly()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			{
				var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ZeroDuration");
				perfTracker.Stop();
			}

			// Assert
			await tracker.WaitForUpdatesAsync();
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("ZeroDuration", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration > 0,
				$"Expected small positive duration due to Stopwatch overhead, got {tracker.MeasuredDuration:F4}ms");
		}

		[Fact]
		public async Task RecordArrangePass_NullElement_RecordsEmptyString()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const double expectedDelayMs = 5.0;
			double minDuration = expectedDelayMs * (1 - DurationTolerance); // 4.25ms

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange);
			await Task.Delay(5);
			perfTracker.Stop();
			await tracker.WaitForUpdatesAsync();

			// Assert
			var history = PerformanceProfiler.GetHistory();
			var layoutHistory = new List<LayoutUpdate>(history.Layout);
			Assert.Single(layoutHistory);
			Assert.Equal(string.Empty, layoutHistory[0].Element);
			Assert.Equal(LayoutPassType.Arrange, layoutHistory[0].PassType);
			Assert.True(layoutHistory[0].TotalTime >= minDuration,
				$"Expected TotalTime ≥ {minDuration:F2}ms, got {layoutHistory[0].TotalTime:F2}ms");
		}

		[Fact]
		public async Task RecordMeasurePass_VerticalStackLayoutWithLabel_ReportsStandaloneDuration()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			var parent = new MockVisualTreeElement(true);
			var child = new MockVisualTreeElement(false) { Parent = parent };
			parent.Children.Add(child);

			// Act
			var childTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, child);
			await Task.Delay(30); // Simulate layout work
			childTracker.Stop();

			var parentTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, parent);
			await Task.Delay(10); // Simulate layout work
			parentTracker.Stop();

			await tracker.WaitForUpdatesAsync();

			// Assert
			var stats = PerformanceProfiler.GetStats();
			Assert.NotNull(stats.Layout);
			Assert.True(stats.Layout.MeasureDuration > 0.0,
				$"Expected non-zero MeasureDuration, got {stats.Layout.MeasureDuration:F2}ms");

			var layoutHistory = new List<LayoutUpdate>(PerformanceProfiler.GetHistory().Layout);
			Assert.Equal(2, layoutHistory.Count);

			var parentUpdate = layoutHistory.Find(u => u.Element == parent);
			Assert.NotNull(parentUpdate);
			Assert.Equal(LayoutPassType.Measure, parentUpdate.PassType);
			double parentMin = 10.0 * (1 - DurationTolerance);
			Assert.True(parentUpdate.TotalTime >= parentMin,
				$"Expected parent TotalTime ≥ {parentMin:F2}ms, got {parentUpdate.TotalTime:F2}ms");

			var childUpdate = layoutHistory.Find(u => u.Element == child);
			Assert.NotNull(childUpdate);
			Assert.Equal(LayoutPassType.Measure, childUpdate.PassType);
			double childMin = 30.0 * (1 - DurationTolerance);
			Assert.True(childUpdate.TotalTime >= childMin,
				$"Expected child TotalTime ≥ {childMin:F2}ms, got {childUpdate.TotalTime:F2}ms");
		}

		[Fact]
		public async Task RecordMeasurePass_DeepLayoutHierarchy_TracksEachElementIndependently()
		{
			// Arrange
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

			// Act
			var rootTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, root);
			await Task.Delay(10);
			var panelTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, panel);
			await Task.Delay(15);
			var label1Tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, label1);
			await Task.Delay(25);
			label1Tracker.Stop();
			var label2Tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, label2);
			await Task.Delay(30);
			label2Tracker.Stop();
			panelTracker.Stop();
			rootTracker.Stop();
			await tracker.WaitForUpdatesAsync();

			// Assert
			var stats = PerformanceProfiler.GetStats();
			Assert.NotNull(stats.Layout);
			Assert.True(stats.Layout.MeasureDuration > 0.0,
				$"Expected non-zero MeasureDuration, got {stats.Layout.MeasureDuration:F2}ms");

			var layoutHistory = new List<LayoutUpdate>(PerformanceProfiler.GetHistory().Layout);
			Assert.Equal(4, layoutHistory.Count);

			var rootUpdate = layoutHistory.Find(u => u.Element == root);
			Assert.NotNull(rootUpdate);
			Assert.Equal(LayoutPassType.Measure, rootUpdate.PassType);
			Assert.True(rootUpdate.TotalTime >= 10.0 * (1 - DurationTolerance),
				$"Expected Root ≥ {10.0 * 0.85:F2}ms, got {rootUpdate.TotalTime:F2}ms");

			var panelUpdate = layoutHistory.Find(u => u.Element == panel);
			Assert.NotNull(panelUpdate);
			Assert.Equal(LayoutPassType.Measure, panelUpdate.PassType);
			Assert.True(panelUpdate.TotalTime >= 15.0 * (1 - DurationTolerance),
				$"Expected Panel ≥ {15.0 * 0.85:F2}ms, got {panelUpdate.TotalTime:F2}ms");

			var label1Update = layoutHistory.Find(u => u.Element == label1);
			Assert.NotNull(label1Update);
			Assert.Equal(LayoutPassType.Measure, label1Update.PassType);
			Assert.True(label1Update.TotalTime >= 25.0 * (1 - DurationTolerance),
				$"Expected Label1 ≥ {25.0 * 0.85:F2}ms, got {label1Update.TotalTime:F2}ms");

			var label2Update = layoutHistory.Find(u => u.Element == label2);
			Assert.NotNull(label2Update);
			Assert.Equal(LayoutPassType.Measure, label2Update.PassType);
			Assert.True(label2Update.TotalTime >= 30.0 * (1 - DurationTolerance),
				$"Expected Label2 ≥ {30.0 * 0.85:F2}ms, got {label2Update.TotalTime:F2}ms");
		}
	}

	// MockVisualTreeElement implements IVisualTreeElement instead of ILayout for simplicity in unit tests.
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
		readonly object _lock = new();
		readonly List<LayoutUpdate> _history = new();
		readonly List<Action<LayoutUpdate>> _subscribers = new();

		readonly ConcurrentDictionary<(object Element, LayoutPassType PassType), double>
			_childDurations = new();

		public int MeasureCallCount { get; private set; }
		public double MeasuredDuration { get; private set; }
		public object MeasuredElement { get; private set; }
		public int ArrangeCallCount { get; private set; }
		public double ArrangedDuration { get; private set; }
		public object ArrangedElement { get; private set; }

		// Track pending updates with a list of tasks
		readonly List<Task> _pendingTasks = new();

		public FakeLayoutTracker()
		{
		}

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
			LayoutUpdate update;
			Task asyncTask;

			lock (_lock)
			{
				double standaloneDuration = duration;

				// Calculate child durations if element is provided
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

				// Create the update object and add to history immediately
				update = new LayoutUpdate(passType, standaloneDuration, element ?? string.Empty, DateTime.UtcNow);
				_history.Add(update);

				// Create async task for subscriber notifications
				asyncTask = Task.Run(async () =>
				{
					await PublishLayoutUpdateAsync(update);
				});

				// Track the task so we can wait for it
				_pendingTasks.Add(asyncTask);

				// Clean up completed tasks to prevent memory leaks
				_pendingTasks.RemoveAll(t => t.IsCompleted);
			}
		}

		async Task PublishLayoutUpdateAsync(LayoutUpdate update)
		{
			Action<LayoutUpdate>[] subscribersSnapshot;
			lock (_lock)
			{
				subscribersSnapshot = _subscribers.ToArray();
			}

			foreach (var subscriber in subscribersSnapshot)
			{
				try
				{
					await Task.Run(() => subscriber.Invoke(update));
				}
				catch
				{
					// Swallow exceptions to avoid breaking the tracker
				}
			}
		}

		// Wait for all pending updates to be processed
		public async Task WaitForUpdatesAsync(CancellationToken cancellationToken = default)
		{
			Task[] tasksToWait;

			lock (_lock)
			{
				// Get a snapshot of pending tasks
				tasksToWait = _pendingTasks.Where(t => !t.IsCompleted).ToArray();
			}

			if (tasksToWait.Length > 0)
			{
				await Task.WhenAll(tasksToWait).WaitAsync(cancellationToken);
			}

			// Small delay to ensure all operations are complete
			await Task.Delay(1, cancellationToken);
		}
	}
}