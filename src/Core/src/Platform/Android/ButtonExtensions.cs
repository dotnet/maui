using Android.Widget;
using Google.Android.Material.Button;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateStrokeColor(this MaterialButton platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeColor is Color stroke)
				platformButton.StrokeColor = ColorStateListExtensions.CreateButton(stroke.ToPlatform());
		}

		public static void UpdateStrokeThickness(this MaterialButton platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeThickness >= 0)
				platformButton.StrokeWidth = (int)platformButton.Context.ToPixels(buttonStroke.StrokeThickness);
		}

		public static void UpdateCornerRadius(this MaterialButton platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.CornerRadius >= 0)
				platformButton.CornerRadius = (int)platformButton.Context.ToPixels(buttonStroke.CornerRadius);
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
	}
}