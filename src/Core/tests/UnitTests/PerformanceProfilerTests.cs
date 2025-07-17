using System;
using System.Threading;
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
			Thread.Sleep(5);
			perfTracker.Stop();

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration >= 5, $"Expected duration >= 5ms, but got {tracker.MeasuredDuration}ms");
			Assert.Equal("ElementA", tracker.MeasuredElement);
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
			Thread.Sleep(7);
			perfTracker.Stop();

			// Assert
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.True(tracker.ArrangedDuration >= 7, $"Expected duration >= 7ms, but got {tracker.ArrangedDuration}ms");
			Assert.Equal("ElementB", tracker.ArrangedElement);
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
			Thread.Sleep(4);
			measureTracker.Stop();
			var statsAfterMeasure = PerformanceProfiler.GetStats();

			// Track arrange
			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "A");
			Thread.Sleep(6);
			arrangeTracker.Stop();
			var statsAfterArrange = PerformanceProfiler.GetStats();

			// Assert measure stats
			Assert.True(statsAfterMeasure.Layout.MeasureDuration >= 4, $"Expected measure duration >= 4ms, but got {statsAfterMeasure.Layout.MeasureDuration}ms");
			Assert.Equal(0, statsAfterMeasure.Layout.ArrangeDuration);

			// Assert arrange stats
			Assert.True(statsAfterArrange.Layout.ArrangeDuration >= 6, $"Expected arrange duration >= 6ms, but got {statsAfterArrange.Layout.ArrangeDuration}ms");
			Assert.True(statsAfterArrange.Layout.MeasureDuration >= 4, $"Expected measure duration >= 4ms, but got {statsAfterArrange.Layout.MeasureDuration}ms");
		}

		[Fact]
		public void GetStats_BeforeAndAfterTracking_VerifyTimestampAndDurations()
		{
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			var beforeStats = PerformanceProfiler.GetStats();
			Assert.Equal(0, beforeStats.Layout.MeasureDuration);
			Assert.Equal(0, beforeStats.Layout.ArrangeDuration);
			Assert.True((DateTime.UtcNow - beforeStats.TimestampUtc).TotalSeconds < 5);

			// Track measure
			const int measureSleep = 20;
			var measureTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure);
			Thread.Sleep(measureSleep);
			measureTracker.Stop();

			SpinWait.SpinUntil(() => tracker.MeasureCallCount == 1, TimeSpan.FromMilliseconds(200));
			var afterMeasure = PerformanceProfiler.GetStats();

			// Assert measure stats
			Assert.InRange(afterMeasure.Layout.MeasureDuration, measureSleep, measureSleep * 3);
			Assert.Equal(0, afterMeasure.Layout.ArrangeDuration);
			Assert.True((DateTime.UtcNow - afterMeasure.TimestampUtc).TotalSeconds < 5);

			// Track arrange
			const int arrangeSleep = 30;
			var arrangeTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange);
			Thread.Sleep(arrangeSleep);
			arrangeTracker.Stop();

			SpinWait.SpinUntil(() => tracker.ArrangeCallCount == 1, TimeSpan.FromMilliseconds(200));
			var afterArrange = PerformanceProfiler.GetStats();

			// Assert arrange stats
			Assert.InRange(afterArrange.Layout.ArrangeDuration, arrangeSleep, arrangeSleep * 3);
			Assert.True(afterArrange.Layout.MeasureDuration >= afterMeasure.Layout.MeasureDuration,
				$"Expected measure duration >= {afterMeasure.Layout.MeasureDuration}ms, but got {afterArrange.Layout.MeasureDuration}ms");
			Assert.True((DateTime.UtcNow - afterArrange.TimestampUtc).TotalSeconds < 5);
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
			var tracker2 = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Element2");
			Thread.Sleep(5);
			tracker1.Stop();
			Thread.Sleep(5);
			tracker2.Stop();

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.Equal("Element1", tracker.MeasuredElement);
			Assert.True(tracker.MeasuredDuration >= 5, $"Expected measure duration >= 5ms, but got {tracker.MeasuredDuration}ms");
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.Equal("Element2", tracker.ArrangedElement);
			Assert.True(tracker.ArrangedDuration >= 10, $"Expected arrange duration >= 10ms, but got {tracker.ArrangedDuration}ms");
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
			Thread.Sleep(5);
			perfTracker.Stop();

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration >= 5, $"Expected duration >= 5ms, but got {tracker.MeasuredDuration}ms");
			Assert.Null(tracker.MeasuredElement);
		}

		[Fact]
		public void Start_LayoutMeasure_WithLongDelay_RecordsAccurateDuration()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const int expectedDelayMs = 500;
			const int toleranceMs = 100;

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutMeasure, "LongDelayedElement");
			Thread.Sleep(expectedDelayMs);
			perfTracker.Stop();

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.InRange(tracker.MeasuredDuration, expectedDelayMs, expectedDelayMs + toleranceMs);
			Assert.Equal("LongDelayedElement", tracker.MeasuredElement);
		}

		[Fact]
		public void Start_LayoutArrange_WithLongDelay_RecordsAccurateDuration()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);
			const int expectedDelayMs = 500;
			const int toleranceMs = 100;

			// Act
			var perfTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "LongDelayedElement");
			Thread.Sleep(expectedDelayMs);
			perfTracker.Stop();

			// Wait for tracking to register
			bool success = SpinWait.SpinUntil(() => tracker.ArrangeCallCount == 1, TimeSpan.FromMilliseconds(200));
			Assert.True(success, "ArrangeCallCount did not reach 1 in expected time.");

			// Assert
			Assert.InRange(tracker.ArrangedDuration, expectedDelayMs, expectedDelayMs + toleranceMs);
			Assert.Equal("LongDelayedElement", tracker.ArrangedElement);
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
				Thread.Sleep(10);
				// perfTracker.Dispose() is called automatically here
			}

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration >= 10, $"Expected duration >= 10ms, but got {tracker.MeasuredDuration}ms");
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
				Thread.Sleep(15);
				// perfTracker.Dispose() is called automatically here
			}

			// Assert
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.True(tracker.ArrangedDuration >= 15, $"Expected duration >= 15ms, but got {tracker.ArrangedDuration}ms");
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
				Thread.Sleep(5);
				throw new InvalidOperationException("Test exception");
			}
			catch (InvalidOperationException)
			{
				// Expected exception
			}

			// Assert that tracking still occurred despite the exception
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration >= 5, $"Expected duration >= 5ms, but got {tracker.MeasuredDuration}ms");
			Assert.Equal("ExceptionElement", tracker.MeasuredElement);
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
				Thread.Sleep(5);
				
				{
					using var innerTracker = PerformanceProfiler.Start(PerformanceCategory.LayoutArrange, "Inner");
					Thread.Sleep(5);
					// innerTracker.Dispose() called here
				}
				
				Thread.Sleep(5);
				// outerTracker.Dispose() called here
			}

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration >= 15, $"Expected outer duration >= 15ms, but got {tracker.MeasuredDuration}ms");
			Assert.Equal("Outer", tracker.MeasuredElement);
			
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.True(tracker.ArrangedDuration >= 5, $"Expected inner duration >= 5ms, but got {tracker.ArrangedDuration}ms");
			Assert.Equal("Inner", tracker.ArrangedElement);
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
	}

	internal class FakeLayoutTracker : ILayoutPerformanceTracker
	{
		readonly object _lock = new object();
		
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
				return new LayoutStats
				{
					MeasureDuration = MeasuredDuration,
					ArrangeDuration = ArrangedDuration
				};
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
		}

		public void RecordArrangePass(double duration, object element = null)
		{
			lock (_lock)
			{
				ArrangeCallCount++;
				ArrangedDuration = duration;
				ArrangedElement = element;
			}
		}
	}
}