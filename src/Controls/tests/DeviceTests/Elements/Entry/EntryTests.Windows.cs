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

		int GetCursorStartPosition(EntryHandler entryHandler)
		{
			var control = GetPlatformControl(entryHandler);
			return control.SelectionStart;
		}

		void UpdateCursorStartPosition(EntryHandler entryHandler, int position)
		{
			var control = GetPlatformControl(entryHandler);
			control.SelectionStart = position;
		}
	}
}
