#nullable enable

using System;
using Android.Runtime;
using Android.Views;

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
			base.ConnectHandler(nativeView);
			NativeView.SetVirtualView(VirtualView);
		}

		public static void RequestNavigation(NavigationViewHandler arg1, INavigationView arg2, object? arg3)
		{
			if (arg3 is MauiNavigationRequestedEventArgs ea)
				arg1.NativeView.RequestNavigation(ea);
		}
	}
}
