using Google.Android.Material.ImageView;
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
			var radius = nativeButton.Context.ToPixels(buttonStroke.CornerRadius);
			nativeButton.ShapeAppearanceModel =
				nativeButton.ShapeAppearanceModel
				.ToBuilder()
				.SetTopLeftCorner(CornerFamily.Rounded, radius)
				.SetTopRightCorner(CornerFamily.Rounded, radius)
				.SetBottomLeftCorner(CornerFamily.Rounded, radius)
				.SetBottomRightCorner(CornerFamily.Rounded, radius)
				.Build();
		}
	}
}