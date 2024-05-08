using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Widget;
using Google.Android.Material.ImageView;
using Google.Android.Material.Shape;

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
			platformButton.SetContentPadding(imageButton);

			// see: https://github.com/material-components/material-components-android/issues/2063
			await Task.Yield();
			platformButton.SetContentPadding(imageButton);
		}

		internal static void SetContentPadding(this ShapeableImageView platformButton, IImageButton imageButton)
		{
			var imageView = platformButton as ImageView;

			if (imageView is not null)
			{
				var bitmapDrawable = imageView.Drawable as BitmapDrawable;

				// Without ImageSource we do not apply Padding, although since there is no content
				// there are no differences.
				if (bitmapDrawable is null)
					return;

				var backgroundBounds = bitmapDrawable.Bounds;

				var padding = imageButton.Padding;

				bitmapDrawable.SetBounds(
					backgroundBounds.Left + (int)platformButton.Context.ToPixels(padding.Left),
					backgroundBounds.Top + (int)platformButton.Context.ToPixels(padding.Top),
					backgroundBounds.Right - (int)platformButton.Context.ToPixels(padding.Right),
					backgroundBounds.Bottom - (int)platformButton.Context.ToPixels(padding.Bottom));
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
			platformView.UpdateMauiRippleDrawableBackground(button);
		}
	}
}
