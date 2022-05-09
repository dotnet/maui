using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : ElementHandler<IWindow, Window>
	{
		public static void MapTitle(IWindowHandler handler, IWindow window) { }

		public static void MapContent(IWindowHandler handler, IWindow window)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var platformContent = window.Content.ToContainerView(handler.MauiContext);

			platformContent.SetAlignment(-1, -1);
			platformContent.SetWeight(1, 1);
			platformContent.Show();
			handler.MauiContext.GetModalStack().Reset();
			handler.MauiContext.GetModalStack().Push(platformContent);

			if (window.VisualDiagnosticsOverlay != null)
				window.VisualDiagnosticsOverlay.Initialize();
		}

		public static void MapRequestDisplayDensity(IWindowHandler handler, IWindow window, object? args)
		{
			if (args is DisplayDensityRequest request)
				request.SetResult(handler.PlatformView.GetDisplayDensity());
		}
	}
}