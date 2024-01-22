using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
	{
		protected override PlatformTouchGraphicsView CreatePlatformView() => new(Context);

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
			if (arg is RectF rect)
			{
				Android.Graphics.Rect aRect = new Android.Graphics.Rect((int)rect.X, (int)rect.Y, (int)(rect.X + rect.Width), (int)(rect.Y + rect.Height));
#pragma warning disable CA1422 // Validate platform compatibility
				// This method was deprecated in API level 28.
				// In API 21 the given rectangle is ignored entirely in favor of an internally-calculated area instead.
				handler.PlatformView?.Invalidate(aRect);
#pragma warning restore CA1422 // Validate platform compatibility
			}
			else
				handler.PlatformView?.Invalidate();
		}
	}
}