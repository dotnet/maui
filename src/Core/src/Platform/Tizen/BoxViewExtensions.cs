using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class BoxViewExtensions
	{
		public static void InvalidateBoxView(this MauiBoxView nativeView, IBoxView boxView)
		{
			nativeView.Invalidate();
		}		
	}
}