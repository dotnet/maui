using System;
using Android.App;
using Android.Views;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Activity>
	{
		void UpdateToolbar()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var appbarLayout = NativeView.FindViewById<ViewGroup>(Microsoft.Maui.Resource.Id.navigationlayout_appbar);

			if (appbarLayout == null || VirtualView is not IToolbarElement te)
				return;

			var nativeToolBar = te.Toolbar?.ToNative(MauiContext, true);
			if (nativeToolBar == null || nativeToolBar.Parent == nativeToolBar)
				return;

			appbarLayout.AddView(nativeToolBar, 0);

		}

		public static void MapTitle(WindowHandler handler, IWindow window) { }

		public static void MapContent(WindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var rootManager = handler.MauiContext.GetNavigationRootManager();
			rootManager.SetRootView(window.Content);
			handler.NativeView.SetContentView(rootManager.RootView);
			if (window.VisualDiagnosticsOverlay != null && rootManager.RootView is ViewGroup group)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapToolbar(WindowHandler handler, IWindow view)
		{
			handler.UpdateToolbar();
		}
	}
}