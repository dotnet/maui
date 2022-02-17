using System;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapDrawable(IViewHandler handler, IGraphicsView graphicsView) { }
		public static void MapFlowDirection(IViewHandler handler, IGraphicsView graphicsView) { }

		public static void MapInvalidate(IViewHandler handler, IGraphicsView graphicsView, object? arg) { }
	}
}