using System;
using System.Runtime.CompilerServices;
using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Opens context flyouts when a confirm key (Enter, D-pad center, ...) is held on the focused view, which is
	/// how Android performs a long click from the keyboard.
	/// </summary>
	/// <remarks>
	/// Keys are observed on the focused view, and only when it (or an ancestor) has a context flyout. They aren't
	/// consumed, so the view still shows its pressed state and clicks when the key is released early. Once the
	/// menu opened, the key release doesn't click the view, as with native long clicks.
	/// </remarks>
	internal static class ContextFlyoutKeyTracker
	{
		static readonly ConditionalWeakTable<AView, object> s_observedRoots = new();

		static WeakReference<AView>? s_listenedView;
		static Java.Lang.Runnable? s_pendingLongPress;
		static Keycode s_keyCode;
		static bool s_consumeKeyUp;
		static KeyEvent? s_consumedKeyUp;

		/// <summary>
		/// Starts following the focus in the window of the given root view. Without context flyouts this only
		/// costs a check when the focus changes.
		/// </summary>
		internal static void Observe(AView? root)
		{
			if (root is null || s_observedRoots.TryGetValue(root, out _) || root.ViewTreeObserver is not ViewTreeObserver observer)
				return;

			s_observedRoots.Add(root, s_observedRoots);
			observer.GlobalFocusChange += OnGlobalFocusChange;
		}

		/// <summary>
		/// Follows the focused view again, e.g. after a context flyout was added to the focused view or an ancestor.
		/// </summary>
		internal static void Refresh(AView view)
		{
			if (view.IsAttachedToWindow && view.RootView?.FindFocus() is AView focused)
				UpdateListenedView(focused);
		}

		/// <summary>
		/// Whether the key release completed a long press that opened a context menu, so it shouldn't act as a click.
		/// </summary>
		internal static bool IsConsumedKeyUp(KeyEvent? e) =>
			e is not null && s_consumedKeyUp is KeyEvent consumed && consumed.IsAlive() &&
			consumed.KeyCode == e.KeyCode && consumed.EventTime == e.EventTime;

		static void OnGlobalFocusChange(object? sender, ViewTreeObserver.GlobalFocusChangeEventArgs e) =>
			UpdateListenedView(e.NewFocus);

		static void UpdateListenedView(AView? focused)
		{
			if (s_listenedView is not null && s_listenedView.TryGetTarget(out var listened))
			{
				if (listened == focused)
					return;

				if (listened.IsAlive())
					listened.KeyPress -= OnKeyPress;
			}

			s_listenedView = null;
			Cancel();

			if (focused is null || ContextFlyoutManager.FindForFocusedView(focused) is null)
				return;

			focused.KeyPress += OnKeyPress;
			s_listenedView = new WeakReference<AView>(focused);
		}

		static void OnKeyPress(object? sender, AView.KeyEventArgs e)
		{
			// The event is handled by default; the view must still receive the key
			e.Handled = false;

			if (sender is not AView view || e.Event is not KeyEvent keyEvent)
				return;

			if (keyEvent.Action == KeyEventActions.Up)
			{
				if (keyEvent.KeyCode != s_keyCode)
					return;

				Cancel();

				if (s_consumeKeyUp)
				{
					// Releasing the view before it handles the key keeps it from clicking
					s_consumeKeyUp = false;
					s_consumedKeyUp = keyEvent;
					view.Pressed = false;
					e.Handled = true;
				}

				return;
			}

			if (keyEvent.Action != KeyEventActions.Down || keyEvent.RepeatCount != 0)
				return;

			Cancel();
			s_consumeKeyUp = false;

			if (!keyEvent.KeyCode.IsConfirmKey() || !keyEvent.HasNoModifiers || ContextFlyoutManager.FindForFocusedView(view) is null)
				return;

			s_keyCode = keyEvent.KeyCode;
			s_pendingLongPress = new Java.Lang.Runnable(OnLongPress);
			view.PostDelayed(s_pendingLongPress, ViewConfiguration.LongPressTimeout);
		}

		static void OnLongPress()
		{
			// Don't dispose the runnable here: it is the one currently running
			s_pendingLongPress = null;

			if (s_listenedView is null || !s_listenedView.TryGetTarget(out var focused) || !focused.IsAlive() || !focused.HasFocus)
				return;

			// Look up the menu again: it may have been removed while the key was held
			if (ContextFlyoutManager.FindForFocusedView(focused) is { } manager && manager.ShowForKey(focused))
			{
				s_consumeKeyUp = true;
				manager.PerformHapticFeedback();
			}
		}

		static void Cancel()
		{
			if (s_pendingLongPress is null)
				return;

			if (s_listenedView is not null && s_listenedView.TryGetTarget(out var view) && view.IsAlive())
				view.RemoveCallbacks(s_pendingLongPress);

			s_pendingLongPress.Dispose();
			s_pendingLongPress = null;
		}
	}
}
