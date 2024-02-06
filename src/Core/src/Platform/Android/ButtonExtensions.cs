using Android.Content.Res;
using Android.Graphics.Drawables;
using Microsoft.Maui.Graphics;
using AButton = Android.Widget.Button;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateBackground(this AButton platformView, IButton button)
		{
			platformView.UpdateBorderDrawable(button);
		}

		public static void UpdateStrokeColor(this AButton platformView, IButton button)
		{
			platformView.UpdateBorderDrawable(button);
		}

		public static void UpdateStrokeThickness(this AButton platformView, IButton button)
		{
			platformView.UpdateBorderDrawable(button);
		}

		public static void UpdateCornerRadius(this AButton platformView, IButton button)
		{
			platformView.UpdateBorderDrawable(button);
		}

		public static void UpdatePadding(this AButton platformControl, IPadding padding, Thickness? defaultPadding = null) =>
			UpdatePadding(platformControl, padding.Padding, defaultPadding);

		public static void UpdatePadding(this AButton platformControl, Thickness padding, Thickness? defaultPadding = null)
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
		
		internal static void UpdateBorderDrawable(this AButton platformView, IButton button)
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
				//platformView.BackgroundTintList = null;

				platformView.Background = mauiDrawable;

				var rippleColor = Colors.Yellow.ToPlatform();
				var rippleDrawable = new RippleDrawable(ColorStateList.ValueOf(rippleColor), mauiDrawable, null);
				platformView.Background = rippleDrawable;

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