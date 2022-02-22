using System;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapDrawable(IGraphicsViewHandler handler, IGraphicsView graphicsView) { }
		public static void MapFlowDirection(IGraphicsViewHandler handler, IGraphicsView graphicsView) { }

		public static void MapInvalidate(IGraphicsViewHandler handler, IGraphicsView graphicsView, object? arg) { }
	}
}