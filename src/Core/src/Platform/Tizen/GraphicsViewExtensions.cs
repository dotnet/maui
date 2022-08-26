using NUISkiaGraphicsView = Tizen.UIExtensions.NUI.GraphicsView.SkiaGraphicsView;

namespace Microsoft.Maui.Platform
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this NUISkiaGraphicsView platformGraphicsView, IGraphicsView graphicsView)
		{
			platformGraphicsView.Drawable = graphicsView.Drawable;
		}
	}
}