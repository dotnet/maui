using UIKit;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdateText(this UITextView textView, IEditor editor)
		{
			string text = editor.Text;

			if (textView.Text != text)
			{
				textView.Text = text;
			}

			var placeholderLabel = textView.FindDescendantView<UILabel>();

			if (placeholderLabel == null)
				return;

			placeholderLabel.Hidden = !string.IsNullOrEmpty(textView.Text);
		}

		public static void UpdatePlaceholder(this UITextView textView, IEditor editor)
		{
			var placeholderLabel = textView.FindDescendantView<UILabel>();

			if (placeholderLabel == null)
				return;

			placeholderLabel.Text = editor.Placeholder;
			placeholderLabel.SizeToFit();
		}
	}
}
