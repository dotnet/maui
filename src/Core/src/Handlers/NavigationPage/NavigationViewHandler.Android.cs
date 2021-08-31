#nullable enable

using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	internal partial class NavigationViewHandler :
		ViewHandler<INavigationView, NavigationLayout>
	{
		protected override NavigationLayout CreateNativeView()
		{
			LayoutInflater? li = LayoutInflater.From(Context);
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var view = li.Inflate(Resource.Layout.navigationlayout, null).JavaCast<NavigationLayout>();
			_ = view ?? throw new InvalidOperationException($"Resource.Layout.navigationlayout view not found");

			return view;
		}

		protected override void ConnectHandler(NavigationLayout nativeView)
		{
			NativeView.SetVirtualView(VirtualView);
			base.ConnectHandler(nativeView);
			nativeView.Connect();
		}

		public static void RequestNavigation(NavigationViewHandler arg1, INavigationView arg2, object? arg3)
		{
			if (arg3 is MauiNavigationRequestedEventArgs ea)
				arg1.NativeView.RequestNavigation(ea);
		}

		internal void OnPop()
		{
			//NativeView
			//	.NavigationView?
			//	.PopAsync()
			//	.FireAndForget((e) =>
			//	{
			//		//Log.Warning(nameof(NavigationViewHandler), $"{e}");
			//	});
		}
	}
}
