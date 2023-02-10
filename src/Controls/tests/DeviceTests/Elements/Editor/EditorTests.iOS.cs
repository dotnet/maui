using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class EditorTests
	{
		MauiTextView GetPlatformControl(EditorHandler handler) =>
			handler.PlatformView;

		Task<string> GetPlatformText(EditorHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		void SetPlatformText(EditorHandler editorHandler, string text) =>
			GetPlatformControl(editorHandler).Text = text;

		int GetPlatformCursorPosition(EditorHandler editorHandler)
		{
			var nativeEditor = GetPlatformControl(editorHandler);

			if (nativeEditor != null && nativeEditor.SelectedTextRange != null)
				return (int)nativeEditor.GetOffsetFromPosition(nativeEditor.BeginningOfDocument, nativeEditor.SelectedTextRange.Start);

			return -1;
		}

		int GetPlatformSelectionLength(EditorHandler editorHandler)
		{
			var nativeEditor = GetPlatformControl(editorHandler);

			if (nativeEditor != null && nativeEditor.SelectedTextRange != null)
				return (int)nativeEditor.GetOffsetFromPosition(nativeEditor.SelectedTextRange.Start, nativeEditor.SelectedTextRange.End);

			return -1;
		}
	}
}
