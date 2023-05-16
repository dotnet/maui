using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Editor)]
	public partial class EditorTests : ControlsHandlerTestBase
	{

#if !IOS && !MACCATALYST
		// iOS is broken until this point
		// https://github.com/dotnet/maui/issues/3425
		[Theory]
		[InlineData(EditorAutoSizeOption.Disabled)]
		[InlineData(EditorAutoSizeOption.TextChanges)]
		public async Task AutoSizeInitializesCorrectly(EditorAutoSizeOption option)
		{
			var editor = new Editor
			{
				AutoSize = option,
				Text = "Test"
			};

			IView layout = new VerticalStackLayout()
			{
				Children =
				{
					editor
				}
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, (_) =>
			{
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(1000, 1000)));
				var initialHeight = editor.Height;

				editor.Text += Environment.NewLine + " Some new text" + Environment.NewLine;
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(1000, 1000)));

				if (option == EditorAutoSizeOption.Disabled)
					Assert.Equal(initialHeight, editor.Height);
				else
					Assert.True(initialHeight < editor.Height);

				return Task.CompletedTask;
			});
		}
#endif

		[Theory(DisplayName = "Text is Transformed Correctly at Initialization")]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Editor() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<EditorHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory(DisplayName = "Text is Transformed Correctly after Initialization")]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new Editor() { Text = text };
			var handler = await CreateHandlerAsync<EditorHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

#if WINDOWS
		// Only Windows needs the IsReadOnly workaround for MaxLength==0 to prevent text from being entered
		[Fact]
		public async Task MaxLengthIsReadOnlyValueTest()
		{
			Editor editor = new Editor();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<EditorHandler>(editor);
				var platformControl = GetPlatformControl(handler);

				editor.MaxLength = 0;
				Assert.True(platformControl.IsReadOnly);
				editor.IsReadOnly = false;
				Assert.True(platformControl.IsReadOnly);

				editor.MaxLength = 10;
				Assert.False(platformControl.IsReadOnly);
				editor.IsReadOnly = true;
				Assert.True(platformControl.IsReadOnly);
			});
		}
#endif

		[Theory(DisplayName = "CursorPosition Updates Correctly")]
		[InlineData(2, 5)]
		public async Task CursorPositionUpdatesCorrectly(int setValue, int unsetValue)
		{
			string text = "This is TEXT!";

			var editor = new Editor
			{
				Text = text
			};

			await ValidatePropertyUpdatesValue<int, EditorHandler>(
				editor,
				nameof(ITextInput.CursorPosition),
				GetPlatformCursorPosition,
				setValue,
				unsetValue
			);
		}

		[Theory(DisplayName = "CursorPosition is Capped to Text's Length")]
		[InlineData(30)]
		public async Task CursorPositionIsCapped(int initialPosition)
		{
			string text = "This is TEXT!";

			var editor = new Editor
			{
				Text = text,
				CursorPosition = initialPosition
			};

			await ValidatePropertyInitValue<int, EditorHandler>(
				editor,
				() => editor.CursorPosition,
				GetPlatformCursorPosition,
				text.Length);
		}

		[Theory(DisplayName = "Unset CursorPosition is kept at zero at initialization")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task UnsetCursorPositionKeepsToZeroOnInitialization(string text)
		{
			var editor = new Editor
			{
				Text = text
			};

			await ValidatePropertyInitValue<int, EditorHandler>(
				editor,
				() => editor.CursorPosition,
				GetPlatformCursorPosition,
				0);
		}

		[Theory(DisplayName = "Unset CursorPosition is kept at zero at initialization with TextTransform")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task UnsetCursorPositionKeepsToZeroOnInitializationWithTextTransform(string text)
		{
			var editor = new Editor
			{
				Text = text,
				TextTransform = TextTransform.Uppercase
			};

			await ValidatePropertyInitValue<int, EditorHandler>(
				editor,
				() => editor.CursorPosition,
				GetPlatformCursorPosition,
				0);
		}

		[Theory(DisplayName = "CursorPosition moves to the end on text change after initialization")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task CursorPositionMovesToTheEndOnTextChangeAfterInitialization(string text)
		{
			var editor = new Editor
			{
				Text = "Test"
			};

			await AttachAndRunFocusAffectedControl<Editor, EditorHandler>(editor, handler => editor.Text = text);

			Assert.Equal(text.Length, editor.CursorPosition);
		}

		[Theory(DisplayName = "SelectionLength Updates Correctly")]
		[InlineData(2, 5)]
		public async Task SelectionLengthUpdatesCorrectly(int setValue, int unsetValue)
		{
			string text = "This is TEXT!";

			var editor = new Editor
			{
				Text = text,
			};

			await ValidatePropertyUpdatesValue<int, EditorHandler>(
				editor,
				nameof(IEditor.SelectionLength),
				GetPlatformSelectionLength,
				setValue,
				unsetValue
			);
		}

		[Theory(DisplayName = "SelectionLength is Capped to Text Length")]
		[InlineData(30)]
		public async Task SelectionLengthIsCapped(int selectionLength)
		{
			string text = "This is TEXT!";

			var editor = new Editor
			{
				Text = text,
				SelectionLength = selectionLength
			};

			await ValidatePropertyInitValue<int, EditorHandler>(
				editor,
				() => editor.SelectionLength,
				GetPlatformSelectionLength,
				text.Length);
		}

		[Theory(DisplayName = "Unset SelectionLength is kept at zero at initialization")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task UnsetSelectionLengthKeepsToZeroOnInitialization(string text)
		{
			var editor = new Editor
			{
				Text = text
			};

			await ValidatePropertyInitValue<int, EditorHandler>(
				editor,
				() => editor.SelectionLength,
				GetPlatformSelectionLength,
				0);
		}

		[Theory(DisplayName = "Unset SelectionLength is kept at zero at initialization with TextTransform")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task UnsetSelectionLengthKeepsToZeroOnInitializationWithTextTransform(string text)
		{
			var editor = new Editor
			{
				Text = text,
				TextTransform = TextTransform.Uppercase
			};

			await ValidatePropertyInitValue<int, EditorHandler>(
				editor,
				() => editor.SelectionLength,
				GetPlatformSelectionLength,
				0);
		}

		[Theory(DisplayName = "SelectionLength is kept at zero on text change after initialization")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task SelectionLengthMovesToTheEndOnTextChangeAfterInitialization(string text)
		{
			var editor = new Editor
			{
				Text = "Test"
			};

			await AttachAndRunFocusAffectedControl<Editor, EditorHandler>(editor, handler => editor.Text = text);

			Assert.Equal(0, editor.SelectionLength);
		}

		[Category(TestCategory.Editor)]
		[Category(TestCategory.TextInput)]
		public class EditorTextInputTests : TextInputTests<EditorHandler, Editor>
		{
			protected override int GetPlatformSelectionLength(EditorHandler handler) =>
				EditorTests.GetPlatformSelectionLength(handler);

			protected override int GetPlatformCursorPosition(EditorHandler handler) =>
				EditorTests.GetPlatformCursorPosition(handler);
		}

		[Collection(RunInNewWindowCollection)]
		[Category(TestCategory.Editor)]
		[Category(TestCategory.TextInput)]
		public class EditorTextInputFocusTests : TextInputFocusTests<EditorHandler, Editor>
		{
		}
	}
}
