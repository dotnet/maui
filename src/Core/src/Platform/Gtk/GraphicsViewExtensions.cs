using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this PlatformTouchGraphicsView platformView, IGraphicsView graphicsView)
		{
			platformView.Drawable = graphicsView.Drawable;
		}
	}
}