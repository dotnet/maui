using Gtk;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdateText(this TextView nativeEditor, IEditor editor)
		{
			var text = editor.Text;
			TextBuffer buffer = nativeEditor.Buffer;

			if (buffer.Text != text)
				buffer.Text = text;
		}
	}
}