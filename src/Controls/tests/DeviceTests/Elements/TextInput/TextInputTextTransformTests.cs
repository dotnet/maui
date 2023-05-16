using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TextInput)]
	public abstract partial class TextInputTextTransformTests<THandler, TView> : ControlsHandlerTestBase
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
	}
}
