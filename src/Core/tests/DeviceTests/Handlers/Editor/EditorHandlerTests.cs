using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Editor)]
	public partial class EditorHandlerTests : HandlerTestBase<EditorHandler, EditorStub>
	{
		[Fact(DisplayName = "Text Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var editor = new EditorStub()
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(editor, () => editor.Text, GetNativeText, editor.Text);
		}

		[Theory(DisplayName = "Text Updates Correctly")]
		[InlineData(null, null)]
		[InlineData(null, "Hello")]
		[InlineData("Hello", null)]
		[InlineData("Hello", "Goodbye")]
		public async Task TextUpdatesCorrectly(string setValue, string unsetValue)
		{
			var editor = new EditorStub();

			await ValidatePropertyUpdatesValue(
				editor,
				nameof(IEditor.Text),
				h =>
				{
					var n = GetNativeText(h);
					if (string.IsNullOrEmpty(n))
						n = null; // native platforms may not upport null text
					return n;
				},
				setValue,
				unsetValue);
		}

		[Fact(DisplayName = "TextColor Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var editor = new EditorStub()
			{
				Text = "Test",
				TextColor = Colors.Yellow
			};

			await ValidatePropertyInitValue(editor, () => editor.TextColor, GetNativeTextColor, editor.TextColor);
		}

		[Fact(DisplayName = "PlaceholderColor Initializes Correctly")]
		public async Task PlaceholderColorInitializesCorrectly()
		{
			var editor = new EditorStub()
			{
				Placeholder = "Test",
				PlaceholderColor = Colors.Yellow
			};

			await ValidatePropertyInitValue(editor, () => editor.PlaceholderColor, GetNativePlaceholderColor, editor.PlaceholderColor);
		}

		[Theory(DisplayName = "PlaceholderColor Updates Correctly")]
		[InlineData(0xFF0000, 0x0000FF)]
		[InlineData(0x0000FF, 0xFF0000)]
		public async Task PlaceholderColorUpdatesCorrectly(uint setValue, uint unsetValue)
		{
			var editor = new EditorStub
			{
				Placeholder = "Placeholder"
			};

			var setColor = Color.FromUint(setValue);
			var unsetColor = Color.FromUint(unsetValue);

			await ValidatePropertyUpdatesValue(
				editor,
				nameof(IEditor.PlaceholderColor),
				GetNativePlaceholderColor,
				setColor,
				unsetColor);
		}

		[Fact(DisplayName = "PlaceholderText Initializes Correctly")]
		public async Task PlaceholderTextInitializesCorrectly()
		{
			var editor = new EditorStub()
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(editor, () => editor.Placeholder, GetNativePlaceholderText, editor.Placeholder);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsReadOnlyInitializesCorrectly(bool isReadOnly)
		{
			var editor = new EditorStub()
			{
				IsReadOnly = isReadOnly,
				Text = "Test"
			};

			await ValidatePropertyInitValue(editor, () => editor.IsReadOnly, GetNativeIsReadOnly, editor.IsReadOnly);
		}

		[Theory(DisplayName = "PlaceholderText Updates Correctly")]
		[InlineData(null, null)]
		[InlineData(null, "Hello")]
		[InlineData("Hello", null)]
		[InlineData("Hello", "Goodbye")]
		public async Task PlaceholderTextUpdatesCorrectly(string setValue, string unsetValue)
		{
			var editor = new EditorStub();

			await ValidatePropertyUpdatesValue(
				editor,
				nameof(IEditor.Placeholder),
				h =>
				{
					var n = GetNativePlaceholderText(h);
					if (string.IsNullOrEmpty(n))
						n = null; // native platforms may not support null text
					return n;
				},
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "MaxLength Initializes Correctly")]
		[InlineData(2)]
		[InlineData(5)]
		[InlineData(8)]
		[InlineData(10)]
		public async Task MaxLengthInitializesCorrectly(int maxLength)
		{
			const string text = "Lorem ipsum dolor sit amet";
			var expectedText = text.Substring(0, maxLength);

			var editor = new EditorStub()
			{
				MaxLength = maxLength,
				Text = text
			};

			var nativeText = await GetValueAsync(editor, GetNativeText);

			Assert.Equal(expectedText, nativeText);
			//TODO: Until Editor gets text update events
			//Assert.Equal(expectedText, editor.Text);
		}

		[Theory(DisplayName = "MaxLength Clips Native Text Correctly")]
		[InlineData(2)]
		[InlineData(5)]
		[InlineData(8)]
		[InlineData(10)]
		public async Task MaxLengthClipsNativeTextCorrectly(int maxLength)
		{
			const string text = "Lorem ipsum dolor sit amet";
			var expectedText = text.Substring(0, maxLength);

			var editor = new EditorStub()
			{
				MaxLength = maxLength,
			};

			var nativeText = await GetValueAsync(editor, handler =>
			{
				editor.Text = text;

				return GetNativeText(handler);
			});

			Assert.Equal(expectedText, nativeText);
			//TODO: Until Editor gets text update events
			//Assert.Equal(expectedText, editor.Text);
		}

		[Theory(DisplayName = "Is Text Prediction Enabled")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsTextPredictionEnabledCorrectly(bool isEnabled)
		{
			var editor = new EditorStub()
			{
				IsTextPredictionEnabled = isEnabled
			};

			await ValidatePropertyInitValue(editor, () => editor.IsTextPredictionEnabled, GetNativeIsTextPredictionEnabled, isEnabled);
		}

		[Theory(DisplayName = "IsTextPredictionEnabled Updates Correctly")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsTextPredictionEnabledUpdatesCorrectly(bool setValue, bool unsetValue)
		{
			var editor = new EditorStub();

			await ValidatePropertyUpdatesValue(
				editor,
				nameof(IEditor.IsTextPredictionEnabled),
				GetNativeIsTextPredictionEnabled,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var editor = new EditorStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", fontSize)
			};

			await ValidatePropertyInitValue(editor, () => editor.Font.FontSize, GetNativeUnscaledFontSize, editor.Font.FontSize);
		}
	}
}