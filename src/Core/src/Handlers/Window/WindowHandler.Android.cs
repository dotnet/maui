using System;
using Android.App;
using Android.Views;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		public static void MapTitle(WindowHandler handler, IWindow window) { }

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootManager = handler.MauiContext.GetNavigationRootManager();
			rootManager.Connect(window.Content);
			handler.NativeView.SetContentView(rootManager.RootView);
			if (window.VisualDiagnosticsOverlay != null && rootManager.RootView is ViewGroup group)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapToolbar(WindowHandler handler, IWindow view)
		{
			if (view is IToolbarElement tb)
				ViewHandler.MapToolbar(handler, tb);
		}
	}
}