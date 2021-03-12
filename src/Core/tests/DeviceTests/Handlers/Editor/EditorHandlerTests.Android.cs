using Android.Text;
using Android.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorHandlerTests
	{
		EditText GetNativeEditor(EditorHandler editorHandler) =>
			(EditText)editorHandler.View;

		string GetNativeText(EditorHandler editorHandler) =>
			GetNativeEditor(editorHandler).Text;
	}
}