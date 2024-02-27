using Gtk;

namespace Microsoft.Maui
{

	public static class EditorExtensions
	{

		public static void UpdateText(this TextView platformView, IEditor editor)
		{
			var text = editor.Text;
			var buffer = platformView.Buffer;

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