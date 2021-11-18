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

			var nativeContent = window.Content.ToContainerView(handler.MauiContext);
			var rootManager = handler.MauiContext.GetNavigationRootManager();
			rootManager.SetContentView(nativeContent);
			handler.NativeView.SetContentView(rootManager.RootView);
			if (window.VisualDiagnosticsOverlay != null && rootManager.RootView is ViewGroup group)
				window.VisualDiagnosticsOverlay.Initialize();
		}
	}
}