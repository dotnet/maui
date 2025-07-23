using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Performance;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Collection("PerformanceProfilerTests")]
	[Category(TestCategory.Core)]
	public class PerformanceProfilerTests
	{
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

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ElementA");
			await Task.Delay(5); // Simulate layout work
			perfTracker.Stop();

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("ElementA", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration > 0, $"Expected MeasuredDuration > 0, got {tracker.MeasuredDuration}");
		}

		[Fact]
		public async Task Start_LayoutArrange_AfterInitialization_RecordsDurationAndElement()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "ElementB");
			await Task.Delay(5); // Simulate layout work
			perfTracker.Stop();

			// Assert
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.Equal("ElementB", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration > 0, $"Expected ArrangedDuration > 0, got {tracker.ArrangedDuration}");
		}

		[Fact]
		public async Task GetStats_AfterTracking_ReturnsLatestDurations()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Track measure
			var measureTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "M");
			await Task.Delay(5); // Simulate short work
			measureTracker.Stop();
			var statsAfterMeasure = PerformanceProfiler.GetStats();

			// Track arrange
			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "A");
			await Task.Delay(5); // Simulate short work
			arrangeTracker.Stop();
			var statsAfterArrange = PerformanceProfiler.GetStats();

			// Assert measure stats
			Assert.NotNull(statsAfterMeasure.Layout);
			Assert.True(statsAfterMeasure.Layout.MeasureDuration > 0,
				$"Expected MeasureDuration > 0 after tracking, got {statsAfterMeasure.Layout.MeasureDuration}");
			Assert.Equal(0, statsAfterMeasure.Layout.ArrangeDuration);

			// Assert arrange stats
			Assert.NotNull(statsAfterArrange.Layout);
			Assert.True(statsAfterArrange.Layout.ArrangeDuration > 0,
				$"Expected ArrangeDuration > 0 after tracking, got {statsAfterArrange.Layout.ArrangeDuration}");
			Assert.True(statsAfterArrange.Layout.MeasureDuration > 0,
				$"Expected MeasureDuration still present, got {statsAfterArrange.Layout.MeasureDuration}");
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
			// Attempt to track with an undefined performance category
			var perfTracker = PerformanceProfiler.Start(unknownType, "TestElement");
			perfTracker.Stop();

			// Assert
			// Ensure no measure or arrange calls were registered
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

			// Act
			// Measure tracker
			var tracker1 = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			await Task.Delay(5);
			tracker1.Stop();

			// Act
			// Arrange tracker
			var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			await Task.Delay(5);
			tracker2.Stop();

			// Assert
			// Measure
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("Element1", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration > 0, $"Expected MeasuredDuration > 0, got {tracker.MeasuredDuration}");

			// Assert
			// Arrange
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.Equal("Element2", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration > 0, $"Expected ArrangedDuration > 0, got {tracker.ArrangedDuration}");
		}

		[Fact]
		public void Start_WithoutElement_RecordsDuration()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure);
			Thread.Sleep(5); // Simulate work with a short delay
			perfTracker.Stop();

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration > 0);
		}

		[Fact]
		public async Task UsingVar_LayoutMeasure_AutomaticallyStops()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			// Duration measured and disposed via using var
			{
				using var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "UsingElement");
				await Task.Delay(5); // Simulate short work
			}

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration > 0, $"Expected MeasuredDuration > 0, got {tracker.MeasuredDuration}");
			Assert.Equal("UsingElement", tracker.MeasuredElement);
		}

		[Fact]
		public async Task UsingVar_LayoutArrange_AutomaticallyStops()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			{
				using var perfTracker =
					PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "UsingArrangeElement");
				await Task.Delay(5);
			}

			// Assert
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.True(tracker.ArrangedDuration > 0, $"Expected ArrangedDuration > 0, got {tracker.ArrangedDuration}");
			Assert.Equal("UsingArrangeElement", tracker.ArrangedElement);
		}

		[Fact]
		public async Task UsingVar_NotInitialized_DoesNotThrow()
		{
			// Arrange
			ResetLayout();

			// Act & Assert
			// No exception expected
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

			// Act
			try
			{
				using var perfTracker =
					PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ExceptionElement");
				await Task.Delay(5); // Simulate work before exception
				throw new InvalidOperationException("Intentional test exception");
			}
			catch (InvalidOperationException)
			{
				// Expected
			}

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration > 0, $"Expected MeasuredDuration > 0, got {tracker.MeasuredDuration}");
		}

		[Fact]
		public async Task UsingVar_MultipleNestedScopes_RecordsEachCorrectly()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act – nested scope simulation
			{
				using var outerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Outer");
				await Task.Delay(5);
			}

			{
				using var innerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Inner");
				await Task.Delay(5);
			}

			// Assert
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
			const PerformanceCategory unknownType = (PerformanceCategory)99;

			// Act – simulate unknown category tracking
			{
				using var perfTracker = PerformanceProfiler.Start(unknownType, "UnknownElement");
				await Task.Delay(5); // Simulate short usage
			}

			// Assert – ensure no measure/arrange tracking occurred
			Assert.Equal(0, tracker.MeasureCallCount);
			Assert.Equal(0, tracker.ArrangeCallCount);
		}

		[Fact]
		public void SubscribeToUpdates_NullCallback_ThrowsArgumentNullException()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act & Assert
			var ex = Assert.Throws<ArgumentNullException>(() => PerformanceProfiler.SubscribeToUpdates(null));
			Assert.Equal("callback", ex.ParamName);
		}

		[Fact]
		public void SubscribeToUpdates_NotInitialized_DoesNotThrow()
		{
			// Arrange
			ResetLayout();

			// Act & Assert – subscribing before profiler initialization should not throw
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
			Thread.Sleep(5); // Simulate work with a short delay
			measureTracker.Stop();

			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			Thread.Sleep(5); // Simulate work with a short delay
			arrangeTracker.Stop();

			// Wait for events to propagate
			await Task.Delay(250);

			// Assert
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
				lock (subscriber1Updates)
				{
					subscriber1Updates.Add(update);
				}
			};
			Action<LayoutUpdate> callback2 = update =>
			{
				lock (subscriber2Updates)
				{
					subscriber2Updates.Add(update);
				}
			};

			// Act
			PerformanceProfiler.SubscribeToUpdates(callback1);
			PerformanceProfiler.SubscribeToUpdates(callback2);

			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "MultiSubscriber");
			Thread.Sleep(5); // Simulate work with a short delay
			perfTracker.Stop();

			// Wait for events to propagate
			await Task.Delay(250);

			// Assert
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
				lock (goodSubscriberUpdates)
				{
					goodSubscriberUpdates.Add(update);
				}
			};

			// Act
			PerformanceProfiler.SubscribeToUpdates(badSubscriber);
			PerformanceProfiler.SubscribeToUpdates(goodSubscriber);

			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "TestElement");
			Thread.Sleep(5); // Simulate work with a short delay
			perfTracker.Stop();

			// Wait for events to propagate
			await Task.Delay(250);

			// Assert
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
			Thread.Sleep(5); // Simulate work with a short delay
			tracker1.Stop();

			var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			Thread.Sleep(5); // Simulate work with a short delay
			tracker2.Stop();

			// Wait for events to propagate
			await Task.Delay(250);

			var history = PerformanceProfiler.GetHistory("Element1");

			// Assert
			var layoutHistory = new List<LayoutUpdate>(history.Layout);
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
			var tracker1 = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Element1");
			Thread.Sleep(5); // Simulate work with a short delay
			tracker1.Stop();

			var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			Thread.Sleep(5); // Simulate work with a short delay
			tracker2.Stop();

			// Wait for events to propagate
			await Task.Delay(250);

			var history = PerformanceProfiler.GetHistory();

			// Assert
			var layoutHistory = new List<LayoutUpdate>(history.Layout);
			Assert.Equal(2, layoutHistory.Count);
			Assert.Contains(layoutHistory, u => u.Element.Equals("Element1") && u.PassType == LayoutPassType.Measure);
			Assert.Contains(layoutHistory, u => u.Element.Equals("Element2") && u.PassType == LayoutPassType.Arrange);
		}

		[Fact]
		public void RecordMeasurePass_ZeroDuration_RecordsCorrectly()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			{
				var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ZeroDuration");
				perfTracker.Stop(); // Stop immediately
			}

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("ZeroDuration", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration > 0);
		}

		[Fact]
		public async Task RecordArrangePass_NullElement_RecordsEmptyString()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange);
			Thread.Sleep(5); // Simulate work with a short delay
			perfTracker.Stop();

			// Wait for events to propagate
			await Task.Delay(250);

			// Assert
			var history = PerformanceProfiler.GetHistory();
			var layoutHistory = new List<LayoutUpdate>(history.Layout);
			Assert.Single(layoutHistory);
			Assert.Equal(string.Empty, layoutHistory[0].Element);
			Assert.Equal(LayoutPassType.Arrange, layoutHistory[0].PassType);
			Assert.True(layoutHistory[0].TotalTime > 0);
		}

		[Fact]
		public async Task RecordMeasurePass_VerticalStackLayoutWithLabel_ReportsStandaloneDuration()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Create mock layout elements
			var parent = new MockVisualTreeElement(true); // e.g., VerticalStackLayout
			var child = new MockVisualTreeElement(false) { Parent = parent }; // e.g., Label
			parent.Children.Add(child);

			// Act
			// Simulate layout measure passes
			var childTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, child);
			await Task.Delay(30); // Simulate child measure
			childTracker.Stop();

			var parentTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, parent);
			await Task.Delay(10); // Simulate parent measure (excluding child)
			parentTracker.Stop();

			await Task.Delay(250); // Allow profiler updates to settle

			// Assert profiler stats
			var stats = PerformanceProfiler.GetStats();
			Assert.NotNull(stats.Layout);
			Assert.True(stats.Layout.MeasureDuration > 0.0,
				$"Expected non-zero MeasureDuration, got {stats.Layout.MeasureDuration}ms");

			// Assert history updates
			var history = PerformanceProfiler.GetHistory().Layout;
			var layoutHistory = new List<LayoutUpdate>(history);

			Assert.Equal(2, layoutHistory.Count);

			var parentUpdate = layoutHistory.Find(u => u.Element == parent);
			Assert.NotNull(parentUpdate);
			Assert.Equal(LayoutPassType.Measure, parentUpdate.PassType);
			Assert.True(parentUpdate.TotalTime >= 10.0,
				$"Expected parent TotalTime ≥ 10ms, got {parentUpdate.TotalTime}ms");

			var childUpdate = layoutHistory.Find(u => u.Element == child);
			Assert.NotNull(childUpdate);
			Assert.Equal(LayoutPassType.Measure, childUpdate.PassType);
			Assert.True(childUpdate.TotalTime >= 30.0,
				$"Expected child TotalTime ≥ 30ms, got {childUpdate.TotalTime}ms");
		}

		[Fact]
		public async Task RecordMeasurePass_DeepLayoutHierarchy_TracksEachElementIndependently()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Build hierarchy:
			// RootLayout
			// └── Layout
			//     ├── Label1
			//     └── Label2
			var root = new MockVisualTreeElement(true); // e.g., Grid
			var panel = new MockVisualTreeElement(true) { Parent = root }; // e.g., StackLayout
			var label1 = new MockVisualTreeElement(false) { Parent = panel };
			var label2 = new MockVisualTreeElement(false) { Parent = panel };

			root.Children.Add(panel);
			panel.Children.Add(label1);
			panel.Children.Add(label2);

			// Act – simulate layout measure passes
			using (var label1Tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, label1))
			{
				await Task.Delay(15); // Simulate Label1 measure
			}

			using (var label2Tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, label2))
			{
				await Task.Delay(20); // Simulate Label2 measure
			}

			using (var panelTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, panel))
			{
				await Task.Delay(10); // Simulate Panel's own measure (excluding children)
			}

			using (var rootTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, root))
			{
				await Task.Delay(5); // Simulate Root's own measure (excluding children)
			}

			await Task.Delay(250); // Allow profiler updates to settle

			// Assert profiler stats
			var stats = PerformanceProfiler.GetStats();
			Assert.NotNull(stats.Layout);
			Assert.True(stats.Layout.MeasureDuration > 0.0,
				$"Expected MeasureDuration > 0, got {stats.Layout.MeasureDuration}");

			// Assert layout history
			var layoutHistory = new List<LayoutUpdate>(PerformanceProfiler.GetHistory().Layout);
			Assert.Equal(4, layoutHistory.Count);

			var rootUpdate = layoutHistory.Find(u => u.Element == root);
			Assert.NotNull(rootUpdate);
			Assert.Equal(LayoutPassType.Measure, rootUpdate.PassType);
			Assert.True(rootUpdate.TotalTime >= 5.0, $"Expected Root measure ≥ 5ms, got {rootUpdate.TotalTime}ms");

			var panelUpdate = layoutHistory.Find(u => u.Element == panel);
			Assert.NotNull(panelUpdate);
			Assert.Equal(LayoutPassType.Measure, panelUpdate.PassType);
			Assert.True(panelUpdate.TotalTime >= 10.0, $"Expected Panel measure ≥ 10ms, got {panelUpdate.TotalTime}ms");

			var label1Update = layoutHistory.Find(u => u.Element == label1);
			Assert.NotNull(label1Update);
			Assert.Equal(LayoutPassType.Measure, label1Update.PassType);
			Assert.True(label1Update.TotalTime >= 15.0,
				$"Expected Label1 measure ≥ 15ms, got {label1Update.TotalTime}ms");

			var label2Update = layoutHistory.Find(u => u.Element == label2);
			Assert.NotNull(label2Update);
			Assert.Equal(LayoutPassType.Measure, label2Update.PassType);
			Assert.True(label2Update.TotalTime >= 20.0,
				$"Expected Label2 measure ≥ 20ms, got {label2Update.TotalTime}ms");
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
		readonly object _lock = new object();
		readonly List<LayoutUpdate> _history = new List<LayoutUpdate>();
		readonly object _historyLock = new object();
		readonly List<Action<LayoutUpdate>> _subscribers = new List<Action<LayoutUpdate>>();
		readonly object _subscriberLock = new object();

		readonly ConcurrentDictionary<(object Element, LayoutPassType PassType), double> _childDurations = new();

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
			lock (_historyLock)
			{
				if (element == null)
					return new List<LayoutUpdate>(_history);

				return _history.FindAll(u => Equals(u.Element, element));
			}
		}

		public void SubscribeToLayoutUpdates(Action<LayoutUpdate> callback)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			lock (_subscriberLock)
			{
				_subscribers.Add(callback);
			}
		}

		public void RecordMeasurePass(double duration, object element = null)
		{
			double standaloneDuration = duration;

			bool hasChildren = element is IVisualTreeElement vte && vte.GetVisualChildren().Count > 0;
			object parent = element is IVisualTreeElement vteParent ? vteParent.GetVisualParent() : null;

			if (parent is not null)
			{
				_childDurations.AddOrUpdate(
					(parent, LayoutPassType.Measure),
					duration,
					(key, oldValue) => oldValue + duration);
			}

			if (element is ILayout)
			{
				standaloneDuration = _childDurations.TryGetValue((element, LayoutPassType.Measure), out var childTime)
					? duration - childTime
					: duration;
				_childDurations.TryRemove((element, LayoutPassType.Measure), out _);
			}

			lock (_lock)
			{
				MeasureCallCount++;
				MeasuredDuration = element is ILayout ? standaloneDuration : duration;
				MeasuredElement = element;
			}

			var update = new LayoutUpdate(
				LayoutPassType.Measure,
				duration,
				element ?? string.Empty,
				DateTime.UtcNow);

			lock (_historyLock)
			{
				_history.Add(update);
			}

			PublishLayoutUpdate(update);
		}

		public void RecordArrangePass(double duration, object element = null)
		{
			double standaloneDuration = duration;

			bool hasChildren = element is IVisualTreeElement vte && vte.GetVisualChildren().Count > 0;
			object parent = element is IVisualTreeElement vteParent ? vteParent.GetVisualParent() : null;

			if (parent is not null)
			{
				_childDurations.AddOrUpdate(
					(parent, LayoutPassType.Arrange),
					duration,
					(key, oldValue) => oldValue + duration);
			}

			if (element is ILayout)
			{
				standaloneDuration = _childDurations.TryGetValue((element, LayoutPassType.Arrange), out var childTime)
					? duration - childTime
					: duration;
				_childDurations.TryRemove((element, LayoutPassType.Arrange), out _);
			}

			lock (_lock)
			{
				ArrangeCallCount++;
				ArrangedDuration = element is ILayout ? standaloneDuration : duration;
				ArrangedElement = element;
			}

			var update = new LayoutUpdate(
				LayoutPassType.Arrange,
				duration,
				element ?? string.Empty,
				DateTime.UtcNow);

			lock (_historyLock)
			{
				_history.Add(update);
			}

			PublishLayoutUpdate(update);
		}

		private void PublishLayoutUpdate(LayoutUpdate update)
		{
			Action<LayoutUpdate>[] subscribersSnapshot;

			lock (_subscriberLock)
			{
				subscribersSnapshot = _subscribers.ToArray();
			}

			foreach (var subscriber in subscribersSnapshot)
			{
				try
				{
					subscriber.Invoke(update);
				}
				catch
				{
					// Swallow exceptions from subscribers to avoid breaking the tracker.
				}
			}
		}
	}
}