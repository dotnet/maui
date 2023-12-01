using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this PlatformTouchGraphicsView PlatformGraphicsView, IGraphicsView graphicsView)
		{
			PlatformGraphicsView.Drawable = graphicsView.Drawable;
		}
	}
}