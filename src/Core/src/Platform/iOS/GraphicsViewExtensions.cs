using Microsoft.Maui.Graphics.Native;

namespace Microsoft.Maui
{
	public static class GraphicsViewExtensions
	{
		public static void UpdateDrawable(this NativeGraphicsView nativeGraphicsView, IGraphicsView graphicsView)
		{
			nativeGraphicsView.Drawable = graphicsView.Drawable;
		}
	}
}