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

		void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		int GetPlatformCursorPosition(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, textField.SelectedTextRange.Start);

			return -1;
		}

		int GetPlatformSelectionLength(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.SelectedTextRange.Start, textField.SelectedTextRange.End);

			return -1;
		}
	}
}
