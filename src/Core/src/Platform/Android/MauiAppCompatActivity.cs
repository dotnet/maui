using System;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity : AppCompatActivity
	{
		internal const string MauiAppCompatActivity_WindowId = "MauiAppCompatActivity_WindowId";
		// Override this if you want to handle the default Android behavior of restoring fragments on an application restart
		protected virtual bool AllowFragmentRestore => false;
		IWindow? _window;

		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);

			if (_window != null)
				outState.PutString(MauiAppCompatActivity_WindowId, _window?.Id);
		}

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

			var services = MauiApplication.Current.Services;

			if (services == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			var mauiApp = services.GetRequiredService<IApplication>();

			MauiContext mauiContext;

			var windowScope = services.CreateScope();
			var windowServices = windowScope.ServiceProvider;

			// create the maui context for this instance
			mauiContext = new MauiContext(windowServices, this);

			var state = new ActivationState(mauiContext, savedInstanceState);
			var args = windowServices.GetRequiredService<WindowCreatingArgs>();
			var windowFactory = windowServices.GetRequiredService<IWindowFactory>();
			args.ActivationState = state;
			args.ServiceProvider = windowServices;
			args.Application = mauiApp;

			_window = windowFactory.GetOrCreateWindow(args);
			
			SetContentView(_window.View.ToContainerView(mauiContext));

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
	}
}
