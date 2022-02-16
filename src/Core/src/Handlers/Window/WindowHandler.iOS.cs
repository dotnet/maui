using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, UIWindow>
	{
		public static void MapTitle(IWindowHandler handler, IWindow window) { }

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var nativeContent = window.Content.ToUIViewController(handler.MauiContext);

			handler.PlatformView.RootViewController = nativeContent;

			if (window.VisualDiagnosticsOverlay != null)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapMenuBar(IWindowHandler handler, IWindow view)
		{
			if (view is IMenuBarElement mb)
			{
				if (MauiUIApplicationDelegate.Current.MenuBuilder == null)
				{
					UIMenuSystem
						.MainSystem
						.SetNeedsRebuild();
				}
				else
				{
					_ = mb.MenuBar?.ToHandler(handler.MauiContext!);
				}
			}
		}
	}
}