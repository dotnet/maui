using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		AppCompatEditText GetPlatformControl(EntryHandler handler) =>
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
			control.SetSelection(position);
		}
	}
}
