using Microsoft.Maui.Graphics.Win2D;

namespace Microsoft.Maui
{
	public static class BoxViewExtensions
	{
		public static void InvalidateBoxView(this W2DGraphicsView nativeView, IBoxView boxView)
		{
			nativeView.Invalidate();
		}
	}
}