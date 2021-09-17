using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform.iOS;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			string originalText = "Test";
			var xplatCharacterSpacing = 4;

			var editor = new EditorStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = originalText
			};

			var values = await GetValueAsync(editor, (handler) =>
			{
				return new
				{
					ViewValue = editor.CharacterSpacing,
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.NativeViewValue);
		}

		static MauiTextView GetNativeEditor(EditorHandler editorHandler) =>
			editorHandler.NativeView;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;

		static void SetNativeText(EditorHandler editorHandler, string text) =>
			GetNativeEditor(editorHandler).Text = text;

		string GetNativePlaceholderText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).PlaceholderText;

		Color GetNativePlaceholderColor(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).PlaceholderTextColor.ToColor();

		double GetNativeCharacterSpacing(EditorHandler editorHandler)
		{
			var editor = GetNativeEditor(editorHandler);
			return editor.AttributedText.GetCharacterSpacing();
		}

		double GetNativeUnscaledFontSize(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Font.PointSize;

		bool GetNativeIsReadOnly(EditorHandler editorHandler) =>
			!GetNativeEditor(editorHandler).UserInteractionEnabled;

		bool GetNativeIsTextPredictionEnabled(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).AutocorrectionType == UITextAutocorrectionType.Yes;

		Color GetNativeTextColor(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).TextColor.ToColor();

		bool GetNativeIsNumericKeyboard(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).KeyboardType == UIKeyboardType.DecimalPad;

		bool GetNativeIsEmailKeyboard(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).KeyboardType == UIKeyboardType.EmailAddress;

		bool GetNativeIsTelephoneKeyboard(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).KeyboardType == UIKeyboardType.PhonePad;

		bool GetNativeIsUrlKeyboard(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).KeyboardType == UIKeyboardType.Url;

		bool GetNativeIsTextKeyboard(EditorHandler editorHandler)
		{
			var nativeEditor = GetNativeEditor(editorHandler);

			return nativeEditor.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				nativeEditor.AutocorrectionType == UITextAutocorrectionType.Yes &&
				nativeEditor.SpellCheckingType == UITextSpellCheckingType.Yes;
		}

		bool GetNativeIsChatKeyboard(EditorHandler editorHandler)
		{
			var nativeEditor = GetNativeEditor(editorHandler);

			return nativeEditor.AutocapitalizationType == UITextAutocapitalizationType.Sentences &&
				nativeEditor.AutocorrectionType == UITextAutocorrectionType.Yes &&
				nativeEditor.SpellCheckingType == UITextSpellCheckingType.No;
		}
	}
}