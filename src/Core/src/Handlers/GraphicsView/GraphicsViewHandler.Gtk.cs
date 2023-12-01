using System;

namespace Microsoft.Maui.Handlers
{

	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
	{
		protected override PlatformTouchGraphicsView CreatePlatformView() => new();

		public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateDrawable(graphicsView);
		}

		[MissingMapper]
		public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView) { }

		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.PlatformView?.QueueDraw();
		}
	}

}