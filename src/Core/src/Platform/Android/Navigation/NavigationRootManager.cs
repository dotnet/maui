using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public class NavigationRootManager
	{
		CoordinatorLayout? _navigationLayout;
		IMauiContext _mauiContext;

		internal CoordinatorLayout NavigationLayout => _navigationLayout ??=
			LayoutInflater
				.Inflate(Resource.Layout.navigationlayout, null)
				.JavaCast<CoordinatorLayout>()
				?? throw new InvalidOperationException($"Resource.Layout.navigationlayout missing");

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
