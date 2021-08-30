using Microsoft.Maui.Graphics.Skia.Views;
using SkiaGraphicsView = Microsoft.Maui.Platform.Tizen.SkiaGraphicsView;

namespace Microsoft.Maui
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this SkiaGraphicsView nativeGraphicsView, IGraphicsView graphicsView)
		{
			nativeGraphicsView.Drawable = graphicsView.Drawable;
		}
	}
}