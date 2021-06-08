using Gtk;

namespace Microsoft.Maui
{

	public static class EditorExtensions
	{

		public static void UpdateText(this TextView nativeEditor, IEditor editor)
		{
			var text = editor.Text;
			var buffer = nativeEditor.Buffer;

			if (buffer.Text == text) return;

			if (text == null)
			{
				buffer.Clear();
			}

			else
			{
				buffer.Text = text;
			}

		}

	}

}