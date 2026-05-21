using Android.Widget;
using Google.Android.Material.Button;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using R = Android.Resource;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateBackground(this MaterialButton platformView, IButton button) =>
			platformView.UpdateButtonBackground(button);

		public static void UpdateStrokeColor(this MaterialButton platformView, IButton button) =>
			platformView.UpdateButtonStroke(button);

		public static void UpdateStrokeThickness(this MaterialButton platformView, IButton button) =>
			platformView.UpdateButtonStroke(button);

		public static void UpdateCornerRadius(this MaterialButton platformView, IButton button) =>
			platformView.UpdateButtonStroke(button);

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

		internal static void UpdateButtonStroke(this MaterialButton platformView, IButton button)
		{
			if (!platformView.UpdateMauiRippleDrawableStroke(button))
			{
				// Fall back to the default mechanism. This may be due to the fact that the background
				// is not a "MAUI" background, so we need to update the stroke on the button itself.

				var (width, color, radius) = button.GetStrokeProperties(platformView.Context!, true);

				platformView.StrokeColor = color;

				platformView.StrokeWidth = width;

				platformView.CornerRadius = radius;
			}
		}

		internal static void UpdateButtonBackground(this MaterialButton platformView, IButton button)
		{
			platformView.UpdateMauiRippleDrawableBackground(
				button.Background,
				button,
				() =>
				{
					// Copy the tints from a temporary button.
					// TODO: optimize this to avoid creating a new button every time.

					var context = platformView.Context!;
					using var btn = new MaterialButton(context);
					var defaultTintList = btn.BackgroundTintList;
					var defaultColor = defaultTintList?.GetColorForState([R.Attribute.StateEnabled], AColor.Black);

					return defaultColor ?? AColor.Black;
				},
				() =>
				{
					// If some theme or user value has been set, we can override the default, white
					// ripple color using this button property.
					return platformView.RippleColor;
				},
				() =>
				{
					// We have a background, so we need to null out the tint list to avoid the tint
					// overriding the background.
					platformView.BackgroundTintList = null;
				});
		}

		public static void UpdateRippleColor(this MaterialButton platformView, Color? rippleColor)
		{
			if (platformView.Background is global::Android.Graphics.Drawables.RippleDrawable ripple)
			{
				if (rippleColor?.ToPlatform() is not null)
				{
					ripple.SetColor(global::Android.Content.Res.ColorStateList.ValueOf(rippleColor.ToPlatform()));
				}
				else
				{
					ripple.ClearColorFilter();
				}
			}
		}
	}
}