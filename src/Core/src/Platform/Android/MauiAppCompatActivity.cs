using System;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Navigation.UI;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			// If the theme has the maui_splash attribute, change the theme
			if (Theme.TryResolveAttribute(Resource.Attribute.maui_splash))
			{
				SetTheme(Resource.Style.Maui_MainTheme_NoActionBar);
			}

			base.OnCreate(savedInstanceState);

			var mauiApp = MauiApplication.Current.Application;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			var services = MauiApplication.Current.Services;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			MauiContext mauiContext;
			IWindow window;

			// TODO Fix once we have multiple windows
			if (mauiApp.Windows.Count > 0)
			{
				window = mauiApp.Windows[0];
				mauiContext = new MauiContext(services, this);
			}
			else
			{
				mauiContext = new MauiContext(services, this);
				ActivationState state = new ActivationState(mauiContext, savedInstanceState);
				window = mauiApp.CreateWindow(state);
			}

			SetContentView(window.View.ToNative(mauiContext));

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
