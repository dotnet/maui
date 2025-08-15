using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content.Resources;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity : AppCompatActivity
	{
		// Override this if you want to handle the default Android behavior of restoring fragments on an application restart
		protected virtual bool AllowFragmentRestore => false;

		protected override void OnCreate(Bundle? savedInstanceState)
		{
			Microsoft.Maui.PlatformMauiAppCompatActivity.OnCreate(
				this,
				savedInstanceState,
				AllowFragmentRestore,
				Resource.Attribute.maui_splash,
				Resource.Style.Maui_MainTheme_NoActionBar);

			base.OnCreate(savedInstanceState);

			if (IPlatformApplication.Current?.Application is not null)
			{
				this.CreatePlatformWindow(IPlatformApplication.Current.Application, savedInstanceState);
			}
		}

		public override bool DispatchTouchEvent(MotionEvent? e)
		{
			// For current purposes this needs to get called before we propagate
			// this message out. In Controls this dispatch call will unfocus the 
			// current focused element which is important for timing if we should
			// hide/show the softkeyboard.
			// If you move this to after the xplat call then the keyboard will show up
			// then close
			bool handled = base.DispatchTouchEvent(e);

			bool implHandled =
				(this.GetWindow() as IPlatformEventsListener)?.DispatchTouchEvent(e) == true;

			return handled || implHandled;
		}

		// Override key methods to support modal dialog forwarding
		// When _processingKeyEvent is true, we don't call base to avoid side effects
		public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
		{
			if (!_processingKeyEvent)
				return base.OnKeyDown(keyCode, e);

			return true;
		}

		public override bool OnKeyUp(Keycode keyCode, KeyEvent? e)
		{
			if (!_processingKeyEvent)
				return base.OnKeyUp(keyCode, e);

			return true;
		}

		public override bool OnKeyLongPress(Keycode keyCode, KeyEvent? e)
		{
			if (!_processingKeyEvent)
				return base.OnKeyLongPress(keyCode, e);

			return true;
		}

		public override bool OnKeyMultiple(Keycode keyCode, int repeatCount, KeyEvent? e)
		{
			if (!_processingKeyEvent)
				return base.OnKeyMultiple(keyCode, repeatCount, e);

			return true;
		}

		public override bool OnKeyShortcut(Keycode keyCode, KeyEvent? e)
		{
			if (!_processingKeyEvent)
				return base.OnKeyShortcut(keyCode, e);

			return true;
		}

		// Internal methods for key event forwarding from modal dialogs
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal bool InternalOnKeyDown(Keycode keyCode, KeyEvent? e)
		{
			if (_processingKeyEvent)
				return false;

			_processingKeyEvent = true;
			try
			{
				return OnKeyDown(keyCode, e);
			}
			finally
			{
				_processingKeyEvent = false;
			}
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal bool InternalOnKeyUp(Keycode keyCode, KeyEvent? e)
		{
			if (_processingKeyEvent)
				return false;

			_processingKeyEvent = true;
			try
			{
				return OnKeyUp(keyCode, e);
			}
			finally
			{
				_processingKeyEvent = false;
			}
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal bool InternalOnKeyLongPress(Keycode keyCode, KeyEvent? e)
		{
			if (_processingKeyEvent)
				return false;

			_processingKeyEvent = true;
			try
			{
				return OnKeyLongPress(keyCode, e);
			}
			finally
			{
				_processingKeyEvent = false;
			}
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal bool InternalOnKeyMultiple(Keycode keyCode, int repeatCount, KeyEvent? e)
		{
			if (_processingKeyEvent)
				return false;

			_processingKeyEvent = true;
			try
			{
				return OnKeyMultiple(keyCode, repeatCount, e);
			}
			finally
			{
				_processingKeyEvent = false;
			}
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal bool InternalOnKeyShortcut(Keycode keyCode, KeyEvent? e)
		{
			if (_processingKeyEvent)
				return false;

			_processingKeyEvent = true;
			try
			{
				return OnKeyShortcut(keyCode, e);
			}
			finally
			{
				_processingKeyEvent = false;
			}
		}

		private bool _processingKeyEvent;
	}
}