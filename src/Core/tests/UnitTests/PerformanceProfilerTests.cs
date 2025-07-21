using System;
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

            // Act & Assert: Should not throw
            var tracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Test");
            tracker.Stop();
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
        public void Start_LayoutMeasure_AfterInitialization_RecordsDurationAndElement()
        {
            // Arrange
            ResetLayout();
            var tracker = new FakeLayoutTracker();
            PerformanceProfiler.Initialize(tracker);

            // Act
            var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ElementA");
            Thread.Sleep(5); // Simulate work with a short delay
            perfTracker.Stop();

            // Assert
            Assert.Equal(1, tracker.MeasureCallCount);
            Assert.True(tracker.MeasuredDuration > 0);
        }

        [Fact]
        public void Start_LayoutArrange_AfterInitialization_RecordsDurationAndElement()
        {
            // Arrange
            ResetLayout();
            var tracker = new FakeLayoutTracker();
            PerformanceProfiler.Initialize(tracker);

            // Act
            var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "ElementB");
            Thread.Sleep(5); // Simulate work with a short delay
            perfTracker.Stop();

            // Assert
            Assert.Equal(1, tracker.ArrangeCallCount);
            Assert.True(tracker.ArrangedDuration > 0);
        }

        [Fact]
        public void GetStats_AfterTracking_ReturnsLatestDurations()
        {
            // Arrange
            ResetLayout();
            var tracker = new FakeLayoutTracker();
            PerformanceProfiler.Initialize(tracker);

            // Track measure
            var measureTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "M");
            Thread.Sleep(5); // Simulate work with a short delay
            measureTracker.Stop();
            var statsAfterMeasure = PerformanceProfiler.GetStats();

            // Track arrange
            var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "A");
            Thread.Sleep(5); // Simulate work with a short delay
            arrangeTracker.Stop();
            var statsAfterArrange = PerformanceProfiler.GetStats();

            // Assert measure stats
            Assert.True(statsAfterMeasure.Layout.MeasureDuration > 0);
            Assert.Equal(0, statsAfterMeasure.Layout.ArrangeDuration);

            // Assert arrange stats
            Assert.True(statsAfterArrange.Layout.ArrangeDuration > 0);
            Assert.True(statsAfterArrange.Layout.MeasureDuration > 0);
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
        public void Start_MultipleTrackers_RecordsEachPass()
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

            // Assert
            Assert.Equal(1, tracker.MeasureCallCount);
            Assert.Equal("Element1", tracker.MeasuredElement);

            Assert.Equal(1, tracker.ArrangeCallCount);
            Assert.Equal("Element2", tracker.ArrangedElement);
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
        public void UsingVar_LayoutMeasure_AutomaticallyStops()
        {
            // Arrange
            ResetLayout();
            var tracker = new FakeLayoutTracker();
            PerformanceProfiler.Initialize(tracker);

            // Act
            {
                using var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "UsingElement");
                Thread.Sleep(5); // Simulate work with a short delay
                // perfTracker.Dispose() is called automatically here
            }

            // Assert
            Assert.Equal(1, tracker.MeasureCallCount);
            Assert.True(tracker.MeasuredDuration > 0);
            Assert.Equal("UsingElement", tracker.MeasuredElement);
        }

        [Fact]
        public void UsingVar_LayoutArrange_AutomaticallyStops()
        {
            // Arrange
            ResetLayout();
            var tracker = new FakeLayoutTracker();
            PerformanceProfiler.Initialize(tracker);

            // Act
            {
                using var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "UsingArrangeElement");
                Thread.Sleep(5); // Simulate work with a short delay
                // perfTracker.Dispose() called automatically here
            }

            // Assert
            Assert.Equal(1, tracker.ArrangeCallCount);
            Assert.True(tracker.ArrangedDuration >= 2 && tracker.ArrangedDuration <= 10,
                $"Expected duration in range 2-10ms, but got {tracker.ArrangedDuration}ms");
            Assert.Equal("UsingArrangeElement", tracker.ArrangedElement);
        }

        [Fact]
        public void UsingVar_NotInitialized_DoesNotThrow()
        {
            // Arrange
            ResetLayout();

            // Act & Assert: Should not throw
            {
                using var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Test");
                Thread.Sleep(5);
                // perfTracker.Dispose() is called automatically here
            }
        }

        [Fact]
        public void UsingVar_WithException_StillRecordsBeforeException()
        {
            // Arrange
            ResetLayout();
            var tracker = new FakeLayoutTracker();
            PerformanceProfiler.Initialize(tracker);

            // Act & Assert
            try
            {
                using var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ExceptionElement");
                Thread.Sleep(5); // Simulate work with a short delay
                throw new InvalidOperationException("Test exception");
            }
            catch (InvalidOperationException)
            {
                // Expected exception
            }

            // Assert that tracking still occurred despite the exception
            Assert.Equal(1, tracker.MeasureCallCount);
            Assert.True(tracker.MeasuredDuration > 0);
        }

        [Fact]
        public void UsingVar_MultipleNestedScopes_RecordsEachCorrectly()
        {
            // Arrange
            ResetLayout();
            var tracker = new FakeLayoutTracker();
            PerformanceProfiler.Initialize(tracker);

            // Act
            {
                using var outerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "Outer");
                Thread.Sleep(5); // Simulate work with a short delay
                {
                    using var innerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Inner");
                    Thread.Sleep(5); // Simulate work with a short delay
                    // innerTracker.Dispose() called here
                }
                // outerTracker.Dispose() called here
            }

            // Assert
            Assert.Equal(1, tracker.MeasureCallCount);
            Assert.Equal(1, tracker.ArrangeCallCount);
        }

        [Fact]
        public void UsingVar_UnknownPerformanceType_DoesNotRecord()
        {
            // Arrange
            ResetLayout();
            var tracker = new FakeLayoutTracker();
            PerformanceProfiler.Initialize(tracker);
            const PerformanceCategory unknownType = (PerformanceCategory)99;

            // Act
            {
                using var perfTracker = PerformanceProfiler.Start(unknownType, "UnknownElement");
                Thread.Sleep(5);
                // perfTracker.Dispose() called automatically here
            }

            // Assert
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

            // Act & Assert
            PerformanceProfiler.SubscribeToUpdates(update => { });
            // Should not throw
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
            await Task.Delay(50);

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
            await Task.Delay(50);

            // Assert
            Assert.Single(subscriber1Updates);
            Assert.Single(subscriber2Updates);
            Assert.Equal("MultiSubscriber", subscriber1Updates[0].Element);
            Assert.Equal("MultiSubscriber", subscriber2Updates[0].Element);
            Assert.Equal(subscriber1Updates[0].TotalTime, subscriber2Updates[0].TotalTime);
            Assert.True(subscriber1Updates[0].TotalTime >= 2 && subscriber1Updates[0].TotalTime <= 10,
                $"Expected duration in range 2-10ms, but got {subscriber1Updates[0].TotalTime}ms");
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
            await Task.Delay(50);

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
            await Task.Delay(50);

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
            await Task.Delay(50);

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
            var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "ZeroDuration");
            perfTracker.Stop(); // Stop immediately

            // Assert
            Assert.Equal(1, tracker.MeasureCallCount);
            Assert.Equal("ZeroDuration", tracker.MeasuredElement);
            Assert.InRange(tracker.MeasuredDuration, 0, 5);
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
            await Task.Delay(50);

            // Assert
            var history = PerformanceProfiler.GetHistory();
            var layoutHistory = new List<LayoutUpdate>(history.Layout);
            Assert.Single(layoutHistory);
            Assert.Equal(string.Empty, layoutHistory[0].Element);
            Assert.Equal(LayoutPassType.Arrange, layoutHistory[0].PassType);
            Assert.True(layoutHistory[0].TotalTime >= 2 && layoutHistory[0].TotalTime <= 10,
                $"Expected duration in range 2-10ms, but got {layoutHistory[0].TotalTime}ms");
        }
    }

    internal class FakeLayoutTracker : ILayoutPerformanceTracker
    {
        readonly object _lock = new object();
        readonly List<LayoutUpdate> _history = new List<LayoutUpdate>();
        readonly object _historyLock = new object();
        readonly List<Action<LayoutUpdate>> _subscribers = new List<Action<LayoutUpdate>>();
        readonly object _subscriberLock = new object();

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
            lock (_lock)
            {
                MeasureCallCount++;
                MeasuredDuration = duration;
                MeasuredElement = element;
            }

            var update = new LayoutUpdate(LayoutPassType.Measure, duration, element ?? string.Empty, DateTime.UtcNow);

            lock (_historyLock)
            {
                _history.Add(update);
            }

            PublishLayoutUpdate(update);
        }

        public void RecordArrangePass(double duration, object element = null)
        {
            lock (_lock)
            {
                ArrangeCallCount++;
                ArrangedDuration = duration;
                ArrangedElement = element;
            }

            var update = new LayoutUpdate(LayoutPassType.Arrange, duration, element ?? string.Empty, DateTime.UtcNow);

            lock (_historyLock)
            {
                _history.Add(update);
            }

            PublishLayoutUpdate(update);
        }

        void PublishLayoutUpdate(LayoutUpdate update)
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