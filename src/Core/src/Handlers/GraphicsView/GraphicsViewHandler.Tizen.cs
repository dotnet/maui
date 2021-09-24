using Microsoft.Maui.Graphics.Skia.Views;

namespace Microsoft.Maui.Handlers
{
	public partial class GraphicsViewHandler : ViewHandler<IGraphicsView, SkiaGraphicsView>
	{
		protected override SkiaGraphicsView CreateNativeView()
		{
			return new SkiaGraphicsView(NativeParent);
		}

		public static void MapDrawable(GraphicsViewHandler handler, IGraphicsView graphicsView)
		{
			handler.NativeView?.UpdateDrawable(graphicsView);
		}
	}
}