using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this W2DGraphicsView nativeGraphicsView, IGraphicsView graphicsView)
		{
			nativeGraphicsView.Drawable = graphicsView.Drawable;
		}
	}
}