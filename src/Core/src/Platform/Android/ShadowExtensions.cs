using Android.Graphics;
using Android.Views;
using Android.Widget;
using AndroidX.Core.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class ShadowExtensions
	{
		public static void SetShadow(this AView nativeView, Shadow shadow)
		{
			if (shadow.IsEmpty)
				return;

			var radius = shadow.Radius;

			if (radius < 0)
				return;

			var opacity = shadow.Opacity;

			if (opacity < 0)
				return;

			var color = shadow.Color.ToNative();
			var alphaColor = new Color(ColorUtils.SetAlphaComponent(color, (int)opacity * 255));

			if (nativeView is TextView textView)
			{
				var offsetX = (float)shadow.Offset.Width;
				var offsetY = (float)shadow.Offset.Height;
				textView.SetShadowLayer(radius, offsetX, offsetY, alphaColor);
				return;
			}

			nativeView.OutlineProvider = ViewOutlineProvider.PaddedBounds;

			if (nativeView.Context != null)
				nativeView.Elevation = nativeView.Context.ToPixels(radius);

			if (!NativeVersion.Supports(NativeApis.OutlineAmbientShadowColor))
				return;

			nativeView.SetOutlineAmbientShadowColor(alphaColor);
			nativeView.SetOutlineSpotShadowColor(alphaColor);
		}

		public static void ClearShadow(this AView nativeView)
		{
			nativeView.OutlineProvider = null;
		}
	}
}