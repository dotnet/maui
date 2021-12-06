using Microsoft.Maui.Graphics.Skia.Views;

namespace Microsoft.Maui.Platform
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this SkiaGraphicsView nativeGraphicsView, IGraphicsView graphicsView)
		{
			nativeGraphicsView.Drawable = graphicsView.Drawable;
		}
	}
}