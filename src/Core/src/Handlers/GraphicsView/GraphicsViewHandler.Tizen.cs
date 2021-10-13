using NUISkiaGraphicsView = Tizen.UIExtensions.NUI.GraphicsView.SkiaGraphicsView;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, NUISkiaGraphicsView>
	{
		protected override NUISkiaGraphicsView CreatePlatformView() => new NUISkiaGraphicsView();

		public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateDrawable(graphicsView);
		}

		public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.PlatformView?.UpdateFlowDirection(graphicsView);
			handler.PlatformView?.Invalidate();
		}

		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.PlatformView?.Invalidate();
		}

		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg)
		{
			handler.PlatformView?.Invalidate();
		}
	}
}