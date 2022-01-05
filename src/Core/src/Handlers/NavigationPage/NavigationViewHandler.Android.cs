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
			UpdateValue(nameof(StackNavigationManager));

			_= _stackNavigationManager ?? throw new InvalidOperationException($"StackNavigationManager cannot be null");
			LayoutInflater? li = _stackNavigationManager?.MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var view = li.Inflate(Resource.Layout.fragment_backstack, null).JavaCast<FragmentContainerView>();
			_ = view ?? throw new InvalidOperationException($"Resource.Layout.navigationlayout view not found");

			return view;
		}

		public void SetStackNavigationManager(StackNavigationManager stackNavigationManager)
		{
			if (_stackNavigationManager != null && _stackNavigationManager != stackNavigationManager)
				throw new InvalidOperationException("StackNavigationManager cannot be assigned to new instance");

			_stackNavigationManager = stackNavigationManager;
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

		public static void MapStackNavigationManager(NavigationViewHandler handler, INavigationView view)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			handler.SetStackNavigationManager(handler._stackNavigationManager ?? new StackNavigationManager(handler.MauiContext));
		}

		public static void MapNavControllerNavigateToResIdRequest(NavigationViewHandler handler, INavigationView view, object? args)
		{
			var navRequest = (NavControllerNavigateToResIdRequest)args!;

			navRequest
				.StackNavigationManager
				.NavController
				.Navigate(navRequest.ResId, navRequest.Args, navRequest.NavOptions, navRequest.NavigatorExtras);
		}

		public static void MapNavControllerPopBackStackRequest(NavigationViewHandler handler, INavigationView view, object? args)
		{
			var navRequest = (NavControllerPopBackStackRequest)args!;

			if (navRequest.DestinationId.HasValue && navRequest.Inclusive.HasValue)
			{
				navRequest
					.StackNavigationManager
					.NavController
					.PopBackStack(navRequest.DestinationId.Value, navRequest.Inclusive.Value);
			}
			else
			{
				navRequest
					.StackNavigationManager
					.NavController
					.PopBackStack();
			}
		}

	}
}
