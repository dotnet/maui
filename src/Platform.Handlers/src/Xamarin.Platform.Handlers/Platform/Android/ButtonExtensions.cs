using Android.Content.Res;
using AndroidX.AppCompat.Widget;
using Xamarin.Forms;

namespace Xamarin.Platform
{
	public static class ButtonExtensions
	{
		public static void UpdateTextColor(this AndroidX.AppCompat.Widget.AppCompatButton button, Color color, ColorStateList? defaultColor)
		{
			if (color.IsDefault)
				button.SetTextColor(defaultColor);
			else
				button.SetTextColor(color.ToNative());
		}

		public static void UpdateTextColor(this AppCompatButton appCompatButton, IButton button) =>
			appCompatButton.UpdateTextColor(button.TextColor, appCompatButton.TextColors);

		public static void UpdateTextColor(this AppCompatButton appCompatButton, IButton button, Color defaultColor) =>
			appCompatButton.SetTextColor(button.TextColor.Cleanse(defaultColor).ToNative());

		public static void UpdateText(this AppCompatButton appCompatButton, IButton button) =>
			appCompatButton.Text = button.Text;

		static Color Cleanse(this Color color, Color defaultColor) => color.IsDefault ? defaultColor : color;
	}
}
