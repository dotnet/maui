using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Editor)]
	public partial class EditorHandlerTests : CoreHandlerTestBase<EditorHandler, EditorStub>
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

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var editor = new EditorStub()
			{
				Text = "Test",
				TextColor = null
			};

			await CreateHandlerAsync(editor);
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

		[Fact(DisplayName = "Null Placeholder Color Doesn't Crash")]
		public async Task NullPlaceholderColorDoesntCrash()
		{
			var editor = new EditorStub()
			{
				Placeholder = "Test",
				PlaceholderColor = null
			};

			await CreateHandlerAsync(editor);
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

#if WINDOWS
			// On Windows, the default value for the Placeholder text is an empty string ("")
			await ValidatePropertyInitValue(editor, () => editor.Placeholder ?? string.Empty, GetNativePlaceholderText, editor.Placeholder ?? string.Empty);
#else
			await ValidatePropertyInitValue(editor, () => editor.Placeholder, GetNativePlaceholderText, editor.Placeholder);
#endif
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

			var platformText = await GetValueAsync(editor, GetNativeText);

			Assert.Equal(expectedText, platformText);
			//TODO: Until Editor gets text update events
			//Assert.Equal(expectedText, editor.Text);
		}

		[Theory(DisplayName = "MaxLength Clips Native Text Correctly"
#if WINDOWS
			, Skip = "https://github.com/dotnet/maui/issues/7939"
#endif
		)]
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

			var platformText = await GetValueAsync(editor, handler =>
			{
				editor.Text = text;

				return GetNativeText(handler);
			});

			Assert.Equal(expectedText, platformText);
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

			var nativeIsTextPredictionEnabled = await GetValueAsync(editor, handler =>
			{
				return GetNativeIsTextPredictionEnabled(handler);
			});

			Assert.Equal(isEnabled, nativeIsTextPredictionEnabled);
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

		[Theory(DisplayName = "Validates Numeric Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), true)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateNumericKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var editor = new EditorStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(editor, () => expected, GetNativeIsNumericKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Email Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), true)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateEmailKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var editor = new EditorStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(editor, () => expected, GetNativeIsEmailKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Telephone Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), true)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateTelephoneKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var editor = new EditorStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(editor, () => expected, GetNativeIsTelephoneKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Url Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), true)]
		public async Task ValidateUrlKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var editor = new EditorStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(editor, () => expected, GetNativeIsUrlKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Text Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), true)]
		[InlineData(nameof(Keyboard.Url), false)]
#if WINDOWS
		// The Text keyboard is the default one on Windows
		[InlineData(nameof(Keyboard.Default), true)]
		// Plain is the same as the Default keyboard on Windows
		[InlineData(nameof(Keyboard.Plain), true)]
#else
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
#endif
		public async Task ValidateTextKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var editor = new EditorStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(editor, () => expected, GetNativeIsTextKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Chat Keyboard")]
		[InlineData(nameof(Keyboard.Chat), true)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateChatKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var editor = new EditorStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(editor, () => expected, GetNativeIsChatKeyboard, expected);
		}

		[Category(TestCategory.Editor)]
		public class EditorTextStyleTests : TextStyleHandlerTests<EditorHandler, EditorStub>
		{
		}
	}
}
