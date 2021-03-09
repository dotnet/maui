using Android.Content.Res;
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
				
		static XColor Cleanse(this XColor color, XColor defaultColor) => color.IsDefault ? defaultColor : color;
	}
}
