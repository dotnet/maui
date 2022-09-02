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

		public override Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void PlatformArrange(Graphics.Rect frame)
		{
			base.PlatformArrange(frame);
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
			PlatformView.LayoutChange += OnLayoutChanged;
		}

		void OnLayoutChanged(object? sender, View.LayoutChangeEventArgs e)
		{
			VirtualView.Arrange(e);
		}

		private protected override void OnDisconnectHandler(View platformView)
		{
			_stackNavigationManager?.Disconnect();
			base.OnDisconnectHandler(platformView);
			platformView.LayoutChange -= OnLayoutChanged;
		}

		public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object? arg3)
		{
			if (arg1 is NavigationViewHandler platformHandler && arg3 is NavigationRequest ea)
				platformHandler._stackNavigationManager?.RequestNavigation(ea);
		}
	}
}
