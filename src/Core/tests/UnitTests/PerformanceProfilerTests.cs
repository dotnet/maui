using System;
using System.Threading;
using Microsoft.Maui.Performance;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class PerformanceProfilerTests
	{
		/// <summary>
		/// Helper to reset the static Layout field to null between tests.
		/// </summary>
		void ResetLayout()
		{
			var prop = typeof(PerformanceProfiler).GetProperty(
				"Layout",
				System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			prop.SetValue(null, null);
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
		public void TrackMeasure_NotInitialized_DoesNotThrow()
		{
			// Arrange
			ResetLayout();

			// Act & Assert: Should not throw
			using (PerformanceProfiler.TrackMeasure("Test"))
			{ }
		}

		[Fact]
		public void TrackArrange_NotInitialized_DoesNotThrow()
		{
			// Arrange
			ResetLayout();

			// Act & Assert: Should not throw
			using (PerformanceProfiler.TrackArrange("Test"))
			{ }
		}

		[Fact]
		public void TrackMeasure_AfterInitialization_RecordsDurationAndElement()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			using (PerformanceProfiler.TrackMeasure("ElementA"))
			{
				Thread.Sleep(5);
			}

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
			Assert.True(tracker.MeasuredDuration >= 5);
			Assert.Equal("ElementA", tracker.MeasuredElement);
		}

		[Fact]
		public void TrackArrange_AfterInitialization_RecordsDurationAndElement()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			using (PerformanceProfiler.TrackArrange("ElementB"))
			{
				Thread.Sleep(7);
			}

			// Assert
			Assert.Equal(1, tracker.ArrangeCallCount);
			Assert.True(tracker.ArrangedDuration >= 7);
			Assert.Equal("ElementB", tracker.ArrangedElement);
		}

		[Fact]
		public void DoubleDispose_DoesNotRecordTwice()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Act
			var scope = PerformanceProfiler.TrackMeasure("X");
			System.Threading.Thread.Sleep(3);
			scope.Dispose();
			scope.Dispose(); // second dispose should be no-op

			// Assert
			Assert.Equal(1, tracker.MeasureCallCount);
		}

		[Fact]
		public void GetStats_AfterTracking_ReturnsLatestDurations()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Track measure
			using (PerformanceProfiler.TrackMeasure("M"))
			{ System.Threading.Thread.Sleep(4); }
			var statsAfterMeasure = PerformanceProfiler.GetStats();

			// Track arrange
			using (PerformanceProfiler.TrackArrange("A"))
			{ System.Threading.Thread.Sleep(6); }
			var statsAfterArrange = PerformanceProfiler.GetStats();

			// Assert measure stats
			Assert.True(statsAfterMeasure.Layout.MeasureDuration >= 4);
			Assert.Equal(0, statsAfterMeasure.Layout.ArrangeDuration);

			// Assert arrange stats
			Assert.True(statsAfterArrange.Layout.ArrangeDuration >= 6);
			Assert.True(statsAfterArrange.Layout.MeasureDuration >= 4);
		}

		[Fact]
		public void GetStats_BeforeAndAfterTracking_VerifyTimestampAndDurations()
		{
			// Arrange
			ResetLayout();
			var tracker = new FakeLayoutTracker();
			PerformanceProfiler.Initialize(tracker);

			// Before any tracking
			var beforeStats = PerformanceProfiler.GetStats();
			Assert.Equal(0, beforeStats.Layout.MeasureDuration);
			Assert.Equal(0, beforeStats.Layout.ArrangeDuration);
			Assert.True((DateTime.UtcNow - beforeStats.TimestampUtc).TotalSeconds < 5);

			// After measure tracking with sleep
			const int measureSleep = 20;
			using (PerformanceProfiler.TrackMeasure())
			{
				Thread.Sleep(measureSleep);
			}
			var afterMeasure = PerformanceProfiler.GetStats();
			Assert.InRange(afterMeasure.Layout.MeasureDuration, measureSleep, measureSleep * 3);
			Assert.Equal(0, afterMeasure.Layout.ArrangeDuration);
			Assert.True((DateTime.UtcNow - afterMeasure.TimestampUtc).TotalSeconds < 5);

			// After arrange tracking with sleep
			const int arrangeSleep = 30;
			using (PerformanceProfiler.TrackArrange())
			{
				Thread.Sleep(arrangeSleep);
			}
			var afterArrange = PerformanceProfiler.GetStats();
			Assert.InRange(afterArrange.Layout.ArrangeDuration, arrangeSleep, arrangeSleep * 3);
			Assert.True(afterArrange.Layout.MeasureDuration >= afterMeasure.Layout.MeasureDuration);
			Assert.True((DateTime.UtcNow - afterArrange.TimestampUtc).TotalSeconds < 5);
		}
	}

	internal class FakeLayoutTracker : ILayoutPerformanceTracker
	{
		public int MeasureCallCount { get; private set; }
		public double MeasuredDuration { get; private set; }
		public string MeasuredElement { get; private set; }

		public int ArrangeCallCount { get; private set; }
		public double ArrangedDuration { get; private set; }
		public string ArrangedElement { get; private set; }

		public LayoutStats GetStats()
		{
			return new LayoutStats
			{
				MeasureDuration = MeasuredDuration,
				ArrangeDuration = ArrangedDuration
			};
		}

		public void RecordMeasurePass(double duration, string element = null)
		{
			MeasureCallCount++;
			MeasuredDuration = duration;
			MeasuredElement = element;
		}

		public void RecordArrangePass(double duration, string element = null)
		{
			ArrangeCallCount++;
			ArrangedDuration = duration;
			ArrangedElement = element;
		}
	}
}
