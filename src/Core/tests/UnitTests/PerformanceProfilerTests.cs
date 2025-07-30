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
		
		const double ShortDelayMs = 25.0;
		const double MediumDelayMs = 50.0;
		const double LongDelayMs = 100.0;
		
		void ResetLayout()
		{
			var layoutProperty = typeof(PerformanceProfiler).GetProperty(
				"Layout",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			layoutProperty?.SetValue(null, null);
        
			Thread.Sleep(10);
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
				using var perfTracker =
					PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "UsingElement");
				await Task.Delay(TimeSpan.FromMilliseconds(expectedDelayMs));
			}

			await tracker.WaitForUpdatesAsync();
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
			var receivedUpdates = new List<LayoutUpdate>();
			Action<LayoutUpdate> callback = update =>
			{
				lock (receivedUpdates)
				{
					receivedUpdates.Add(update);
				}
			};

			PerformanceProfiler.SubscribeToUpdates(callback);
			var measureTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			await Task.Delay((int)ShortDelayMs);
			measureTracker.Stop();
			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			await Task.Delay((int)ShortDelayMs);
			arrangeTracker.Stop();

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
			var history = PerformanceProfiler.GetHistory("Element1");
			var layoutHistory = new List<LayoutUpdate>(history.Layout);

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
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			{
				var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ZeroDuration");
				perfTracker.Stop();
			}

			await tracker.WaitForUpdatesAsync();
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
			Assert.NotNull(stats.Layout);
			Assert.True(stats.Layout.MeasureDuration > 0.0,
				$"Expected non-zero MeasureDuration, got {stats.Layout.MeasureDuration:F2}ms");

			var layoutHistory = new List<LayoutUpdate>(PerformanceProfiler.GetHistory().Layout);
			Assert.Equal(2, layoutHistory.Count);

			var parentUpdate = layoutHistory.Find(u => u.Element == parent);
			Assert.NotNull(parentUpdate);
			Assert.Equal(LayoutPassType.Measure, parentUpdate.PassType);
			double parentMin = MediumDelayMs * (1 - DurationTolerance);
			Assert.True(parentUpdate.TotalTime >= parentMin,
				$"Expected parent TotalTime ≥ {parentMin:F2}ms, got {parentUpdate.TotalTime:F2}ms. " +
				$"Parent delay was {MediumDelayMs}ms with {DurationTolerance:P} tolerance.");

			var childUpdate = layoutHistory.Find(u => u.Element == child);
			Assert.NotNull(childUpdate);
			Assert.Equal(LayoutPassType.Measure, childUpdate.PassType);
			double childMin = LongDelayMs * (1 - DurationTolerance);
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
			Assert.NotNull(stats.Layout);
			Assert.True(stats.Layout.MeasureDuration > 0.0,
				$"Expected non-zero MeasureDuration, got {stats.Layout.MeasureDuration:F2}ms");

			var layoutHistory = new List<LayoutUpdate>(PerformanceProfiler.GetHistory().Layout);
			Assert.Equal(4, layoutHistory.Count);

			var rootUpdate = layoutHistory.Find(u => u.Element == root);
			Assert.NotNull(rootUpdate);
			Assert.Equal(LayoutPassType.Measure, rootUpdate.PassType);
			Assert.True(rootUpdate.TotalTime >= ShortDelayMs * (1 - DurationTolerance),
				$"Expected Root ≥ {ShortDelayMs * 0.85:F2}ms, got {rootUpdate.TotalTime:F2}ms");

			var panelUpdate = layoutHistory.Find(u => u.Element == panel);
			Assert.NotNull(panelUpdate);
			Assert.Equal(LayoutPassType.Measure, panelUpdate.PassType);
			Assert.True(panelUpdate.TotalTime >= ShortDelayMs * (1 - DurationTolerance),
				$"Expected Panel ≥ {ShortDelayMs * 0.85:F2}ms, got {panelUpdate.TotalTime:F2}ms");

			var label1Update = layoutHistory.Find(u => u.Element == label1);
			Assert.NotNull(label1Update);
			Assert.Equal(LayoutPassType.Measure, label1Update.PassType);
			Assert.True(label1Update.TotalTime >= ShortDelayMs * (1 - DurationTolerance),
				$"Expected Label1 ≥ {ShortDelayMs * 0.85:F2}ms, got {label1Update.TotalTime:F2}ms");

			var label2Update = layoutHistory.Find(u => u.Element == label2);
			Assert.NotNull(label2Update);
			Assert.Equal(LayoutPassType.Measure, label2Update.PassType);
			Assert.True(label2Update.TotalTime >= ShortDelayMs * (1 - DurationTolerance),
				$"Expected Label2 ≥ {ShortDelayMs * 0.85:F2}ms, got {label2Update.TotalTime:F2}ms");
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

        private int _activeOperations = 0;

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
            
            lock (_lock)
            {
                Interlocked.Increment(ref _activeOperations);
                
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

                var update = new LayoutUpdate(passType, standaloneDuration, element ?? string.Empty, DateTime.UtcNow);
                _history.Add(update);

                if (_subscribers.Count > 0)
                {
	                var subscribersSnapshot = _subscribers.ToArray();

	                notificationTask = new Task(() => PublishLayoutUpdateSafe(update, subscribersSnapshot));
	                _pendingTasks.Add(notificationTask);

	                if (_pendingTasks.Count > TaskCleanupThreshold)
	                {
		                _pendingTasks.RemoveAll(t => t.IsCompleted);
	                }
                }
                else
                {
                    Interlocked.Decrement(ref _activeOperations);
                }
            }
            
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
            Task[] tasksToWait;
            lock (_lock)
            {
                tasksToWait = _pendingTasks.Where(t => !t.IsCompleted).ToArray();
            }

            if (tasksToWait.Length > 0)
            {
                try
                {
                    await Task.WhenAll(tasksToWait).WaitAsync(cancellationToken);
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

            var maxWaitTime = TimeSpan.FromSeconds(5);
            var startTime = DateTime.UtcNow;
            
            while (_activeOperations > 0 && (DateTime.UtcNow - startTime) < maxWaitTime)
            {
                await Task.Delay(5, cancellationToken);
            }

            lock (_lock)
            {
                _pendingTasks.RemoveAll(t => t.IsCompleted);
            }

            await Task.Delay(25, cancellationToken);
        }
    }
}