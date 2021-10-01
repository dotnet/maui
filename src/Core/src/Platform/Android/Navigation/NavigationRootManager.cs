using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class NavigationRootManager
	{
		AppBarLayout? _appBar;
		Toolbar? _toolbar;
		NavigationLayout? _navigationLayout;
		IMauiContext _mauiContext;

		internal NavigationLayout NavigationLayout => _navigationLayout ??=
			LayoutInflater
				.Inflate(Resource.Layout.navigationlayout, null)
				.JavaCast<NavigationLayout>()
				?? throw new InvalidOperationException($"Resource.Layout.navigationlayout missing");

		internal Toolbar Toolbar =>
			_toolbar ??= NavigationLayout.FindViewById<MaterialToolbar>(Resource.Id.navigationlayout_toolbar)
			?? throw new InvalidOperationException($"Resource.Id.navigationlayout_toolbar missing");

		internal AppBarLayout AppBar =>
			_appBar ??= NavigationLayout.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar)
			?? throw new InvalidOperationException($"Resource.Id.navigationlayout_appbar missing");

		LayoutInflater LayoutInflater => _mauiContext?.GetLayoutInflater()
			?? throw new InvalidOperationException($"LayoutInflater missing");

		internal FragmentManager FragmentManager => _mauiContext?.GetFragmentManager()
			?? throw new InvalidOperationException($"FragmentManager missing");

		public AView RootView => NavigationLayout;

		public NavigationRootManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		// TODO MAUI: this will eventually get replaced by Navigation
		internal virtual void SetContentView(AView view)
		{
			FragmentManager.BeginTransaction()
				.Replace(Resource.Id.navigationlayout_content, new ViewFragment(view))
				.SetReorderingAllowed(true)
				.Commit();
		}
	}
}
