using System;
using Android.App;
using Android.Views;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		internal static void UpdateContent(IWindowHandler handler, IWindow window, object addTo)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootManager = handler.MauiContext.GetNavigationRootManager();
			rootManager.Connect(window.Content);

			if (addTo is Activity activity)
			{
				activity.SetContentView(rootManager.RootView);
				if (window.VisualDiagnosticsOverlay != null && rootManager.RootView is ViewGroup group)
					window.VisualDiagnosticsOverlay.Initialize();
			}
			else if (addTo is ViewGroup vg)
			{
				vg.RemoveAllViews();
				vg.AddView(rootManager.RootView);
			}
		}

		public static void MapTitle(IWindowHandler handler, IWindow window) { }

		public static void MapContent(IWindowHandler handler, IWindow window) =>
			UpdateContent(handler, window, handler.PlatformView);

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
	}
}