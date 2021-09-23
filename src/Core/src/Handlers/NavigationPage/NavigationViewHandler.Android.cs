#nullable enable

using System;
using Android.Runtime;
using Android.Views;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler :
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

		public override void SetVirtualView(IView view)
		{
			_navigationManager ??= CreateNavigationManager();
			base.SetVirtualView(view);
		}

		protected virtual NavigationManager CreateNavigationManager() =>
			new NavigationManager(MauiContext!);

		protected override void ConnectHandler(NavigationLayout nativeView)
		{
			_navigationManager ??= CreateNavigationManager();
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
