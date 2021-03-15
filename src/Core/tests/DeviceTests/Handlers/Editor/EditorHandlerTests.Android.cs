using Android.Text;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
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

        AppCompatEditText GetNativeEditor(EditorHandler editorHandler) =>
			(AppCompatEditText)editorHandler.View;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;
		
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
    }
}