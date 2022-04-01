using System;
using System.Diagnostics;
using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
	{
		protected override PlatformTouchGraphicsView CreatePlatformView()
		{
			Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362));
			return new PlatformTouchGraphicsView();
		}

		public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362));
			handler.PlatformView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362));
			handler.PlatformView?.UpdateFlowDirection(graphicsView);
			handler.PlatformView?.Invalidate();
		}

		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362));
			handler.PlatformView?.Invalidate();
		}
	}
}