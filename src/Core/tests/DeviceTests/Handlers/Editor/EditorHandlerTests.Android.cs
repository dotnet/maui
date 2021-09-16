using System.Threading.Tasks;
using Android.Text;
using Android.Text.Method;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
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
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue, EmCoefficientPrecision);
		}

		static AppCompatEditText GetNativeEditor(EditorHandler editorHandler) =>
			(AppCompatEditText)editorHandler.NativeView;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;

		static void SetNativeText(EditorHandler editorHandler, string text) =>
			GetNativeEditor(editorHandler).Text = text;

		string GetNativePlaceholderText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Hint;

		Color GetNativePlaceholderColor(EditorHandler editorHandler) =>
			((uint)GetNativeEditor(editorHandler).CurrentHintTextColor).ToColor();


		bool GetNativeIsReadOnly(EditorHandler editorHandler) =>
			!GetNativeEditor(editorHandler).Focusable;

		bool GetNativeIsTextPredictionEnabled(EditorHandler editorHandler) =>
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

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagNoSuggestions);
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

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && !inputTypes.HasFlag(InputTypes.TextFlagNoSuggestions);
		}
	}
}
