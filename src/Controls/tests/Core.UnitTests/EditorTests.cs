using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class EditorTests : BaseTestFixture
	{
		[Theory]
		[InlineData("Hi", "My text has changed")]
		[InlineData(null, "My text has changed")]
		[InlineData("Hi", null)]
		public void EditorTextChangedEventArgs(string initialText, string finalText)
		{
			var editor = new Editor
			{
				Text = initialText
			};

			Editor editorFromSender = null;
			string oldText = null;
			string newText = null;

			editor.TextChanged += (s, e) =>
			{
				editorFromSender = (Editor)s;
				oldText = e.OldTextValue;
				newText = e.NewTextValue;
			};

			editor.Text = finalText;

			Assert.Equal(editor, editorFromSender);
			Assert.Equal(initialText, oldText);
			Assert.Equal(finalText, newText);
		}

		[Theory]
		[InlineData(true)]
		public void IsReadOnlyTest(bool isReadOnly)
		{
			Editor editor = new Editor();
			editor.SetValue(InputView.IsReadOnlyProperty, isReadOnly);
			Assert.Equal(isReadOnly, editor.IsReadOnly);
		}

		[Fact]
		public void IsReadOnlyDefaultValueTest()
		{
			Editor editor = new Editor();
			Assert.False(editor.IsReadOnly);
		}
	}
}
