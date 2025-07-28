#if ANDROID
using AndroidX.Core.App;

namespace Microsoft.Maui.Performance
{
	/// <summary>
	/// Provides an Android-specific implementation for tracking scrolling performance using FrameMetricsAggregator.
	/// </summary>
	internal class AndroidScrollingPerformanceTracker : ScrollingPerformanceTracker
	{
		private readonly FrameMetricsAggregator _frameMetrics;

		/// <summary>
		/// Initializes a new instance of the <see cref="AndroidScrollingPerformanceTracker"/> class.
		/// </summary>
		public AndroidScrollingPerformanceTracker()
		{
			_frameMetrics = new FrameMetricsAggregator();
		}

		/// <summary>
		/// Records scrolling time using FrameMetricsAggregator for precise measurements, falling back to base implementation if metrics are unavailable.
		/// </summary>
		/// <param name="duration">The duration of the scrolling event in milliseconds.</param>
		/// <param name="element">The optional element associated with the scrolling event.</param>
		public override void RecordScrollingTime(double duration, object? element = null)
		{
			var metrics = _frameMetrics.GetMetrics(); // Get all metrics
			var totalDurationMetrics = metrics?[FrameMetricsAggregator.TotalDuration];
		
			if (totalDurationMetrics != null)
			{
				long totalDurationNanos = 0;
				for (int i = 0; i < totalDurationMetrics.Size(); i++)
				{
					totalDurationNanos += totalDurationMetrics.ValueAt(i);
				}
				double durationMs = totalDurationNanos / 1_000_000.0; // Convert to milliseconds
				base.RecordScrollingTime(durationMs, element);
			}
			else
			{
				base.RecordScrollingTime(duration, element);
			}
		}
	}
}
#endif