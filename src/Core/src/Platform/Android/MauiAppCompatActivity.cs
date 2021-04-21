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
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var mauiApp = MauiApplication.Current.Application;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IApplication)} instance was not found.");

			var services = MauiApplication.Current.Services;
			if (mauiApp == null)
				throw new InvalidOperationException($"The {nameof(IServiceProvider)} instance was not found.");

			var mauiContext = new MauiContext(services, this);

			var state = new ActivationState(mauiContext, savedInstanceState);
			var window = mauiApp.CreateWindow(state);

			var matchParent = ViewGroup.LayoutParams.MatchParent;

			// Create the root native layout and set the Activity's content to it
			CoordinatorLayout nativeRootLayout = new CoordinatorLayout(this);
			SetContentView(nativeRootLayout, new ViewGroup.LayoutParams(matchParent, matchParent));

			//AddToolbar(parent);

			var page = window.View;

			// This currently relies on IPage : IView, which may not exactly be right
			// we may have to add another handler extension that works for Page
			// Also, AbstractViewHandler is set to work for IView (obviously); if IPage is not IView,
			// then we'll need to change it to AbstractFrameworkElementHandler or create a separate
			// abstract handler for IPage
			// TODO ezhart Think about all this stuff ^^

			var nativePage = page.ToNative(mauiContext);

			// Add the IPage to the root layout; use match parent so the page automatically has the dimensions of the activity
			nativeRootLayout.AddView(nativePage, new CoordinatorLayout.LayoutParams(matchParent, matchParent));
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
