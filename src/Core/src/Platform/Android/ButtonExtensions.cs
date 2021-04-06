using Android.Content.Res;
using Android.Util;
using AndroidX.AppCompat.Widget;
using XColor = Microsoft.Maui.Color;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this AppCompatButton appCompatButton, IButton button) =>
			appCompatButton.Text = button.Text;

		public static void UpdateTextColor(this AppCompatButton appCompatButton, IButton button) =>
			appCompatButton.UpdateTextColor(button.TextColor, appCompatButton.TextColors);

		public static void UpdateTextColor(this AppCompatButton button, XColor color, ColorStateList? defaultColor)
		{
			if (color.IsDefault)
				button.SetTextColor(defaultColor);
			else
				button.SetTextColor(color.ToNative());
		}

		public static void UpdateTextColor(this AppCompatButton appCompatButton, IButton button, XColor defaultColor) =>
			appCompatButton.SetTextColor(button.TextColor.Cleanse(defaultColor).ToNative());

		public static void UpdateCharacterSpacing(this AppCompatButton appCompatButton, IButton button) =>
			appCompatButton.LetterSpacing = button.CharacterSpacing.ToEm();

		public static void UpdateFont(this AppCompatButton appCompatButton, IButton button, IFontManager fontManager)
		{
			var font = button.Font;

			var tf = fontManager.GetTypeface(font);
			appCompatButton.Typeface = tf;

			var sp = fontManager.GetScaledPixel(font);
			appCompatButton.SetTextSize(ComplexUnitType.Sp, sp);
		}

		public static void UpdatePadding(this AppCompatButton appCompatButton, IButton button, Thickness? defaultPadding = null)
		{
			var context = appCompatButton.Context;
			if (context == null)
				return;

			var padding = defaultPadding ?? new Thickness();
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

		static XColor Cleanse(this XColor color, XColor defaultColor) => color.IsDefault ? defaultColor : color;
	}
}
