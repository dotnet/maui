#nullable enable

using System;
using Android.Runtime;
using Android.Views;

namespace Microsoft.Maui.Handlers
{
	internal partial class NavigationViewHandler :
		ViewHandler<INavigationView, NavigationLayout>
	{
		NavigationManager? _navigationManager;

		protected override NavigationLayout CreateNativeView()
		{
			LayoutInflater? li = MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var view = li.Inflate(Resource.Layout.navigationlayout, null).JavaCast<NavigationLayout>();
			_ = view ?? throw new InvalidOperationException($"Resource.Layout.navigationlayout view not found");

			return view;
		}

		// this should move to a factory method
		protected virtual NavigationManager CreateNavigationManager() =>
			_navigationManager ??= new NavigationManager();

		protected override void ConnectHandler(NavigationLayout nativeView)
		{
			_navigationManager = CreateNavigationManager();
			nativeView.NavigationManager = _navigationManager;
			base.ConnectHandler(nativeView);
			_navigationManager.Connect(VirtualView, nativeView);
		}

		public static void RequestNavigation(NavigationViewHandler arg1, INavigationView arg2, object? arg3)
		{
			if (arg3 is NavigationRequest ea)
				arg1._navigationManager?.RequestNavigation(ea);
		}
	}
}
