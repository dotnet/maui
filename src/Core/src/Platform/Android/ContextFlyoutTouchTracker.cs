using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Android.OS;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Observes the touches a window dispatches and opens context flyouts on long press or secondary click.
	/// </summary>
	/// <remarks>
	/// It runs before the window dispatches each event and never consumes events unless a menu was opened,
	/// so views keep their regular touch behavior. When a menu opens, the in-flight touch is cancelled
	/// (so the pressed view doesn't click, select or scroll on release) and the rest of it is swallowed.
	/// Each window tracks its own touch stream, as windows can receive touches at the same time.
	/// </remarks>
	internal static class ContextFlyoutTouchTracker
	{
		static readonly ConditionalWeakTable<Window, TouchState> s_states = new();

		// Keeps states (and their windows) alive while they track a touch, and keeps the fast path cheap
		static readonly HashSet<TouchState> s_activeStates = new();
		static int s_touchSlop;

		/// <returns><see langword="true"/> when the event was handled and must not be dispatched.</returns>
		internal static bool OnDispatchTouchEvent(Window? window, MotionEvent? e)
		{
			if (e is null || window is null)
				return false;

			if (s_activeStates.Count == 0 && !ContextFlyoutManager.HasAny)
				return false;

			if (!s_states.TryGetValue(window, out var state))
			{
				if (e.ActionMasked != MotionEventActions.Down || !ContextFlyoutManager.HasAny)
					return false;

				state = new TouchState(window);
				s_states.Add(window, state);
			}

			return state.OnDispatchTouchEvent(e);
		}

		sealed class TouchState
		{
			// Held strongly: the .NET peer of a window can otherwise be collected while a touch is tracked.
			// Active states are rooted by s_activeStates, inactive ones only live as long as their window.
			readonly Window _window;
			WeakReference<ContextFlyoutManager>? _candidate;
			Java.Lang.Runnable? _pendingLongPress;
			float _downX;
			float _downY;
			long _downTime;
			bool _swallowing;
			DetachListener? _detachListener;
			bool _dispatchingCancel;
			bool _active;

			public TouchState(Window window) =>
				_window = window;

			public bool OnDispatchTouchEvent(MotionEvent e)
			{
				if (_dispatchingCancel)
					return false;

				switch (e.ActionMasked)
				{
					case MotionEventActions.Down:
						Reset();
						return OnDown(e);

					case MotionEventActions.Move:
						if (_swallowing)
							return true;

						if (_candidate is not null && IsBeyondSlop(e))
							Reset();

						return false;

					case MotionEventActions.Up:
					case MotionEventActions.Cancel:
						var swallow = _swallowing;
						Reset();
						return swallow;

					default:
						// A second finger means this isn't a long press
						if (!_swallowing)
						{
							Reset();
							return false;
						}

						return true;
				}
			}

			bool OnDown(MotionEvent e)
			{
				if (!ContextFlyoutManager.HasAny || !_window.IsAlive() || _window.DecorView is not AView root)
					return false;

				var manager = ContextFlyoutManager.FindTarget(root, e.GetX(), e.GetY());
				if (manager is null)
					return false;

				if (e.IsFromSource(InputSourceType.Mouse) && (e.ButtonState & MotionEventButtonState.Secondary) != 0)
				{
					// Secondary click opens the menu right away; the view never sees the press.
					// On text it's the text's own context menu, with the flyout items added.
					float windowX = e.GetX(), windowY = e.GetY();
					if (ShowTextContextMenu(root, manager, windowX, windowY) || manager.Show(windowX, windowY))
					{
						SetActive(true);
						_swallowing = true;
						return true;
					}

					return false;
				}

				// The pressed view's own long-press handler (a gesture, a long-click listener) comes first, like natively
				if (manager.DefersLongPress(root, e.GetX(), e.GetY()))
					return false;

				SetActive(true);
				_candidate = new WeakReference<ContextFlyoutManager>(manager);
				_downX = e.GetX();
				_downY = e.GetY();
				_downTime = e.DownTime;

				if (s_touchSlop == 0 && root.Context is not null)
					s_touchSlop = ViewConfiguration.Get(root.Context)?.ScaledTouchSlop ?? 0;

				// Gesture detectors schedule their long press for the same time. This one is scheduled first, so it
				// runs first and cancels theirs when the menu opens, like a native long click takes the touch over.
				_pendingLongPress = new Java.Lang.Runnable(OnLongPress);
				if (root.Handler is not global::Android.OS.Handler handler ||
					!handler.PostAtTime(_pendingLongPress, e.DownTime + ViewConfiguration.LongPressTimeout))
					root.PostDelayed(_pendingLongPress, ViewConfiguration.LongPressTimeout);

				return false;
			}

			void OnLongPress()
			{
				// Don't dispose the runnable here: it is the one currently running
				_pendingLongPress = null;

				if (_candidate is null || !_candidate.TryGetTarget(out var manager) || !_window.IsAlive() || !manager.Show(_downX, _downY))
				{
					Reset();
					return;
				}

				manager.PerformHapticFeedback();
				_swallowing = true;

				// Cancel the touch already delivered so the pressed view doesn't click, select or scroll on release.
				// SuperDispatchTouchEvent goes straight to the views, without coming back through the activity.
				using var cancel = MotionEvent.Obtain(_downTime, SystemClock.UptimeMillis(), MotionEventActions.Cancel, _downX, _downY, 0);
				if (cancel is null)
					return;

				_dispatchingCancel = true;
				try
				{
					_window.SuperDispatchTouchEvent(cancel);

					// Views that saw the press without taking the touch don't get that cancel, but they may be waiting
					// for the long press too. Natively they wouldn't have seen the press, as the long-clickable view takes it.
					if (_window.IsAlive() && _window.DecorView is AView root)
						ContextFlyoutManager.CancelLongPresses(root, _downX, _downY, cancel);
				}
				finally
				{
					_dispatchingCancel = false;
				}
			}

			static bool ShowTextContextMenu(AView root, ContextFlyoutManager manager, float x, float y) =>
				manager.FindTouchTarget(root, x, y) is AView target &&
				ContextFlyoutTextMenus.IsText(target) &&
				ContextFlyoutManager.TryMapToLocal(root, target, x, y, out var localX, out var localY) &&
				ContextFlyoutTextMenus.ShowContextMenu(target, localX, localY, () => manager.Show(x, y));

			bool IsBeyondSlop(MotionEvent e)
			{
				var dx = e.GetX() - _downX;
				var dy = e.GetY() - _downY;
				return (dx * dx) + (dy * dy) > (s_touchSlop * s_touchSlop);
			}

			void CancelPendingLongPress()
			{
				if (_pendingLongPress is not null)
				{
					if (_window.IsAlive() && _window.DecorView is AView decor)
					{
						decor.Handler?.RemoveCallbacks(_pendingLongPress);
						decor.RemoveCallbacks(_pendingLongPress);
					}

					_pendingLongPress.Dispose();
					_pendingLongPress = null;
				}

				_candidate = null;
			}

			void Reset()
			{
				CancelPendingLongPress();
				_swallowing = false;
				SetActive(false);
			}

			void SetActive(bool active)
			{
				if (_active == active)
					return;

				_active = active;

				if (active)
				{
					s_activeStates.Add(this);

					// A window that goes away during a touch never ends it, which would keep it alive
					if (_detachListener is null && _window.IsAlive() && _window.DecorView is AView decor)
					{
						_detachListener = new DetachListener(this);
						decor.AddOnAttachStateChangeListener(_detachListener);
					}
				}
				else
				{
					s_activeStates.Remove(this);
				}
			}

			internal void OnDetached() => Reset();
		}

		sealed class DetachListener : Java.Lang.Object, AView.IOnAttachStateChangeListener
		{
			readonly WeakReference<TouchState> _state;

			public DetachListener(TouchState state) =>
				_state = new WeakReference<TouchState>(state);

			public void OnViewAttachedToWindow(AView? attachedView)
			{
			}

			public void OnViewDetachedFromWindow(AView? detachedView)
			{
				if (_state.TryGetTarget(out var state))
					state.OnDetached();
			}
		}
	}
}
