using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Widget;
using Google.Android.Material.ImageView;
using Google.Android.Material.Shape;
using Java.Util;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ImageButtonExtensions
	{
		const int MauiBackgroundDrawableId = 1002;

		// TODO: NET8 should this be public?
		internal static void UpdateBackground(this ShapeableImageView platformButton, IImageButton imageButton)
		{
			Paint? paint = imageButton.Background;

			// Ripple Background
			var gradientDrawable = paint?.ToDrawable(platformButton.Context);

			if (gradientDrawable is not null)
			{
				// Ripple Mask
				float[] outerRadii = new float[8];
				float cornerRadius = platformButton.Context.ToPixels(imageButton.CornerRadius);
				Arrays.Fill(outerRadii, cornerRadius);
				RoundRectShape shape = new RoundRectShape(outerRadii, null, null);
				Android.Graphics.Drawables.ShapeDrawable maskDrawable = new Android.Graphics.Drawables.ShapeDrawable(shape);

				// Ripple SelectionColor
				var rippleColor = ColorStateList.ValueOf(Colors.White.WithAlpha(0.5f).ToPlatform());

				var rippleDrawable = new RippleDrawable(rippleColor, gradientDrawable, maskDrawable);
				rippleDrawable.SetId(0, MauiBackgroundDrawableId);

				platformButton.Background = rippleDrawable;
			}
			else
			{
				platformButton.Background = null;
			}
		}

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
			var radius = platformButton.Context.ToPixels(buttonStroke.CornerRadius);

			platformButton.ShapeAppearanceModel =
				platformButton.ShapeAppearanceModel
				.ToBuilder()
				.SetTopLeftCorner(CornerFamily.Rounded, radius)
				.SetTopRightCorner(CornerFamily.Rounded, radius)
				.SetBottomLeftCorner(CornerFamily.Rounded, radius)
				.SetBottomRightCorner(CornerFamily.Rounded, radius)
				.Build();
		}

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
	}
}
