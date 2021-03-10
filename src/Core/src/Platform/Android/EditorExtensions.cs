using Android.Widget;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdateText(this EditText editText, IEditor editor)
		{
			string text = editor.Text;

			if (editText.Text == text)
				return;

			editText.Text = text;
			editText.SetSelection(text.Length);
		}
	}
}