#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		TextBox GetPlatformControl(EntryHandler handler) =>
			handler.PlatformView;

		Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		int GetPlatformCursorPosition(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).GetCursorPosition();

		int GetPlatformSelectionLength(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).SelectionLength;
	}
}
