using System;
using Android.App;
using Android.Views;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		private protected override void OnConnectHandler(object platformView)
		{
			base.OnConnectHandler(platformView);

			if (OperatingSystem.IsAndroidVersionAtLeast(17) &&
				PlatformView?.Resources?.Configuration != null)
			{
				VirtualView.SetDeviceFlowDirection(PlatformView.Resources.Configuration.LayoutDirection.ToFlowDirection());
			}
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) { }

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootManager = handler.MauiContext.GetNavigationRootManager();
			rootManager.Connect(window.Content);
			handler.PlatformView.SetContentView(rootManager.RootView);
			if (window.VisualDiagnosticsOverlay != null && rootManager.RootView is ViewGroup group)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapToolbar(IWindowHandler handler, IWindow view)
		{
			if (view is IToolbarElement tb)
				ViewHandler.MapToolbar(handler, tb);
		}
	}
}