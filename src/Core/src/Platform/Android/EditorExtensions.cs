using Android.Content.Res;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdatePlaceholder(this AppCompatEditText editText, IEditor editor)
		{
			if (editText.Hint == editor.Placeholder)
				return;

			editText.Hint = editor.Placeholder;
		}

		public static void UpdatePlaceholderColor(this AppCompatEditText editText, IEditor editor, ColorStateList? defaultColor)
		{
			var placeholderTextColor = editor.PlaceholderColor;
			if (placeholderTextColor.IsDefault)
			{
				editText.SetHintTextColor(defaultColor);
			}
			else
			{
				var androidColor = placeholderTextColor.ToNative();

				if (!editText.HintTextColors.IsOneColor(ColorExtensions.States, androidColor))
				{
					var acolor = androidColor.ToArgb();
					editText.SetHintTextColor(new ColorStateList(ColorExtensions.States, new[] { acolor, acolor }));
				}
			}
		}
	}
}
