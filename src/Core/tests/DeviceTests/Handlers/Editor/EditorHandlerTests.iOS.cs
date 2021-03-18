using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform.iOS;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
		MauiTextView GetNativeEditor(EditorHandler editorHandler) =>
			(MauiTextView)editorHandler.View;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;

		string GetNativePlaceholderText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).PlaceholderText;

		Color GetNativePlaceholderColor(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).PlaceholderTextColor.ToColor();
	}
}