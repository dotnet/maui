﻿using System.Threading.Tasks;
using Google.Android.Material.ImageView;
using Google.Android.Material.Shape;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ImageButtonExtensions
	{
		// TODO: NET9 should this be public?
		internal static void UpdateBackground(this ShapeableImageView platformButton, IImageButton imageButton) =>
			platformButton.UpdateButtonBackground(imageButton);

		public static void UpdateStrokeColor(this ShapeableImageView platformButton, IButtonStroke buttonStroke) =>
			platformButton.UpdateButtonStroke(buttonStroke);

		public static void UpdateStrokeThickness(this ShapeableImageView platformButton, IButtonStroke buttonStroke) =>
			platformButton.UpdateButtonStroke(buttonStroke);

		public static void UpdateCornerRadius(this ShapeableImageView platformButton, IButtonStroke buttonStroke) =>
			platformButton.UpdateButtonStroke(buttonStroke);

		public static async void UpdatePadding(this ShapeableImageView platformButton, IImageButton imageButton)
		{
			var padding = platformButton.Context!.ToPixels(imageButton.Padding);
			var (strokeWidth, _, _) = imageButton.GetStrokeProperties(platformButton.Context!, true);
			int additionalPadding = strokeWidth;
			padding = new Thickness(padding.Left + additionalPadding, padding.Top + additionalPadding, padding.Right + additionalPadding, padding.Bottom + additionalPadding);

			// The simple operation we are trying to do.
			platformButton.SetContentPadding((int)padding.Left, (int)padding.Top, (int)padding.Right, (int)padding.Bottom);

			// Because there is only a single padding property, we need to reset the padding to 0 otherwise we
			// probably will get a double padding. Trust me. I've seen it happen. It's not pretty.
			platformButton.SetPadding(0, 0, 0, 0);

			// The padding has a few issues, but setting and then resetting after some calculations
			// are done seems to work. This is a workaround for the following issue:
			// https://github.com/material-components/material-components-android/issues/2063
			await Task.Yield();

			if (!platformButton.IsAlive())
				return;

			// We must re-set all the paddings because the first time was not hard enough.
			platformButton.SetContentPadding((int)padding.Left, (int)padding.Top, (int)padding.Right, (int)padding.Bottom);
			platformButton.SetPadding(0, 0, 0, 0);

			// Just like before, the bugs are not done. This needs to trigger a re-calculation of
			// the shape appearance mode to avoid clipping issues.
			if (platformButton.Drawable is not null)
			{
				platformButton.ShapeAppearanceModel =
					platformButton.ShapeAppearanceModel
						.ToBuilder()
						.Build();
			}
		}

		internal static void UpdateButtonStroke(this ShapeableImageView platformView, IButtonStroke button)
		{
			if (!platformView.UpdateMauiRippleDrawableStroke(button))
			{
				// Fall back to the default mechanism. This may be due to the fact that the background
				// is not a "MAUI" background, so we need to update the stroke on the button itself.

				var (width, color, radius) = button.GetStrokeProperties(platformView.Context!, true);

				platformView.StrokeColor = color;

				platformView.StrokeWidth = width;

				platformView.ShapeAppearanceModel =
					platformView.ShapeAppearanceModel
						.ToBuilder()
						.SetTopLeftCorner(CornerFamily.Rounded, radius)
						.SetTopRightCorner(CornerFamily.Rounded, radius)
						.SetBottomLeftCorner(CornerFamily.Rounded, radius)
						.SetBottomRightCorner(CornerFamily.Rounded, radius)
						.Build();
			}
		}

		internal static void UpdateButtonBackground(this ShapeableImageView platformView, IImageButton button)
		{
			platformView.UpdateMauiRippleDrawableBackground(
				button.Background ?? new SolidPaint(Colors.Transparent), // transparent to force some background
				button,
				beforeSet: () =>
				{
					// We have a background, so we need to remove the things that were set on the
					// platform view as they are now in the drawable.

					platformView.StrokeColor = null;

					platformView.StrokeWidth = 0;

					platformView.ShapeAppearanceModel =
						platformView.ShapeAppearanceModel
							.ToBuilder()
							.SetAllCornerSizes(0)
							.Build();
				});
		}
	}
}
