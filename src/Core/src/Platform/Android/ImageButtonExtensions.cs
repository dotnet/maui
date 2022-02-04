using Google.Android.Material.ImageView;
using Google.Android.Material.Shape;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public static class ImageButtonExtensions
	{
		public static void UpdateStrokeColor(this ShapeableImageView nativeButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeColor is Color stroke)
				nativeButton.StrokeColor = ColorStateListExtensions.CreateButton(stroke.ToPlatform());
		}

		public static void UpdateStrokeThickness(this ShapeableImageView nativeButton, IButtonStroke buttonStroke)
		{
			if (buttonStroke.StrokeThickness >= 0)
				nativeButton.StrokeWidth = (int)nativeButton.Context.ToPixels(buttonStroke.StrokeThickness);
		}

		public static void UpdateCornerRadius(this ShapeableImageView nativeButton, IButtonStroke buttonStroke)
		{
			nativeButton.ShapeAppearanceModel =
				nativeButton.ShapeAppearanceModel
				.ToBuilder()
				.SetTopLeftCorner(CornerFamily.Rounded, buttonStroke.CornerRadius)
				.SetTopRightCorner(CornerFamily.Rounded, buttonStroke.CornerRadius)
				.SetBottomLeftCorner(CornerFamily.Rounded, buttonStroke.CornerRadius)
				.SetBottomRightCorner(CornerFamily.Rounded, buttonStroke.CornerRadius)
				.Build();
		}
	}
}