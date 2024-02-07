using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Widget;
using Google.Android.Material.Button;
using Java.Util;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		const int BackgroundDrawableId = 999;

		public static void UpdateBackground(this MaterialButton platformView, IButton button)
		{
			platformView.UpdateBorderDrawable(button);
		}

		public static void UpdateStrokeColor(this MaterialButton platformView, IButton button)
		{
			if (platformView.Background is RippleDrawable background)
			{
				var gradientDrawable = background.FindDrawableByLayerId(BackgroundDrawableId);

				if (gradientDrawable is not null)
				{
					platformView.UpdateBorderDrawable(button);
					return;
				}
			}

			if (button is IButtonStroke buttonStroke && buttonStroke.StrokeColor is Color stroke)
				platformView.StrokeColor = ColorStateListExtensions.CreateButton(stroke.ToPlatform());
		}

		public static void UpdateStrokeThickness(this MaterialButton platformView, IButton button)
		{
			if (platformView.Background is RippleDrawable background)
			{
				var gradientDrawable = background.FindDrawableByLayerId(BackgroundDrawableId);

				if (gradientDrawable is not null)
				{
					platformView.UpdateBorderDrawable(button);
					return;
				}
			}

			if (button is IButtonStroke buttonStroke && buttonStroke.StrokeThickness >= 0)
				platformView.StrokeWidth = (int)platformView.Context.ToPixels(buttonStroke.StrokeThickness);
		}

		public static void UpdateCornerRadius(this MaterialButton platformView, IButton button)
		{
			if (platformView.Background is RippleDrawable background)
			{
				var gradientDrawable = background.FindDrawableByLayerId(BackgroundDrawableId);

				if (gradientDrawable is not null)
				{
					platformView.UpdateBorderDrawable(button);
					return;
				}
			}

			if (button is IButtonStroke buttonStroke && buttonStroke.CornerRadius >= 0)
				platformView.CornerRadius = (int)platformView.Context.ToPixels(buttonStroke.CornerRadius);
		}

		public static void UpdatePadding(this Button platformControl, IPadding padding, Thickness? defaultPadding = null) =>
			UpdatePadding(platformControl, padding.Padding, defaultPadding);

		public static void UpdatePadding(this Button platformControl, Thickness padding, Thickness? defaultPadding = null)
		{
			var context = platformControl.Context;
			if (context == null)
				return;

			if (padding.IsNaN)
				padding = defaultPadding ?? Thickness.Zero;

			padding = context.ToPixels(padding);

			platformControl.SetPadding(
				(int)padding.Left,
				(int)padding.Top,
				(int)padding.Right,
				(int)padding.Bottom);
		}

		internal static void UpdateBorderDrawable(this MaterialButton platformView, IButton button)
		{
			var background = button.Background;

			if (!background.IsNullOrEmpty())
			{
				// Remove previous background gradient if any
				if (platformView.Background is RippleDrawable previousBackground)
				{
					platformView.Background = null;
					previousBackground.Dispose();
				}

				// Null out BackgroundTintList to avoid that the MaterialButton custom background doesn't get tinted.
				platformView.BackgroundTintList = null;

				// Ripple selection color
				var rippleColor = Colors.White.WithAlpha(0.5f).ToPlatform();

				var w = platformView.Width;
				var h = platformView.Height;

				// Button background gradient
				GradientDrawable? gradientDrawable = background.ToGradientDrawable(w, h);
				gradientDrawable?.SetShape(ShapeType.Rectangle);
				gradientDrawable?.SetCornerRadius(button.CornerRadius);

				// Ripple mask
				float[] outerRadii = new float[8];
				const int defaultCornerRadius = 2; // Default value for Android material button.
				int cornerRadius = button.CornerRadius >= 0 ? button.CornerRadius : defaultCornerRadius;
				Arrays.Fill(outerRadii, cornerRadius);
				RoundRectShape shape = new RoundRectShape(outerRadii, null, null);
				Android.Graphics.Drawables.ShapeDrawable maskDrawable = new Android.Graphics.Drawables.ShapeDrawable(shape);

				var rippleDrawable = new RippleDrawable(ColorStateList.ValueOf(rippleColor), gradientDrawable, maskDrawable);
				rippleDrawable.SetId(0, BackgroundDrawableId);

				// Update the Stroke
				if (button.StrokeColor != null && button.StrokeThickness > 0)
					gradientDrawable?.SetStroke((int)platformView.Context.ToPixels(button.StrokeThickness), ColorStateList.ValueOf(button.StrokeColor.ToPlatform()));

				// Update the CornerRadius
				gradientDrawable?.SetCornerRadius(platformView.Context.ToPixels(cornerRadius));

				platformView.Background = rippleDrawable;
			}
		}
	}
}