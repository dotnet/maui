﻿using Google.Android.Material.ImageView;
using Google.Android.Material.Shape;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ImageButtonExtensions
	{
		public static void UpdateStrokeColor(this ShapeableImageView platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeColor is Color stroke)
				platformButton.StrokeColor = ColorStateListExtensions.CreateButton(stroke.ToPlatform());
		}

		public static void UpdateStrokeThickness(this ShapeableImageView platformButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeThickness >= 0)
				platformButton.StrokeWidth = (int)platformButton.Context.ToPixels(buttonStroke.StrokeThickness);
		}

		public static void UpdateCornerRadius(this ShapeableImageView platformButton, IButtonStroke buttonStroke)
		{
			platformButton.ShapeAppearanceModel =
				platformButton.ShapeAppearanceModel
				.ToBuilder()
				.SetTopLeftCorner(CornerFamily.Rounded, buttonStroke.CornerRadius)
				.SetTopRightCorner(CornerFamily.Rounded, buttonStroke.CornerRadius)
				.SetBottomLeftCorner(CornerFamily.Rounded, buttonStroke.CornerRadius)
				.SetBottomRightCorner(CornerFamily.Rounded, buttonStroke.CornerRadius)
				.Build();
		}

		public static void UpdatePadding(this ShapeableImageView platformButton, IImageButton imageButton)
		{
			platformButton.SetContentPadding(imageButton);

			// NOTE(jpr): post on handler to get around an Android Framework bug.
			// see: https://github.com/material-components/material-components-android/issues/2063
			platformButton.Post(() =>
			{
				platformButton.SetContentPadding(imageButton);
			});
		}

		internal static void SetContentPadding(this ShapeableImageView platformButton, IImageButton imageButton)
		{
			var padding = imageButton.Padding;

			platformButton.SetContentPadding(
				(int)platformButton.Context.ToPixels(padding.Left),
				(int)platformButton.Context.ToPixels(padding.Top),
				(int)platformButton.Context.ToPixels(padding.Right),
				(int)platformButton.Context.ToPixels(padding.Bottom)
			);
		}
	}
}