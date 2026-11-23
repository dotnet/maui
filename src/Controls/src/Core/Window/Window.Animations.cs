#nullable enable
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		AnimationFrameClock? _animationClock;
		bool _animationClockLifecycleHooked;

		/// <summary>
		/// The window's animation clock, created on first use. It is suspended while the window is stopped, resumed
		/// when it resumes or activates, and dropped on <see cref="Destroying"/>: Android can hand the same
		/// <see cref="Window"/> to a recreated Activity, which then gets a fresh clock on its next run.
		/// </summary>
		internal AnimationFrameClock GetAnimationClock()
		{
			if (_animationClock is { IsDestroyed: false } clock)
				return clock;

			if (!_animationClockLifecycleHooked)
			{
				_animationClockLifecycleHooked = true;
				Stopped += (_, _) => _animationClock?.Suspend();
				Resumed += (_, _) => _animationClock?.Resume();
				Activated += (_, _) => _animationClock?.Resume();
				Destroying += (_, _) =>
				{
					var destroyed = _animationClock;
					_animationClock = null;
					destroyed?.Destroy();
				};
			}

			clock = AnimationFrameClock.Create(this.TryFindDispatcher());
			_animationClock = clock;
			return clock;
		}
	}
}
