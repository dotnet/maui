using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
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
			var control = new TView
			{
				Text = text,
				TextTransform = TextTransform.Uppercase
			};

			await ValidatePropertyInitValue<int, THandler>(
				control,
				() => control.CursorPosition,
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
			var control = new TView
			{
				Text = text,
				TextTransform = TextTransform.Uppercase
			};

			await ValidatePropertyInitValue<int, THandler>(
				control,
				() => control.SelectionLength,
				GetPlatformSelectionLength,
				0);
		}

		[Theory(DisplayName = "CursorPosition Initializes Correctly")]
		[InlineData(2)]
		public async Task CursorPositionInitializesCorrectly(int initialPosition)
		{
			var control = new TView
			{
				Text = "This is TEXT!",
				CursorPosition = initialPosition
			};

			await ValidatePropertyInitValue<int, THandler>(
				control,
				() => control.CursorPosition,
				GetPlatformCursorPosition,
				initialPosition);
		}

		[Theory]
		[InlineData(2)]
		public async Task CursorPositionInitializesCorrectlyWithUpdateCursorPositionLast(int initialPosition)
		{
			var control = new TView
			{
				Text = "This is TEXT!",
				CursorPosition = initialPosition
			};

			var value = await GetValueAsync<int, THandler>(control, handler =>
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
			var control = new TView
			{
				Text = "This is TEXT!",
				CursorPosition = initialPosition
			};

			var value = await GetValueAsync<int, THandler>(control, handler =>
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
			var control = new TView
			{
				Text = "This is TEXT!",
				SelectionLength = initialLength
			};

			await ValidatePropertyInitValue<int, THandler>(
				control,
				() => control.SelectionLength,
				GetPlatformSelectionLength,
				initialLength);
		}

		[Theory]
		[InlineData(2)]
		public async Task SelectionLengthInitializesCorrectlyWithUpdateCursorPositionLast(int initialLength)
		{
			var control = new TView
			{
				Text = "This is TEXT!",
				SelectionLength = initialLength
			};

			var value = await GetValueAsync<int, THandler>(control, handler =>
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
			var control = new TView
			{
				Text = "This is TEXT!",
				SelectionLength = initialLength
			};

			var value = await GetValueAsync<int, THandler>(control, handler =>
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

			var control = new TView
			{
				Text = text
			};

			await ValidatePropertyUpdatesValue<int, THandler>(
				control,
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

			var control = new TView
			{
				Text = text,
				CursorPosition = initialPosition
			};

			await ValidatePropertyInitValue<int, THandler>(
				control,
				() => control.CursorPosition,
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
			var control = new TView
			{
				Text = text
			};

			await ValidatePropertyInitValue<int, THandler>(
				control,
				() => control.CursorPosition,
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
			var control = new TView
			{
				Text = "Test"
			};

			await AttachAndRunFocusAffectedControl<TView, THandler>(control, handler => control.Text = text);

			Assert.Equal(text.Length, control.CursorPosition);
		}

		[Theory(DisplayName = "SelectionLength Updates Correctly")]
		[InlineData(2, 5)]
		public async Task SelectionLengthUpdatesCorrectly(int setValue, int unsetValue)
		{
			string text = "This is TEXT!";

			var control = new TView
			{
				Text = text,
			};

			await ValidatePropertyUpdatesValue<int, THandler>(
				control,
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

			var control = new TView
			{
				Text = text,
				SelectionLength = selectionLength
			};

			await ValidatePropertyInitValue<int, THandler>(
				control,
				() => control.SelectionLength,
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
			var control = new TView
			{
				Text = text
			};

			await ValidatePropertyInitValue<int, THandler>(
				control,
				() => control.SelectionLength,
				GetPlatformSelectionLength,
				0);
		}

		[Theory(DisplayName = "SelectionLength is kept at zero on text change after initialization")]
		[InlineData("This is a test!!!")]
		[InlineData("a")]
		[InlineData("")]
		[InlineData(" ")]
		public async Task SelectionLengthKeepsToZeroOnTextChangeAfterInitialization(string text)
		{
			var control = new TView
			{
				Text = "Test"
			};

			await AttachAndRunFocusAffectedControl<TView, THandler>(control, handler => control.Text = text);

			Assert.Equal(0, control.SelectionLength);
		}
	}
}
