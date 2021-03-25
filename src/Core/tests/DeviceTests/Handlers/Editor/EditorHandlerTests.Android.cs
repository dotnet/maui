using System.Threading.Tasks;
using Android.Text;
using AndroidX.AppCompat.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

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

		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("monospace")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var editor = new EditorStub()
			{
				Text = "Test",
				Font = Font.OfSize(family, 10)
			};

			var handler = await CreateHandlerAsync(editor);
			var nativeEditor = GetNativeEditor(handler);

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			var nativeFont = fontManager.GetTypeface(Font.OfSize(family, 0.0));

			Assert.Equal(nativeFont, nativeEditor.Typeface);

			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultTypeface, nativeEditor.Typeface);
			else
				Assert.NotEqual(fontManager.DefaultTypeface, nativeEditor.Typeface);
		}

		AppCompatEditText GetNativeEditor(EditorHandler editorHandler) =>
			(AppCompatEditText)editorHandler.View;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;

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

		double GetNativeUnscaledFontSize(EditorHandler editorHandler)
		{
			var textView = GetNativeEditor(editorHandler);
			return textView.TextSize / textView.Resources.DisplayMetrics.Density;
		}
	}
}
