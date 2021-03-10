using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
		UITextView GetNativeEditor(EditorHandler editorHandler) =>
			(UITextView)editorHandler.View;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;
	}
}