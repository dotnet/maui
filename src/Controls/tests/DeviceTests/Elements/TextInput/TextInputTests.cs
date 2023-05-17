using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TextInput)]
	public abstract partial class TextInputTests<THandler, TView> : ControlsHandlerTestBase
		where THandler : class, IViewHandler, IPlatformViewHandler, new()
		where TView : InputView, IView, ITextInput, new()
	{
		protected abstract int GetPlatformCursorPosition(THandler handler);

		protected abstract int GetPlatformSelectionLength(THandler handler);

		protected abstract Task<string> GetPlatformText(THandler handler);

		[Theory(DisplayName = "Text is Transformed Correctly at Initialization")]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new TView() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<THandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory(DisplayName = "Text is Transformed Correctly after Initialization")]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new TView() { Text = text };
			var handler = await CreateHandlerAsync<THandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

		[Theory(DisplayName = "Unset CursorPosition is kept at zero at initialization with TextTransform")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task UnsetCursorPositionKeepsToZeroOnInitializationWithTextTransform(string text)
		{
			var editor = new TView
			{
				Text = text,
				TextTransform = TextTransform.Uppercase
			};

			await ValidatePropertyInitValue<int, THandler>(
				editor,
				() => editor.CursorPosition,
				GetPlatformCursorPosition,
				0);
		}

		[Theory(DisplayName = "Unset SelectionLength is kept at zero at initialization with TextTransform")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task UnsetSelectionLengthKeepsToZeroOnInitializationWithTextTransform(string text)
		{
			var editor = new TView
			{
				Text = text,
				TextTransform = TextTransform.Uppercase
			};

			await ValidatePropertyInitValue<int, THandler>(
				editor,
				() => editor.SelectionLength,
				GetPlatformSelectionLength,
				0);
		}

		[Theory(DisplayName = "CursorPosition Initializes Correctly")]
		[InlineData(2)]
		public async Task CursorPositionInitializesCorrectly(int initialPosition)
		{
			var entry = new TView
			{
				Text = "This is TEXT!",
				CursorPosition = initialPosition
			};

			await ValidatePropertyInitValue<int, THandler>(
				entry,
				() => entry.CursorPosition,
				GetPlatformCursorPosition,
				initialPosition);
		}

		[Theory]
		[InlineData(2)]
		public async Task CursorPositionInitializesCorrectlyWithUpdateCursorPositionLast(int initialPosition)
		{
			var entry = new TView
			{
				Text = "This is TEXT!",
				CursorPosition = initialPosition
			};

			var value = await GetValueAsync<int, THandler>(entry, handler =>
			{
				handler.UpdateValue(nameof(ITextInput.CursorPosition));
				return GetPlatformCursorPosition(handler);
			});

			Assert.Equal(initialPosition, value);
		}

		[Theory]
		[InlineData(2)]
		public async Task CursorPositionInitializesCorrectlyWithUpdateTextLast(int initialPosition)
		{
			var entry = new TView
			{
				Text = "This is TEXT!",
				CursorPosition = initialPosition
			};

			var value = await GetValueAsync<int, THandler>(entry, handler =>
			{
				handler.UpdateValue(nameof(ITextInput.Text));
				return GetPlatformCursorPosition(handler);
			});

			Assert.Equal(initialPosition, value);
		}

		[Theory(DisplayName = "SelectionLength Initializes Correctly")]
		[InlineData(2)]
		public async Task SelectionLengthInitializesCorrectly(int initialLength)
		{
			var entry = new TView
			{
				Text = "This is TEXT!",
				SelectionLength = initialLength
			};

			await ValidatePropertyInitValue<int, THandler>(
				entry,
				() => entry.SelectionLength,
				GetPlatformSelectionLength,
				initialLength);
		}

		[Theory]
		[InlineData(2)]
		public async Task SelectionLengthInitializesCorrectlyWithUpdateCursorPositionLast(int initialLength)
		{
			var entry = new TView
			{
				Text = "This is TEXT!",
				SelectionLength = initialLength
			};

			var value = await GetValueAsync<int, THandler>(entry, handler =>
			{
				handler.UpdateValue(nameof(ITextInput.CursorPosition));
				return GetPlatformSelectionLength(handler);
			});

			Assert.Equal(initialLength, value);
		}

		[Theory]
		[InlineData(2)]
		public async Task SelectionLengthInitializesCorrectlyWithUpdateTextLast(int initialLength)
		{
			var entry = new TView
			{
				Text = "This is TEXT!",
				SelectionLength = initialLength
			};

			var value = await GetValueAsync<int, THandler>(entry, handler =>
			{
				handler.UpdateValue(nameof(ITextInput.Text));
				return GetPlatformSelectionLength(handler);
			});

			Assert.Equal(initialLength, value);
		}

		[Theory(DisplayName = "CursorPosition Updates Correctly")]
		[InlineData(2, 5)]
		public async Task CursorPositionUpdatesCorrectly(int setValue, int unsetValue)
		{
			string text = "This is TEXT!";

			var editor = new TView
			{
				Text = text
			};

			await ValidatePropertyUpdatesValue<int, THandler>(
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

			var editor = new TView
			{
				Text = text,
				CursorPosition = initialPosition
			};

			await ValidatePropertyInitValue<int, THandler>(
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
			var editor = new TView
			{
				Text = text
			};

			await ValidatePropertyInitValue<int, THandler>(
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
			var editor = new TView
			{
				Text = "Test"
			};

			await AttachAndRunFocusAffectedControl<TView, THandler>(editor, handler => editor.Text = text);

			Assert.Equal(text.Length, editor.CursorPosition);
		}

		[Theory(DisplayName = "SelectionLength Updates Correctly")]
		[InlineData(2, 5)]
		public async Task SelectionLengthUpdatesCorrectly(int setValue, int unsetValue)
		{
			string text = "This is TEXT!";

			var editor = new TView
			{
				Text = text,
			};

			await ValidatePropertyUpdatesValue<int, THandler>(
				editor,
				nameof(ITextInput.SelectionLength),
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

			var editor = new TView
			{
				Text = text,
				SelectionLength = selectionLength
			};

			await ValidatePropertyInitValue<int, THandler>(
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
			var editor = new TView
			{
				Text = text
			};

			await ValidatePropertyInitValue<int, THandler>(
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
			var editor = new TView
			{
				Text = "Test"
			};

			await AttachAndRunFocusAffectedControl<TView, THandler>(editor, handler => editor.Text = text);

			Assert.Equal(0, editor.SelectionLength);
		}
	}
}
