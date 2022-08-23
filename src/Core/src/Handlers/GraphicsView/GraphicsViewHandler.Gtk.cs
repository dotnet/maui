using System;

namespace Microsoft.Maui.Handlers
{

	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, PlatformTouchGraphicsView>
	{

		protected override PlatformTouchGraphicsView CreatePlatformView() => new();

		public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			if (handler.PlatformView is { } nativeView)
			{
				nativeView.Drawable = graphicsView.Drawable;
			}
		}

		[MissingMapper]
		public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView) { }

		[MissingMapper]
		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg) { }
	}

}