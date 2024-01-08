using Android.Widget;
using Google.Android.Material.Button;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateBackground(this MaterialButton platformView, IButton button)
		{
			platformView.UpdateBorderDrawable(button);
		}

		public static void UpdateStrokeColor(this MaterialButton platformView, IButton button)
		{
			if (platformView.Background is BorderDrawable)
			{
				platformView.UpdateBorderDrawable(button);
				return;
			}

			if (button is IButtonStroke buttonStroke && buttonStroke.StrokeColor is Color stroke)
				platformView.StrokeColor = ColorStateListExtensions.CreateButton(stroke.ToPlatform());
		}

		public static void UpdateStrokeThickness(this MaterialButton platformView, IButton button)
		{
			if (platformView.Background is BorderDrawable)
			{
				platformView.UpdateBorderDrawable(button);
				return;
			}

			if (button is IButtonStroke buttonStroke && buttonStroke.StrokeThickness >= 0)
				platformView.StrokeWidth = (int)platformView.Context.ToPixels(buttonStroke.StrokeThickness);
		}

		public static void UpdateCornerRadius(this MaterialButton platformView, IButton button)
		{
			if (platformView.Background is BorderDrawable)
			{
				platformView.UpdateBorderDrawable(button);
				return;
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
				if (platformView.Background is BorderDrawable previousBackground)
				{
					platformView.Background = null;
					previousBackground.Dispose();
				}

				var mauiDrawable = new BorderDrawable(platformView.Context);

				// Null out BackgroundTintList to avoid that the MaterialButton custom background doesn't get tinted.
				platformView.BackgroundTintList = null;

				platformView.Background = mauiDrawable;

				mauiDrawable.SetBackground(background);

				if (button.StrokeColor != null)
					mauiDrawable.SetBorderBrush(new SolidPaint { Color = button.StrokeColor });

				if (button.StrokeThickness > 0)
					mauiDrawable.SetBorderWidth(button.StrokeThickness);

				if (button.CornerRadius >= 0)
					mauiDrawable.SetCornerRadius(button.CornerRadius);
				else
				{
					const int defaultCornerRadius = 2; // Default value for Android material button.
					mauiDrawable.SetCornerRadius(platformView.Context.ToPixels(defaultCornerRadius));
				}
			}
		}
	}
}