using System;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapDrawable(IViewHandler handler, IGraphicsView graphicsView) { }
	}
}