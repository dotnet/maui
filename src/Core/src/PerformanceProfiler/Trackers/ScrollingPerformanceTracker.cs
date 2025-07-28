using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Performance
{
	/// <summary>
	/// Provides a generic implementation for tracking scrolling performance across platforms.
	/// </summary>
	internal class ScrollingPerformanceTracker : IScrollingPerformanceTracker
	{
		const double TargetFrameTimeMs = 16.67; // 60 FPS
		
		readonly ConcurrentBag<ScrollingUpdate> _history = new();
		readonly List<Action<ScrollingUpdate>> _callbacks = new();
		readonly object _lock = new();
		double _totalFrameTime;
		int _droppedFrames;
		double _totalScrollDuration;
		int _frameCount;

		/// <summary>
		/// Retrieves current scrolling performance statistics.
		/// </summary>
		/// <returns>A <see cref="ScrollingStats"/> object containing current metrics.</returns>
		public virtual ScrollingStats GetStats()
		{
			lock (_lock)
			{
				return new ScrollingStats
				{
					AverageFrameTime = _frameCount > 0 ? _totalFrameTime / _frameCount : 0,
					DroppedFrames = _droppedFrames,
					TotalScrollDuration = _totalScrollDuration,
					FrameCount = _frameCount
				};
			}
		}

		/// <summary>
		/// Retrieves the history of scrolling updates, optionally filtered by element.
		/// </summary>
		/// <param name="element">The optional element to filter the history by.</param>
		/// <returns>An enumerable collection of <see cref="ScrollingUpdate"/> records matching the filter.</returns>
		public virtual IEnumerable<ScrollingUpdate> GetHistory(object? element = null)
		{
			return element is null
				? _history
				: _history.Where(update => update.Element == element);
		}

		/// <summary>
		/// Subscribes a callback to receive real-time scrolling update notifications.
		/// </summary>
		/// <param name="callback">The callback to receive <see cref="ScrollingUpdate"/> data.</param>
		public virtual void SubscribeToScrollingUpdates(Action<ScrollingUpdate> callback)
		{
			lock (_lock)
			{
				_callbacks.Add(callback);
			}
		}

		/// <summary>
		/// Records a scrolling event with duration and optional element, updating metrics and notifying subscribers.
		/// </summary>
		/// <param name="duration">The duration of the scrolling event in milliseconds.</param>
		/// <param name="element">The optional element associated with the scrolling event.</param>
		public virtual void RecordScrollingTime(double duration, object? element = null)
		{
			var update = new ScrollingUpdate
			{
				FrameTime = duration,
				Timestamp = DateTime.UtcNow,
				Element = element,
				IsDroppedFrame = duration > TargetFrameTimeMs * 1.5 // Frame is dropped if 50% slower than target
			};

			lock (_lock)
			{
				_history.Add(update);
				_totalFrameTime += duration;
				_totalScrollDuration += duration;
				_frameCount++;
				
				if (update.IsDroppedFrame) 
					_droppedFrames++;

				foreach (var callback in _callbacks)
				{
					callback(update);
				}
			}
		}
	}
}