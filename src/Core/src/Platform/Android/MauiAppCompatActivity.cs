using System;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui
{
	public partial class MauiAppCompatActivity : AppCompatActivity
	{
		void MauiOnCreate(Bundle? savedInstanceState)
		{
			var mauiApp = MauiApplication.Current.Application;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			var services = MauiApplication.Current.Services;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			var mauiContext = new MauiContext(services, this);

			var state = new ActivationState(mauiContext, savedInstanceState);
			var window = mauiApp.CreateWindow(state);
			window.MauiContext = mauiContext;

			var content = (window.Page as IView) ?? window.Page.View;

			CoordinatorLayout parent = new CoordinatorLayout(this);

			SetContentView(parent, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

			//AddToolbar(parent);

			parent.AddView(content.ToNative(window.MauiContext), new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
		}

		void AddToolbar(ViewGroup parent)
		{
			Toolbar toolbar = new Toolbar(this);
			var appbarLayout = new AppBarLayout(this);

			appbarLayout.AddView(toolbar, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, Android.Resource.Attribute.ActionBarSize));
			SetSupportActionBar(toolbar);
			parent.AddView(appbarLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
		}
	}
}