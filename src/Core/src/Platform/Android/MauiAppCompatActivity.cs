using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
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
			if (!AllowFragmentRestore)
			{
				// Remove the automatically persisted fragment structure; we don't need them
				// because we're rebuilding everything from scratch. This saves a bit of memory
				// and prevents loading errors from child fragment managers
				savedInstanceState?.Remove("android:support:fragments");
				savedInstanceState?.Remove("androidx.lifecycle.BundlableSavedStateRegistry.key");
			}

			// If the theme has the maui_splash attribute, change the theme
			if (Theme.TryResolveAttribute(Resource.Attribute.maui_splash))
			{
				SetTheme(Resource.Style.Maui_MainTheme_NoActionBar);
			}

			base.OnCreate(savedInstanceState);

			this.CreatePlatformWindow(MauiApplication.Current.Application, savedInstanceState);
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