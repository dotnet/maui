using Microsoft.Maui.Graphics;
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
			=> textView.PlaceholderTextColor = editor.PlaceholderColor?.ToNative() ?? defaultPlaceholderColor;
	}
}
