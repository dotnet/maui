using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdateText(this AppCompatEditText editText, IEditor editor)
		{
			string text = editor.Text;

			if (editText.Text == text)
				return;

			editText.Text = text;

			if (string.IsNullOrEmpty(text))
				return;

			editText.SetSelection(text.Length);
		}

		public static void UpdatePlaceholder(this AppCompatEditText editText, IEditor editor)
		{
			if (editText.Hint == editor.Placeholder)
				return;

			editText.Hint = editor.Placeholder;
		}
	}
}
