using Android.Content.Res;
using Xamarin.Forms;

namespace Xamarin.Platform
{
	public static class ViewExtensions
	{
		public static void SetTextColor(this AndroidX.AppCompat.Widget.AppCompatButton button, Color color, Color defaultColor)
			=> button.SetTextColor(color.Cleanse(defaultColor).ToNative());

		public static void SetTextColor(this AndroidX.AppCompat.Widget.AppCompatButton button, Color color, ColorStateList defaultColor)
		{
			if (color.IsDefault)
				button.SetTextColor(defaultColor);
			else
				button.SetTextColor(color.ToNative());
		}

		static Color Cleanse(this Color color, Color defaultColor) => color.IsDefault ? defaultColor : color;

		public static void SetText(this AndroidX.AppCompat.Widget.AppCompatButton button, string text)
			=> button.Text = text;
	}
}