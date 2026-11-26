#nullable enable
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		AnimationFrameClock? _animationClock;
		bool _animationsSuspended;

		/// <summary>
		/// The window's animation clock, created on first use. The window's own lifecycle suspends it while the window is
		/// stopped and resumes it when the window resumes or activates; that state is tracked before any clock exists, so
		/// a clock created while the window is stopped starts suspended. The clock is dropped on Destroying: Android can
		/// hand the same <see cref="Window"/> to a recreated Activity, which then gets a fresh clock on its next run.
		/// </summary>
		internal AnimationFrameClock? GetAnimationClock()
		{
			// A destroyed window has no clock: runs started from its teardown use the application clock (finite) or wait
			// for a window (forever). Created resets IsDestroyed when Android hands the window to a new Activity.
			if (IsDestroyed)
				return null;

			if (_animationClock is { IsDestroyed: false } clock)
				return clock;

			clock = AnimationFrameClock.Create(this.TryFindDispatcher());
			if (_animationsSuspended)
				clock.Suspend();

			_animationClock = clock;
			return clock;
		}

		void SuspendAnimations()
		{
			_animationsSuspended = true;
			_animationClock?.Suspend();
		}

		void ResumeAnimations()
		{
			_animationsSuspended = false;
			_animationClock?.Resume();
		}

		void DestroyAnimationClock()
		{
			var destroyed = _animationClock;
			_animationClock = null;
			destroyed?.Destroy();
		}
	}
}
