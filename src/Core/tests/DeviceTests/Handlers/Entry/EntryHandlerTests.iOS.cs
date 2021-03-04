using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		UITextField GetNativeEntry(EntryHandler entryHandler) =>
			(UITextField)entryHandler.View;

		string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		Color GetNativeTextColor(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextColor.ToColor();

		bool GetNativeIsPassword(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).SecureTextEntry;

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Placeholder;
	}
}