using System;
using Android.Content;
using Android.Content.Res;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Google.Android.Material.Button;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateStrokeColor(this MaterialButton nativeButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeColor != null)
				nativeButton.StrokeColor = buttonStroke.StrokeColor.ToAndroidPreserveDisabled(nativeButton.StrokeColor);
		}

		public static void UpdateStrokeThickness(this MaterialButton nativeButton, IButtonStroke buttonStroke)
		{
			nativeButton.StrokeWidth = (int)(buttonStroke.StrokeThickness * (nativeButton?.Resources?.DisplayMetrics?.Density ?? 1));
		}

		public static void UpdateCornerRadius(this MaterialButton nativeButton, IButtonStroke buttonStroke)
		{
			nativeButton.CornerRadius = buttonStroke.CornerRadius;
		}

		public static void UpdatePadding(this AppCompatButton appCompatButton, IButton button, Thickness? defaultPadding = null)
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