using Android.OS;
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

			this.CreateNativeWindow(MauiApplication.Current.Application, savedInstanceState);

			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnCreate>(del => del(this, savedInstanceState));
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			var window = this.GetWindow();

			if (window is not null)
				window.Handler?.Invoke(nameof(IApplication.OnWindowClosed), window);
			
			MauiApplication.Current?.Services?.InvokeLifecycleEvents<AndroidLifecycle.OnDestroy>(del => del(this));
		}
	}
}