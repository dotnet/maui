using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
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

        UITextView GetNativeEditor(EditorHandler editorHandler) =>
			(UITextView)editorHandler.View;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;

        double GetNativeCharacterSpacing(EditorHandler editorHandler)
        {
            var editor = GetNativeEditor(editorHandler);
            return editor.AttributedText.GetCharacterSpacing();
        }
		
		bool GetNativeIsTextPredictionEnabled(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).AutocorrectionType == UITextAutocorrectionType.Yes;
	}
}