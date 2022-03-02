using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		UITextField GetPlatformControl(EntryHandler handler) =>
			(UITextField)handler.PlatformView;

		Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		void UpdateCursorStartPosition(EntryHandler entryHandler, int position)
		{
			var control = GetPlatformControl(entryHandler);
			var endPosition = control.GetPosition(control.BeginningOfDocument, position);
			control.SelectedTextRange = control.GetTextRange(endPosition, endPosition);
		}

		int GetCursorStartPosition(EntryHandler entryHandler)
		{
			var control = GetPlatformControl(entryHandler);
			return (int)control.GetOffsetFromPosition(control.BeginningOfDocument, control.SelectedTextRange.Start);
		}
	}
}
