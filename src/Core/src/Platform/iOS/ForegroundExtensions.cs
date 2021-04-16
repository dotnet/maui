using CoreGraphics;
using UIKit;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class ForegroundExtensions
	{
		public static void SetForeground(this UILabel nativeView, IBrush? brush, UIColor? defaultTextColor = null)
		{
			if (brush.IsNullOrEmpty())
				return;

			if (brush is ISolidColorBrush solidColorBrush)
			{
				if (defaultTextColor != null)
					nativeView.TextColor = solidColorBrush.Color.ToNative(defaultTextColor);
				else
					nativeView.TextColor = solidColorBrush.Color.ToNative();
			}

			if (brush is IGradientBrush gradientBrush)
			{
				nativeView.SizeToFit();
				CGRect frame = nativeView.Frame;

				if (frame.IsEmpty)
					return;

				var gradientImage = nativeView.GetBackgroundImage(gradientBrush, frame);
				nativeView.TextColor = gradientImage != null ? UIColor.FromPatternImage(gradientImage) : UIColor.Clear;
			}
		}

		public static void SetForeground(this UITextField nativeView, IBrush? brush, UIColor? defaultTextColor = null)
		{
			if (brush.IsNullOrEmpty())
				return;

			if (brush is ISolidColorBrush solidColorBrush)
			{
				if (defaultTextColor != null)
					nativeView.TextColor = solidColorBrush.Color.ToNative(defaultTextColor);
				else
					nativeView.TextColor = solidColorBrush.Color.ToNative();
			}

			if (brush is IGradientBrush gradientBrush)
			{
				nativeView.SizeToFit();
				CGRect frame = nativeView.Frame;

				if (frame.IsEmpty)
					return;

				var gradientImage = nativeView.GetBackgroundImage(gradientBrush, frame);
				nativeView.TextColor = gradientImage != null ? UIColor.FromPatternImage(gradientImage) : UIColor.Clear;
			}
		}

		public static void SetForeground(this UITextView nativeView, IBrush? brush, UIColor? defaultTextColor = null)
		{
			if (brush.IsNullOrEmpty())
				return;

			if (brush is ISolidColorBrush solidColorBrush)
			{
				if (defaultTextColor != null)
					nativeView.TextColor = solidColorBrush.Color.ToNative(defaultTextColor);
				else
					nativeView.TextColor = solidColorBrush.Color.ToNative();
			}

			if (brush is IGradientBrush gradientBrush)
			{
				nativeView.SizeToFit();
				CGRect frame = nativeView.Frame;

				if (frame.IsEmpty)
					return;

				var gradientImage = nativeView.GetBackgroundImage(gradientBrush, frame);
				nativeView.TextColor = gradientImage != null ? UIColor.FromPatternImage(gradientImage) : UIColor.Clear;
			}
		}

		public static void SetForeground(this UIButton nativeView, IBrush? brush, UIColor? buttonTextColorDefaultNormal, UIColor? buttonTextColorDefaultHighlighted, UIColor? buttonTextColorDefaultDisabled)
		{
			if (brush.IsNullOrEmpty())
				return;

			if (brush is ISolidColorBrush solidColorBrush)
			{
				if (solidColorBrush.Color == null)
				{
					nativeView.SetTitleColor(buttonTextColorDefaultNormal, UIControlState.Normal);
					nativeView.SetTitleColor(buttonTextColorDefaultHighlighted, UIControlState.Highlighted);
					nativeView.SetTitleColor(buttonTextColorDefaultDisabled, UIControlState.Disabled);
				}
				else
				{
					var color = solidColorBrush.Color.ToNative();

					nativeView.SetTitleColor(color, UIControlState.Normal);
					nativeView.SetTitleColor(color, UIControlState.Highlighted);
					nativeView.SetTitleColor(color, UIControlState.Disabled);

					nativeView.TintColor = color;
				}
			}

			if (brush is IGradientBrush gradientBrush)
			{
				nativeView.SizeToFit();
				CGRect frame = nativeView.Frame;

				if (frame.IsEmpty)
					return;

				var gradientImage = nativeView.GetBackgroundImage(gradientBrush, frame);
				UIColor? gradientColor = gradientImage != null ? UIColor.FromPatternImage(gradientImage) : UIColor.Clear;

				nativeView.SetTitleColor(gradientColor, UIControlState.Normal);
				nativeView.SetTitleColor(gradientColor, UIControlState.Highlighted);
				nativeView.SetTitleColor(gradientColor, UIControlState.Disabled);
			}
		}

		public static UIImage? GetBackgroundImage(this UIView control, IGradientBrush gradientBrush, CGRect frame = default)
		{
			if (control == null || gradientBrush == null || gradientBrush.IsEmpty)
				return null;

			var gradientLayer = gradientBrush.CreateCALayer(frame);

			if (gradientLayer == null)
				return null;

			UIGraphics.BeginImageContextWithOptions(gradientLayer.Bounds.Size, false, UIScreen.MainScreen.Scale);

			if (UIGraphics.GetCurrentContext() == null)
				return null;

			gradientLayer.RenderInContext(UIGraphics.GetCurrentContext());
			UIImage gradientImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return gradientImage;
		}
	}
}
