using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this PlatformGraphicsView PlatformGraphicsView, IGraphicsView graphicsView)
		{
			PlatformGraphicsView.Drawable = graphicsView.Drawable;
		}

		internal static void UpdateBackground(this PlatformGraphicsView PlatformGraphicsView, IGraphicsView graphicsView)
		{
			PlatformGraphicsView.BackgroundColor = graphicsView.Background?.ToColor();
		}
	}
}