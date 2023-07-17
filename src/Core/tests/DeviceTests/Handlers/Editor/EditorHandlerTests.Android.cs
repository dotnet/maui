using System.Threading.Tasks;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;
using ATextAlignemnt = Android.Views.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
		[Fact(DisplayName = "InputType Keeps MultiLine Flag")]
		public async Task InputTypeKeepsMultiLineFlag()
		{
			var editor = new EditorStub();
			var inputType = await GetValueAsync(editor, (handler) =>
			{
				return GetNativeEditor(handler).InputType;
			});

			Assert.True(inputType.HasFlag(InputTypes.TextFlagMultiLine));
		}

		[Fact(DisplayName = "ReadOnly Keeps MultiLine Flag")]
		public async Task InputTypeInitializesWithMultiLineFlag()
		{
			var editor = new EditorStub() { IsReadOnly = true };
			var inputType = await GetValueAsync(editor, (handler) =>
			{
				return GetNativeEditor(handler).InputType;
			});

			Assert.True(inputType.HasFlag(InputTypes.TextFlagMultiLine));
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var editor = new EditorStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = "Test"
			};

			float expectedValue = editor.CharacterSpacing.ToEm();

			var values = await GetValueAsync(editor, (handler) =>
			{
				return new
				{
					ViewValue = editor.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var editorStub = new EditorStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			var values = await GetValueAsync(editorStub, (handler) =>
			{
				return new
				{
					ViewValue = editorStub.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);

			(var gravity, var textAlignment) = values.PlatformViewValue;

			// Device Tests runner has RTL support enabled, so we expect TextAlignment values
			// (If it didn't, we'd have to fall back to gravity)
			var expectedValue = ATextAlignemnt.ViewEnd;

			Assert.Equal(expectedValue, textAlignment);
		}

		static AppCompatEditText GetNativeEditor(EditorHandler editorHandler) =>
			(AppCompatEditText)editorHandler.PlatformView;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;

		internal static void SetNativeText(EditorHandler editorHandler, string text) =>
			GetNativeEditor(editorHandler).Text = text;

		internal static int GetCursorStartPosition(EditorHandler editorHandler)
		{
			var control = GetNativeEditor(editorHandler);
			return control.SelectionStart;
		}

		internal static void UpdateCursorStartPosition(EditorHandler editorHandler, int position)
		{
			var control = GetNativeEditor(editorHandler);
			control.SetSelection(position);
		}

		string GetNativePlaceholderText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Hint;

		Color GetNativePlaceholderColor(EditorHandler editorHandler) =>
			((uint)GetNativeEditor(editorHandler).CurrentHintTextColor).ToColor();


		bool GetNativeIsReadOnly(EditorHandler editorHandler) =>
			!GetNativeEditor(editorHandler).Focusable;

		bool GetNativeIsTextPredictionEnabled(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).InputType.HasFlag(InputTypes.TextFlagAutoCorrect);

		bool GetNativeIsSpellCheckEnabled(EditorHandler editorHandler) =>
			!GetNativeEditor(editorHandler).InputType.HasFlag(InputTypes.TextFlagNoSuggestions);

		double GetNativeCharacterSpacing(EditorHandler editorHandler)
		{
			var editText = GetNativeEditor(editorHandler);

			if (editText != null)
			{
				return editText.LetterSpacing;
			}

			return -1;
		}

		Color GetNativeTextColor(EditorHandler editorHandler)
		{
			int currentTextColorInt = GetNativeEditor(editorHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		(GravityFlags gravity, ATextAlignemnt alignment) GetNativeHorizontalTextAlignment(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);
			return (textView.Gravity, textView.TextAlignment);
		}

		GravityFlags GetNativeVerticalTextAlignment(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Gravity & GravityFlags.VerticalGravityMask;

		GravityFlags GetNativeVerticalTextAlignment(TextAlignment textAlignment) =>
			textAlignment.ToVerticalGravityFlags();

		bool GetNativeIsNumericKeyboard(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);
			var inputTypes = textView.InputType;

			return textView.KeyListener is NumberKeyListener
				&& (inputTypes.HasFlag(InputTypes.NumberFlagDecimal) && inputTypes.HasFlag(InputTypes.ClassNumber) && inputTypes.HasFlag(InputTypes.NumberFlagSigned));
		}

		bool GetNativeIsChatKeyboard(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);
			var inputTypes = textView.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagAutoComplete);
		}

		bool GetNativeIsEmailKeyboard(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);
			var inputTypes = textView.InputType;

			return (inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationEmailAddress));
		}

		bool GetNativeIsTelephoneKeyboard(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);
			var inputTypes = textView.InputType;

			return inputTypes.HasFlag(InputTypes.ClassPhone);
		}

		bool GetNativeIsUrlKeyboard(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);
			var inputTypes = textView.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationUri);
		}

		bool GetNativeIsTextKeyboard(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);
			var inputTypes = textView.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagAutoComplete);
		}

		int GetNativeCursorPosition(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);

			if (textView != null)
				return textView.SelectionStart;

			return -1;
		}

		int GetNativeSelectionLength(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);

			if (textView != null)
				return textView.SelectionEnd - textView.SelectionStart;

			return -1;
		}
	}
}
