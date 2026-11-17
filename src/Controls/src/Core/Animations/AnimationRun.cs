#nullable enable
using System;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>Clock plumbing shared by visual-state transitions and storyboard players.</summary>
	internal abstract class AnimationRun : IAnimationTrackOwner
	{
		IStoryboardClock? _clock;
		bool _ticking;

		protected IStoryboardClock? Clock => _clock;

		protected bool IsTicking => _ticking;

		protected void StartTicking(IStoryboardClock clock)
		{
			if (_ticking)
				return;

			_clock = clock;
			_ticking = true;
			clock.Tick += OnClockTick;
			if (clock is AnimationFrameClock frameClock)
				frameClock.Destroyed += OnClockDestroyed;
		}

		protected void StopTicking()
		{
			if (!_ticking || _clock is null)
				return;

			_ticking = false;
			_clock.Tick -= OnClockTick;
			if (_clock is AnimationFrameClock frameClock)
				frameClock.Destroyed -= OnClockDestroyed;
		}

		void OnClockTick(object? sender, EventArgs e)
		{
			// A run removed during this frame can still be in the frame's invocation list.
			if (_ticking)
				OnTick();
		}

		void OnClockDestroyed(object? sender, EventArgs e) => OnWindowDestroyed();

		protected abstract void OnTick();

		protected abstract void OnWindowDestroyed();

		public abstract void OnTrackLost(BindableObject target, BindableProperty property, TrackLossReason reason);
	}
}
