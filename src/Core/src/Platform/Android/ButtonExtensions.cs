using Android.Widget;
using Google.Android.Material.Button;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateStrokeColor(this MaterialButton nativeButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeColor is Color stroke)
				nativeButton.StrokeColor = ColorStateListExtensions.CreateButton(stroke.ToPlatform());
		}

		public static void UpdateStrokeThickness(this MaterialButton nativeButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeThickness >= 0)
				nativeButton.StrokeWidth = (int)nativeButton.Context.ToPixels(buttonStroke.StrokeThickness);
		}

		public static void UpdateCornerRadius(this MaterialButton nativeButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.CornerRadius >= 0)
				nativeButton.CornerRadius = (int)nativeButton.Context.ToPixels(buttonStroke.CornerRadius);
		}

		public static void UpdatePadding(this Button nativeControl, IPadding padding, Thickness? defaultPadding = null) =>
			UpdatePadding(nativeControl, padding.Padding, defaultPadding);

		public static void UpdatePadding(this Button nativeControl, Thickness padding, Thickness? defaultPadding = null)
		{
			var context = nativeControl.Context;
			if (context == null)
				return;

			if (padding.IsNaN)
				padding = defaultPadding ?? Thickness.Zero;

			padding = context.ToPixels(padding);

			nativeControl.SetPadding(
				(int)padding.Left,
				(int)padding.Top,
				(int)padding.Right,
				(int)padding.Bottom);
		}
	}
}