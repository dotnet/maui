using System;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity : AppCompatActivity
	{
		WeakReference<IWindow>? _virtualWindow;
		internal IWindow? VirtualWindow
		{
			get
			{
				IWindow? window = null;
				_virtualWindow?.TryGetTarget(out window);
				return window;
			}
		}


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

			CreateNativeWindow(savedInstanceState);

			//TODO MAUI
			// Allow users to customize the toolbarid?
			bool? windowActionBar;
			if (Theme.TryResolveAttribute(Resource.Attribute.windowActionBar, out windowActionBar) &&
				windowActionBar == false)
			{
				var toolbar = FindViewById<Toolbar>(Resource.Id.maui_toolbar);
				if (toolbar != null)
					SetSupportActionBar(toolbar);
			}
		}

		void CreateNativeWindow(Bundle? savedInstanceState = null)
		{
			var mauiApp = MauiApplication.Current.Application;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			var services = MauiApplication.Current.Services;
			if (services == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			var mauiContext = new MauiContext(services, this);

			services.InvokeLifecycleEvents<AndroidLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			// TODO: Fix once we have multiple windows
			IWindow window;
			if (mauiApp.Windows.Count > 0)
			{
				// assume if there are windows, then this is a "resume" activity
				window = mauiApp.Windows[0];
			}
			else
			{
				// there are no windows, so this is a fresh launch
				var state = new ActivationState(mauiContext, savedInstanceState);
				window = mauiApp.CreateWindow(state);
			}

			_virtualWindow = new WeakReference<IWindow>(window);
			this.SetWindow(window, mauiContext);
		}
	}
}
