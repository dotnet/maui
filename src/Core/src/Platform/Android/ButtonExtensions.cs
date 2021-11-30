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
				nativeButton.StrokeColor = ColorStateListExtensions.CreateButton(stroke.ToNative());
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

		public static void UpdatePadding(this Button appCompatButton, IButton button, Thickness? defaultPadding = null)
		{
			var context = appCompatButton.Context;
			if (context == null)
				return;

			// TODO: have a way to use default padding
			//       Windows keeps the default as a base but this is also wrong.
			// var padding = defaultPadding ?? new Thickness();
			var padding = new Thickness();
			padding.Left += context.ToPixels(button.Padding.Left);
			padding.Top += context.ToPixels(button.Padding.Top);
			padding.Right += context.ToPixels(button.Padding.Right);
			padding.Bottom += context.ToPixels(button.Padding.Bottom);

			appCompatButton.SetPadding(
				(int)padding.Left,
				(int)padding.Top,
				(int)padding.Right,
				(int)padding.Bottom);
		}
	}
}