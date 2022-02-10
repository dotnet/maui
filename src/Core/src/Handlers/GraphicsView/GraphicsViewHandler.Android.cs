using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformGraphicsView>
	{
		protected override PlatformGraphicsView CreateNativeView()
		{
			return new PlatformGraphicsView(Context);
		}

		public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.NativeView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.NativeView?.UpdateFlowDirection(graphicsView);
			handler.NativeView?.Invalidate();
		}

		public static void MapInvalidate(GraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.NativeView?.Invalidate();
		}
	}
}