using CoreGraphics;
using UIKit;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class ForegroundExtensions
	{
		public static void SetForeground(this UILabel nativeView, Paint? paint, UIColor? defaultTextColor = null)
		{
			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				if (defaultTextColor != null)
					nativeView.TextColor = solidPaint.Color.ToNative(defaultTextColor);
				else
					nativeView.TextColor = solidPaint.Color.ToNative();
			}

			if (paint is GradientPaint gradientPaint)
			{
				nativeView.SizeToFit();
				CGRect frame = nativeView.Frame;

				if (frame.IsEmpty)
					return;

				var gradientImage = nativeView.GetBackgroundImage(gradientPaint, frame);
				nativeView.TextColor = gradientImage != null ? UIColor.FromPatternImage(gradientImage) : UIColor.Clear;
			}
		}

		public static void SetForeground(this UITextField nativeView, Paint? paint, UIColor? defaultTextColor = null)
		{
			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				if (defaultTextColor != null)
					nativeView.TextColor = solidPaint.Color.ToNative(defaultTextColor);
				else
					nativeView.TextColor = solidPaint.Color.ToNative();
			}

			if (paint is GradientPaint gradientPaint)
			{
				nativeView.SizeToFit();
				CGRect frame = nativeView.Frame;

				if (frame.IsEmpty)
					return;

				var gradientImage = nativeView.GetBackgroundImage(gradientPaint, frame);
				nativeView.TextColor = gradientImage != null ? UIColor.FromPatternImage(gradientImage) : UIColor.Clear;
			}
		}

		public static void SetForeground(this UITextView nativeView, Paint? paint, UIColor? defaultTextColor = null)
		{
			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				if (defaultTextColor != null)
					nativeView.TextColor = solidPaint.Color.ToNative(defaultTextColor);
				else
					nativeView.TextColor = solidPaint.Color.ToNative();
			}

			if (paint is GradientPaint gradientPaint)
			{
				nativeView.SizeToFit();
				CGRect frame = nativeView.Frame;

				if (frame.IsEmpty)
					return;

				var gradientImage = nativeView.GetBackgroundImage(gradientPaint, frame);
				nativeView.TextColor = gradientImage != null ? UIColor.FromPatternImage(gradientImage) : UIColor.Clear;
			}
		}

		public static void SetForeground(this UIButton nativeView, Paint? paint, UIColor? buttonTextColorDefaultNormal, UIColor? buttonTextColorDefaultHighlighted, UIColor? buttonTextColorDefaultDisabled)
		{
			if (paint.IsNullOrEmpty())
				return;

			if (paint is SolidPaint solidPaint)
			{
				if (solidPaint.Color == null)
				{
					nativeView.SetTitleColor(buttonTextColorDefaultNormal, UIControlState.Normal);
					nativeView.SetTitleColor(buttonTextColorDefaultHighlighted, UIControlState.Highlighted);
					nativeView.SetTitleColor(buttonTextColorDefaultDisabled, UIControlState.Disabled);
				}
				else
				{
					var color = solidPaint.Color.ToNative();

					nativeView.SetTitleColor(color, UIControlState.Normal);
					nativeView.SetTitleColor(color, UIControlState.Highlighted);
					nativeView.SetTitleColor(color, UIControlState.Disabled);

					nativeView.TintColor = color;
				}
			}

			if (paint is GradientPaint gradientPaint)
			{
				nativeView.SizeToFit();
				CGRect frame = nativeView.Frame;

				if (frame.IsEmpty)
					return;

				var gradientImage = nativeView.GetBackgroundImage(gradientPaint, frame);
				UIColor? gradientColor = gradientImage != null ? UIColor.FromPatternImage(gradientImage) : UIColor.Clear;

				nativeView.SetTitleColor(gradientColor, UIControlState.Normal);
				nativeView.SetTitleColor(gradientColor, UIControlState.Highlighted);
				nativeView.SetTitleColor(gradientColor, UIControlState.Disabled);
			}
		}

		public static UIImage? GetBackgroundImage(this UIView control, GradientPaint gradientPaint, CGRect frame = default)
		{
			if (control == null || gradientPaint.IsNullOrEmpty())
				return null;

			var gradientLayer = gradientPaint.CreateCALayer(frame);

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
