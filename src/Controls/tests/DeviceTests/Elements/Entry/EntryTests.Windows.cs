#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		static TextBox GetPlatformControl(EntryHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		Task<float> GetPlatformOpacity(EntryHandler entryHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(entryHandler);
				return (float)nativeView.Opacity;
			});
		}

		static void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		static int GetPlatformCursorPosition(EntryHandler entryHandler) =>
			GetPlatformControl(entryHandler).SelectionStart;

		static int GetPlatformSelectionLength(EntryHandler entryHandler) =>
			GetPlatformControl(entryHandler).SelectionLength;
	}
}
