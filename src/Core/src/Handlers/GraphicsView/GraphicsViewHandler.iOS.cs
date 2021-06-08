using Microsoft.Maui.Graphics.Native;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, NativeGraphicsView>
	{
		protected override NativeGraphicsView CreateNativeView()
		{
			return new NativeGraphicsView();
		}

		public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.NativeView?.UpdateDrawable(graphicsView);
		}
	}
}