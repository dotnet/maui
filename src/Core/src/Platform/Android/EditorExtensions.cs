using Android.Widget;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdateText(this EditText editText, IEditor editor)
		{
			editText.Text = editor.Text;
		}
	}
}
