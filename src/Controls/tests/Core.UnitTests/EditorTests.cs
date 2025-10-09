using System.Windows.Input;
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

		[Fact]
		public void CompletedCommandTest()
		{
			var editor = new Editor();
			var parameter = new object();
			object commandExecuted = null;
			ICommand cmd = new Command(() => commandExecuted = parameter);

			editor.ReturnCommand = cmd;
			editor.ReturnCommandParameter = parameter;
			editor.SendCompleted();

			Assert.Equal(commandExecuted, parameter);
		}

		[Fact]
		public void CompletedCommandNotExecutedWhenDisabled()
		{
			int counter = 0;
			var editor = new Editor();
			Command cmd = new Command(() => counter++);

			editor.ReturnCommand = cmd;
			editor.IsEnabled = false;
			editor.SendCompleted();

			Assert.Equal(0, counter);
		}

		[Fact]
		public void CompletedCommandNotExecutedWhenCanExecuteReturnsFalse()
		{
			int counter = 0;
			var editor = new Editor();
			Command cmd = new Command(() => counter++, () => false);

			editor.ReturnCommand = cmd;
			editor.SendCompleted();

			Assert.Equal(0, counter);
		}

		[Fact]
		public void CompletedEventAndCommandBothFire()
		{
			var editor = new Editor();
			int eventFired = 0;
			int commandFired = 0;

			editor.Completed += (s, e) => eventFired++;
			editor.ReturnCommand = new Command(() => commandFired++);

			editor.SendCompleted();

			Assert.Equal(1, eventFired);
			Assert.Equal(1, commandFired);
		}
	}
}
