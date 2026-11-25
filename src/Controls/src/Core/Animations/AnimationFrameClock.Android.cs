#nullable enable
using System;
using Microsoft.Maui.Dispatching;
using Android.Views;

namespace Microsoft.Maui.Controls.Animations
{
	internal abstract partial class AnimationFrameClock
	{
		static partial void CreatePlatformClockCore(IDispatcher? dispatcher, ref AnimationFrameClock? clock)
			=> clock = new ChoreographerFrameClock();

		/// <summary>
		/// <c>Choreographer</c> frame callbacks with <c>frameTimeNanos</c>. Unlike a <c>ValueAnimator</c> ticker,
		/// it keeps ticking at animator scale 0 ("Remove animations", battery saver), which is what lets
		/// <see cref="ReducedMotionBehavior.Play"/> work on Android.
		/// </summary>
		sealed class ChoreographerFrameClock : AnimationFrameClock
		{
			readonly FrameCallback _callback;
			bool _posted;
			bool _active;

			public ChoreographerFrameClock()
			{
				_callback = new FrameCallback(this);
			}

			protected override void StartFrames()
			{
				_active = true;
				Post();
			}

			protected override void StopFrames() => _active = false;

			// Choreographer doesn't merge duplicate posts: a run that ends inside a frame and a new one started
			// from it must not post twice, or every later frame ticks twice.
			void Post()
			{
				if (_posted)
					return;

				_posted = true;
				Choreographer.Instance?.PostFrameCallback(_callback);
			}

			void OnDoFrame(long frameTimeNanos)
			{
				_posted = false;
				if (!_active)
					return;

				OnFrame(TimeSpan.FromTicks(frameTimeNanos / 100));
				if (_active)
					Post();
			}

			sealed class FrameCallback : Java.Lang.Object, Choreographer.IFrameCallback
			{
				readonly WeakReference<ChoreographerFrameClock> _clock;

				public FrameCallback(ChoreographerFrameClock clock)
				{
					_clock = new WeakReference<ChoreographerFrameClock>(clock);
				}

				public void DoFrame(long frameTimeNanos)
				{
					if (_clock.TryGetTarget(out var clock))
						clock.OnDoFrame(frameTimeNanos);
				}
			}
		}
	}
}
