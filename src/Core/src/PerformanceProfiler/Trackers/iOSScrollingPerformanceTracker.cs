#if IOS
using System;
using CoreAnimation;

namespace Microsoft.Maui.Performance
{

	/// <summary>
	/// Provides an iOS-specific implementation for tracking scrolling performance using CADisplayLink.
	/// </summary>
	internal class iOSScrollingPerformanceTracker : ScrollingPerformanceTracker
	{
		readonly CADisplayLink? _displayLink;
		DateTime _lastFrameTime;

		/// <summary>
		/// Initializes a new instance of the <see cref="iOSScrollingPerformanceTracker"/> class, setting up CADisplayLink.
		/// </summary>
		public iOSScrollingPerformanceTracker()
		{
			_displayLink = CADisplayLink.Create(() =>
			{
				var currentTime = DateTime.UtcNow;
				if (_lastFrameTime != default)
				{
					var duration = (currentTime - _lastFrameTime).TotalMilliseconds;
					RecordScrollingTime(duration);
				}

				_lastFrameTime = currentTime;
			});
			_displayLink.AddToRunLoop(Foundation.NSRunLoop.Main, Foundation.NSRunLoopMode.Common);
			_displayLink.Paused = true;
		}

		/// <summary>
		/// Records scrolling time only when CADisplayLink is active.
		/// </summary>
		/// <param name="duration">The duration of the scrolling event in milliseconds.</param>
		/// <param name="element">The optional element associated with the scrolling event.</param>
		public override void RecordScrollingTime(double duration, object? element = null)
		{
			if (_displayLink is not null && !_displayLink.Paused)
			{
				base.RecordScrollingTime(duration, element);
			}
		}

		/// <summary>
		/// Subscribes to scrolling updates and enables CADisplayLink for active tracking.
		/// </summary>
		/// <param name="callback">The callback to receive <see cref="ScrollingUpdate"/> data.</param>
		public override void SubscribeToScrollingUpdates(Action<ScrollingUpdate> callback)
		{
			base.SubscribeToScrollingUpdates(callback);
			
			if (_displayLink is not null)
			{
				_displayLink.Paused = false;
			}
		}
	}
}
#endif