using System;
using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui.Handlers
{
	public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, View>
	{
		StackNavigationManager? _stackNavigationManager;
		internal StackNavigationManager? StackNavigationManager => _stackNavigationManager;

		protected override View CreatePlatformView()
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

		protected override void ConnectHandler(View platformView)
		{
			base.ConnectHandler(platformView);

			_stackNavigationManager?.Connect(VirtualView);

			platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow += OnViewDetachedFromWindow;
		}

		void OnViewDetachedFromWindow(object? sender, View.ViewDetachedFromWindowEventArgs e)
		{
			PlatformView.LayoutChange -= OnLayoutChanged;
		}

		void OnViewAttachedToWindow(object? sender, View.ViewAttachedToWindowEventArgs e)
		{
			PlatformView.LayoutChange += OnLayoutChanged;
		}

		void OnLayoutChanged(object? sender, View.LayoutChangeEventArgs e) =>
			VirtualView.Arrange(e);

		void RequestNavigation(NavigationRequest ea)
		{
			_stackNavigationManager?.RequestNavigation(ea);
		}

		private protected override void OnDisconnectHandler(View platformView)
		{
			platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
			platformView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
			platformView.LayoutChange -= OnLayoutChanged;

			_stackNavigationManager?.Disconnect();
			base.OnDisconnectHandler(platformView);
		}

		public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest ea)
				platformHandler.RequestNavigation(ea);
		}
	}
}
