using UIKit;

namespace Microsoft.Maui
{
	public static class EditorExtensions
	{
		public static void UpdateText(this UITextView textView, IEditor editor)
		{
			textView.Text = editor.Text;
		}
	}
}