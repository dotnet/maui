using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using System;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.AppBar;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.Content.Res;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui
{
	public class MauiAppCompatActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			if (App.Current as MauiApp == null)
				throw new InvalidOperationException($"App is not {nameof(MauiApp)}");

			var mauiApp = (MauiApp)App.Current;

			if (mauiApp.Services == null)
				throw new InvalidOperationException("App was not initialized");

			var window = mauiApp.GetWindowFor(null!);

			window.MauiContext = new HandlersContext(mauiApp.Services, this);

			//Hack for now we set this on the App Static but this should be on IFrameworkElement
			App.Current.SetHandlerContext(window.MauiContext);

			var content = window.Page.View;

			CoordinatorLayout parent = new CoordinatorLayout(this);
			NestedScrollView main = new NestedScrollView(this);

			SetContentView(parent, new ViewGroup.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent));

			//AddToolbar(parent);

			parent.AddView(main, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

			main.AddView(content.ToNative(window.MauiContext), new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
		}

		void AddToolbar(ViewGroup parent)
		{	
			Toolbar toolbar = new Toolbar(this);
			var appbarLayout = new AppBarLayout(this);
			
			appbarLayout.AddView(toolbar, new ViewGroup.LayoutParams(AppBarLayout.LayoutParams.MatchParent, global::Android.Resource.Attribute.ActionBarSize));
			SetSupportActionBar(toolbar);
			parent.AddView(appbarLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
		}
	}
}
