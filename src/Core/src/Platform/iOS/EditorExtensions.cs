using Microsoft.Maui.Platform.iOS;
using UIKit;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdatePlaceholder(this MauiTextView textView, IEditor editor)
		{
			textView.PlaceholderText = editor.Placeholder;
		}

		public static void UpdatePlaceholderColor(this MauiTextView textView, IEditor editor, UIColor? defaultPlaceholderColor)
		{
			Color placeholderColor = editor.PlaceholderColor;
			if (placeholderColor.IsDefault)
				textView.PlaceholderTextColor = defaultPlaceholderColor;
			else
				textView.PlaceholderTextColor = placeholderColor.ToNative();
		}
	}
}
