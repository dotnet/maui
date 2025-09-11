using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
	{
		protected override PlatformTouchGraphicsView CreatePlatformView() => new(Context);

		// TODO : The modifier needs to be changed to public in the future.
		internal static void MapBackground(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			if (graphicsView.Background is not null)
			{
				handler.PlatformView?.UpdateBackground(graphicsView);
				handler.PlatformView?.Invalidate();
			}
		}

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
	}
}