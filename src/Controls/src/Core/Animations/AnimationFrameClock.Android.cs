#nullable enable
using System;
using Android.Views;

namespace Microsoft.Maui.Controls.Animations
{
	internal abstract partial class AnimationFrameClock
	{
		internal static partial AnimationFrameClock CreatePlatformClock() => new ChoreographerClock();

		// Choreographer keeps delivering frames when the animator duration scale is 0 or battery saver
		// disables animators, unlike a ValueAnimator-based ticker (Android spike, conditions C2 and C4b).
		sealed class ChoreographerClock : AnimationFrameClock
		{
			readonly FrameCallback _callback;
			long _lastFrameNanos;
			bool _running;
			bool _posted;

			public ChoreographerClock() => _callback = new FrameCallback(this);

			protected override TimeSpan Now
				=> TimeSpan.FromTicks((_running && _lastFrameNanos != 0 ? _lastFrameNanos : Java.Lang.JavaSystem.NanoTime()) / 100);

			protected override void StartFrames()
			{
				if (_running)
					return;

				_running = true;
				_lastFrameNanos = 0;
				Post();
			}

			protected override void StopFrames()
			{
				if (!_running)
					return;

				_running = false;
				if (_posted)
				{
					_posted = false;
					Choreographer.Instance?.RemoveFrameCallback(_callback);
				}
			}

			// Choreographer doesn't merge duplicate posts. A run that ends inside a frame and a new one that starts
			// from it would otherwise post twice and tick every run twice per frame from then on.
			void Post()
			{
				if (_posted)
					return;

				_posted = true;
				Choreographer.Instance?.PostFrameCallback(_callback);
			}

			void DoFrame(long frameTimeNanos)
			{
				_posted = false;
				if (!_running)
					return;

				_lastFrameNanos = frameTimeNanos;
				OnFrame();

				if (_running)
					Post();
			}

			sealed class FrameCallback : Java.Lang.Object, Choreographer.IFrameCallback
			{
				readonly WeakReference<ChoreographerClock> _clock;

				public FrameCallback(ChoreographerClock clock) => _clock = new WeakReference<ChoreographerClock>(clock);

				public void DoFrame(long frameTimeNanos)
				{
					if (_clock.TryGetTarget(out var clock))
						clock.DoFrame(frameTimeNanos);
				}
			}
		}
	}
}
