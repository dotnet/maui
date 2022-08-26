using Tizen.UIExtensions.NUI;
using NColor = Tizen.NUI.Color;

namespace Microsoft.Maui.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateTextColor(this Button platformButton, ITextStyle button)
		{
			platformButton.TextColor = button.TextColor.ToPlatform();
		}

		public static void UpdateText(this Button platformButton, IText button)
		{
			platformButton.Text = button.Text ?? "";
		}

		public static void UpdateCharacterSpacing(this Button platformButton, ITextStyle button)
		{
			platformButton.TextLabel.CharacterSpacing = button.CharacterSpacing.ToScaledPixel();
		}

		public static void UpdateFont(this Button platformButton, ITextStyle label, IFontManager fontManager)
		{
			platformButton.FontSize = label.Font.Size > 0 ? label.Font.Size.ToScaledPoint() : 14d.ToScaledPoint();
			platformButton.FontAttributes = label.Font.GetFontAttributes();
			platformButton.FontFamily = fontManager.GetFontFamily(label.Font.Family) ?? "";
		}

		public static void UpdateStrokeColor(this Button platformButton, IButtonStroke button)
		{
			platformButton.BorderlineColor = button.StrokeColor.ToNUIColor() ?? NColor.Transparent;
		}

		public static void UpdateStrokeThickness(this Button platformButton, IButtonStroke button)
		{
			platformButton.BorderlineWidth = button.StrokeThickness.ToScaledPixel();
		}

		public static void UpdateCornerRadius(this Button platformButton, IButtonStroke button)
		{
			if (button.CornerRadius != -1)
			{
				platformButton.CornerRadius = ((double)button.CornerRadius).ToScaledPixel();
			}
		}
	}
}
