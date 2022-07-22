#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EditorTests
	{
		TextBox GetPlatformControl(EditorHandler handler) =>
			handler.PlatformView;

		Task<string> GetPlatformText(EditorHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		void SetPlatformText(EditorHandler editorHandler, string text) =>
			GetPlatformControl(editorHandler).Text = text;

		int GetPlatformCursorPosition(EditorHandler editorHandler) =>
			GetPlatformControl(editorHandler).SelectionStart;

		int GetPlatformSelectionLength(EditorHandler editorHandler) =>
			GetPlatformControl(editorHandler).SelectionLength;
	}
}
