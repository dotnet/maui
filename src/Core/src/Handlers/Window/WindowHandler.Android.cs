using System;
using Android.App;
using Android.Views;
using AndroidX.Window.Layout;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		protected override void ConnectHandler(Activity platformView)
		{
			base.ConnectHandler(platformView);

			UpdateVirtualViewFrame(platformView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) { }

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootView = CreateRootViewFromContent(handler, window);
			handler.PlatformView.SetContentView(rootView);
		}

		public static void MapX(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateX(view);

		public static void MapY(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateY(view);

		public static void MapWidth(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateWidth(view);

		public static void MapHeight(IWindowHandler handler, IWindow view) =>
			handler.PlatformView?.UpdateHeight(view);

		public static void MapToolbar(IWindowHandler handler, IWindow view)
		{
			if (view is IToolbarElement tb)
				ViewHandler.MapToolbar(handler, tb);
		}

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
		{
			if (args is DisplayDensityRequest request)
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}

		NavigationRootManager? _rootManager;
		private protected override void OnConnectHandler(object platformView)
		{
			base.OnConnectHandler(platformView);

			_rootManager = MauiContext!.GetNavigationRootManager();
			_rootManager.RootViewChanged += OnRootViewChanged;
		}

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);
			if (_rootManager != null)
				_rootManager.RootViewChanged += OnRootViewChanged;
		}

		void OnRootViewChanged(object? sender, EventArgs e)
		{
			if (VirtualView.VisualDiagnosticsOverlay != null && _rootManager?.RootView is ViewGroup)
				VirtualView.VisualDiagnosticsOverlay.Initialize();
		}

		internal static View? CreateRootViewFromContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootManager = handler.MauiContext.GetNavigationRootManager();
			rootManager.Connect(window.Content);
			return rootManager.RootView;
		}

		void UpdateVirtualViewFrame(Activity activity)
		{
			var frame = activity.GetWindowFrame();
			VirtualView.FrameChanged(frame);
		}
	}
}