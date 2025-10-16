using System;
using Android.App;
using Android.Views;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;
using AndroidX.Window.Layout;
using Google.Android.Material.AppBar;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;
using Android.Content.Res;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		NavigationRootManager? _rootManager;

		protected override void ConnectHandler(Activity platformView)
		{
			base.ConnectHandler(platformView);
			if (OperatingSystem.IsAndroidVersionAtLeast(36))
			{
				//Edge to Edge enabled for Android API 36+
				PlatformView.Window.ConfigureTranslucentSystemBars(PlatformView);
			}
			UpdateVirtualViewFrame(platformView);
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) =>
			handler.PlatformView.UpdateTitle(window);

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

		private protected override void OnConnectHandler(object platformView)
		{
			base.OnConnectHandler(platformView);

			_rootManager = MauiContext!.GetNavigationRootManager();
			_rootManager.RootViewChanged += OnRootViewChanged;
		}

		private protected override void OnDisconnectHandler(object platformView)
		{
			base.OnDisconnectHandler(platformView);

			DisconnectHandler(_rootManager);

			if (_rootManager != null)
				_rootManager.RootViewChanged -= OnRootViewChanged;
		}

		void OnRootViewChanged(object? sender, EventArgs e)
		{
			if (VirtualView.VisualDiagnosticsOverlay != null && _rootManager?.RootView is ViewGroup)
			{
				if (VirtualView.VisualDiagnosticsOverlay.IsPlatformViewInitialized)
					VirtualView.VisualDiagnosticsOverlay.Deinitialize();

				VirtualView.VisualDiagnosticsOverlay.Initialize();
			}
		}

		// This is here to try and ensure symmetry with disconnect code between test handler
		// and the real handler
		internal static void DisconnectHandler(NavigationRootManager? navigationRootManager)
		{
			navigationRootManager?.Disconnect();
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