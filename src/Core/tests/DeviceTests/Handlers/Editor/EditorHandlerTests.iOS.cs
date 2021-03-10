using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
		UITextField GetNativeEditor(EditorHandler editorHandler) =>
			(UITextField)editorHandler.View;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;
	}
}