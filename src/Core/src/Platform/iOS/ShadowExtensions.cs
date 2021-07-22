using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class ShadowExtensions
	{
		public static void SetShadow(this UIView nativeView, Shadow shadow)
		{
			var layer = nativeView.Layer;
			layer?.SetShadow(shadow);
		}

		public static void SetShadow(this CALayer layer, Shadow shadow)
		{
			if (shadow.IsEmpty)
				return;

			var radius = shadow.Radius;
			var opacity = shadow.Opacity;
			var color = shadow.Color.ToNative();
			var offset = new CGSize((double)shadow.Offset.Width, (double)shadow.Offset.Height);

			layer.ShadowColor = color.CGColor;
			layer.ShadowOpacity = opacity;
			layer.ShadowRadius = radius;
			layer.ShadowOffset = offset;

			layer.SetNeedsDisplay();
		}

		public static void ClearShadow(this UIView nativeView)
		{
			var layer = nativeView.Layer;
			layer?.ClearShadow();
		}

		public static void ClearShadow(this CALayer layer)
		{
			layer.ShadowColor = new CGColor(0, 0, 0, 0);
			layer.ShadowRadius = 0;
			layer.ShadowOffset = new CGSize();
			layer.ShadowOpacity = 0;
		}
	}
}