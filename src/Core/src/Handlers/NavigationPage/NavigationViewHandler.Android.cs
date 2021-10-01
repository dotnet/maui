using System;
using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler :
		ViewHandler<INavigationView, View>
	{
		NavigationManager? _navigationManager;

		protected override View CreateNativeView()
		{
			LayoutInflater? li = MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var view = li.Inflate(Resource.Layout.fragment_backstack, null).JavaCast<FragmentContainerView>();
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

		protected override void ConnectHandler(View nativeView)
		{
			_navigationManager = CreateNavigationManager();
			var rootContainer = MauiContext!.GetNavigationRootManager();
			var navigationLayout = rootContainer.NavigationLayout;

			base.ConnectHandler(nativeView);
			_navigationManager.Connect(VirtualView, navigationLayout);
		}

		private protected override void OnDisconnectHandler(View nativeView)
		{
			_navigationManager.Disconnect();
			base.OnDisconnectHandler(nativeView);
		}

		protected override void DisconnectHandler(View nativeView)
		{
			base.DisconnectHandler(nativeView);
		}

		public static void RequestNavigation(NavigationViewHandler arg1, INavigationView arg2, object? arg3)
		{
			if (arg3 is NavigationRequest ea)
				arg1._navigationManager?.RequestNavigation(ea);
		}
	}
}
