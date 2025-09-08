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

		private GlobalWindowInsetListener? _globalWindowInsetListener;

		/// <summary>
		/// Gets the shared GlobalWindowInsetListener instance for this activity.
		/// This ensures all views use the same listener instance for coordinated inset management.
		/// </summary>
		public GlobalWindowInsetListener GlobalWindowInsetListener =>
			_globalWindowInsetListener ??= new GlobalWindowInsetListener();

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

		protected override void OnDestroy()
		{
			_globalWindowInsetListener?.Dispose();
			_globalWindowInsetListener = null;
			base.OnDestroy();
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
	}
}