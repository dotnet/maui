using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui.Platform
{
	public static class GraphicsViewExtensions
	{
		[System.Runtime.Versioning.SupportedOSPlatform("windows10.0.18362")]
		public static void UpdateDrawable(this W2DGraphicsView PlatformGraphicsView, IGraphicsView graphicsView)
		{
			PlatformGraphicsView.Drawable = graphicsView.Drawable;
		}
	}
}