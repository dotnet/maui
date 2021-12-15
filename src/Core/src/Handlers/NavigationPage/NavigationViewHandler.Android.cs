using System;
using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler :
		ViewHandler<INavigationView, View>
	{
		StackNavigationManager? _stackNavigationManager;
		internal StackNavigationManager? StackNavigationManager => _stackNavigationManager;

		protected override View CreateNativeView()
		{
			LayoutInflater? li = CreateNavigationManager().MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var view = li.Inflate(Resource.Layout.fragment_backstack, null).JavaCast<FragmentContainerView>();
			_ = view ?? throw new InvalidOperationException($"Resource.Layout.navigationlayout view not found");

			return view;
		}

		StackNavigationManager CreateNavigationManager()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			return _stackNavigationManager ??= new StackNavigationManager(MauiContext);
		}

		protected override void ConnectHandler(View nativeView)
		{
			base.ConnectHandler(nativeView);
			_stackNavigationManager?.Connect(VirtualView);
		}

		private protected override void OnDisconnectHandler(View nativeView)
		{
			_stackNavigationManager?.Disconnect();
			base.OnDisconnectHandler(nativeView);
		}

		public static void RequestNavigation(NavigationViewHandler arg1, INavigationView arg2, object? arg3)
		{
			if (arg3 is NavigationRequest ea)
				arg1._stackNavigationManager?.RequestNavigation(ea);
		}
	}
}
