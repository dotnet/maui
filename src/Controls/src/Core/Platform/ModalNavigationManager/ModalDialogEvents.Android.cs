#nullable enable
using Android.App;
using Android.Views;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Forwards the input events of the dialogs that host modal pages to the app, because a dialog has its own Android window and
	/// its events never reach the activity.
	/// </summary>
	internal static class ModalDialogEvents
	{
		public static bool OnKeyDown(Dialog dialog, Keycode keyCode, KeyEvent e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyDown>(del =>
			{
				handled = del(dialog, keyCode, e) || handled;
			});

			return handled;
		}

		public static bool OnKeyLongPress(Dialog dialog, Keycode keyCode, KeyEvent e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyLongPress>(del =>
			{
				handled = del(dialog, keyCode, e) || handled;
			});

			return handled;
		}

		public static bool OnKeyMultiple(Dialog dialog, Keycode keyCode, int repeatCount, KeyEvent e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyMultiple>(del =>
			{
				handled = del(dialog, keyCode, repeatCount, e) || handled;
			});

			return handled;
		}

		public static bool OnKeyShortcut(Dialog dialog, Keycode keyCode, KeyEvent e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyShortcut>(del =>
			{
				handled = del(dialog, keyCode, e) || handled;
			});

			return handled;
		}

		public static bool OnKeyUp(Dialog dialog, Keycode keyCode, KeyEvent e)
		{
			var handled = false;
			IPlatformApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnKeyUp>(del =>
			{
				handled = del(dialog, keyCode, e) || handled;
			});

			return handled;
		}

		/// <summary>
		/// Forwards a touch to the MAUI window so that HideSoftInputOnTappedChangedManager can detect taps and dismiss the keyboard
		/// when HideSoftInputOnTapped is enabled.
		/// </summary>
		public static bool DispatchTouchEvent(Dialog dialog, MotionEvent e) =>
			(dialog.Context.GetWindow() as IPlatformEventsListener)?.DispatchTouchEvent(e) == true;
	}
}
