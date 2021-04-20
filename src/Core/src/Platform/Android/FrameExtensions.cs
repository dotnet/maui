using Android.Graphics.Drawables;
using AndroidX.CardView.Widget;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui
{
	public static class FrameExtensions
	{
		public static void UpdateContent(this CardView cardView, IFrame frame, IMauiContext? mauiContext)
		{
			var content = frame.Content;

			if (content == null || mauiContext == null)
				return;

			cardView.AddView(content.ToNative(mauiContext));
		}

		public static void UpdateBackgroundColor(this CardView cardView, IFrame frame)
		{
			var gradientdrawable = cardView.Background as GradientDrawable;
			cardView.UpdateBorderColor(frame, gradientdrawable);
		}

		public static void UpdateBackgroundColor(this CardView cardView, IFrame frame, GradientDrawable? gradientDrawable)
		{
			Color backgroundColor = frame.BackgroundColor;
			gradientDrawable?.SetColor(backgroundColor.IsDefault() ? AColor.White : backgroundColor.ToNative());
		}

		public static void UpdateBorderColor(this CardView cardView, IFrame frame)
		{
			var gradientdrawable = cardView.Background as GradientDrawable;
			cardView.UpdateBorderColor(frame, gradientdrawable);
		}

		public static void UpdateBorderColor(this CardView cardView, IFrame frame, GradientDrawable? gradientDrawable)
		{
			if (cardView == null)
				return;

			Color borderColor = frame.BorderColor;

			if (borderColor.IsDefault())
				gradientDrawable?.SetStroke(0, AColor.Transparent);
			else
				gradientDrawable?.SetStroke(3, borderColor.ToNative());
		}

		public static void UpdateHasShadow(this CardView cardView, IFrame frame)
		{
			cardView.UpdateHasShadow(frame, null);
		}

		public static void UpdateHasShadow(this CardView cardView, IFrame frame, float? defaultElevation)
		{
			if (defaultElevation == -1f)
				defaultElevation = cardView.CardElevation;

			float elevation = defaultElevation ?? -1f;

			if (elevation == -1f)
				elevation = cardView.CardElevation;

			if (frame.HasShadow)
				cardView.CardElevation = elevation;
			else
				cardView.CardElevation = 0f;
		}

		public static void UpdateCornerRadius(this CardView cardView, IFrame frame)
		{
			var gradientdrawable = cardView.Background as GradientDrawable;
			cardView.UpdateCornerRadius(frame, gradientdrawable, null);
		}

		public static void UpdateCornerRadius(this CardView cardView, IFrame frame, GradientDrawable? gradientDrawable, float? defaultCornerRadius)
		{
			if (defaultCornerRadius == -1f)
				defaultCornerRadius = cardView.Radius;

			var cornerRadius = frame.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = defaultCornerRadius ?? 0;
			else
				cornerRadius = cardView.Context?.ToPixels(cornerRadius) ?? 0;

			gradientDrawable?.SetCornerRadius(cornerRadius);
		}
	}
}