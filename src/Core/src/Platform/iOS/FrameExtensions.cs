using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class FrameExtensions
	{
		public static void UpdateContent(this MauiFrame nativeView, IFrame frame, IMauiContext? mauiContext)
		{
			var content = frame.Content;

			if (content == null || mauiContext == null)
				return;

			nativeView.AddSubview(content.ToNative(mauiContext));
			nativeView.SetNeedsLayout();
		}

		public static void UpdateBackgroundColor(this MauiFrame nativeView, IFrame frame, MauiFrameContent? nativeFrame)
		{
			nativeView.SetupLayer(frame, nativeFrame);
		}

		public static void UpdateBorderColor(this MauiFrame nativeView, IFrame frame, MauiFrameContent? nativeFrame)
		{
			nativeView.SetupLayer(frame, nativeFrame);
		}

		public static void UpdateHasShadow(this MauiFrame nativeView, IFrame frame, MauiFrameContent? nativeFrame)
		{
			nativeView.SetupLayer(frame, nativeFrame);
		}

		public static void UpdateCornerRadius(this MauiFrame nativeView, IFrame frame, MauiFrameContent? nativeFrame)
		{
			nativeView.SetupLayer(frame, nativeFrame);
		}

		internal static void SetupLayer(this MauiFrame nativeView, IFrame frame, MauiFrameContent? nativeFrame)
		{
			if (nativeView == null || nativeFrame == null)
				return;

			var cornerRadius = frame.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = 5f; // Default corner radius

			nativeFrame.Layer.CornerRadius = cornerRadius;
			nativeFrame.Layer.MasksToBounds = cornerRadius > 0;

			if (frame.BackgroundColor.IsDefault())
				nativeFrame.Layer.BackgroundColor = ColorExtensions.BackgroundColor.CGColor;
			else
			{
				// BackgroundColor gets set on the base class too which messes with
				// the corner radius, shadow, etc. so override that behaviour here
				nativeView.BackgroundColor = UIColor.Clear;
				nativeFrame.Layer.BackgroundColor = frame.BackgroundColor.ToCGColor();
			}

			if (frame.BorderColor.IsDefault())
				nativeFrame.Layer.BorderColor = UIColor.Clear.CGColor;
			else
			{
				nativeFrame.Layer.BorderColor = frame.BorderColor.ToCGColor();
				nativeFrame.Layer.BorderWidth = 1;
			}

			if (frame.HasShadow)
			{
				nativeView.Layer.ShadowRadius = 5;
				nativeView.Layer.ShadowColor = UIColor.Black.CGColor;
				nativeView.Layer.ShadowOpacity = 0.8f;
				nativeView.Layer.ShadowOffset = CGSize.Empty;
			}
			else
			{
				nativeView.Layer.ShadowOpacity = 0;
			}

			nativeView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			nativeView.Layer.ShouldRasterize = true;
			nativeView.Layer.MasksToBounds = false;

			nativeFrame.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			nativeFrame.Layer.ShouldRasterize = true;
		}
	}
}