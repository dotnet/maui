using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
		AppCompatEditText GetNativeEditor(EditorHandler editorHandler) =>
			(AppCompatEditText)editorHandler.View;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;

		string GetNativePlaceholderText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Hint;

		Color GetNativePlaceholderColor(EditorHandler editorHandler) =>
			((uint)GetNativeEditor(editorHandler).CurrentHintTextColor).ToColor();
	}
}