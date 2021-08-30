using Microsoft.Maui.Graphics.Skia.Views;
using SkiaGraphicsView = Microsoft.Maui.Platform.Tizen.SkiaGraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this SkiaGraphicsView platformGraphicsView, IGraphicsView graphicsView)
		{
			platformGraphicsView.Drawable = graphicsView.Drawable;
		}
	}
}