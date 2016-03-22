using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class EditorTests : BaseTestFixture
	{
		[TestCase ("Hi", "My text has changed")]
		[TestCase (null, "My text has changed")]
		[TestCase ("Hi", null)]
		public void EditorTextChangedEventArgs (string initialText, string finalText)
		{
			var editor = new Editor {
				Text = initialText
			};

			Editor editorFromSender = null;
			string oldText = null;
			string newText = null;

			editor.TextChanged += (s, e) => {
				editorFromSender = (Editor)s;
				oldText = e.OldTextValue;
				newText = e.NewTextValue;
			};

			editor.Text = finalText;

			Assert.AreEqual (editor, editorFromSender);
			Assert.AreEqual (initialText, oldText);
			Assert.AreEqual (finalText, newText);
		}
	}
}
