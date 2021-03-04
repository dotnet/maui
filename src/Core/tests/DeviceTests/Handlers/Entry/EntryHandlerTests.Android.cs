using Android.Text;
using Android.Widget;
using Microsoft.Maui.Handlers;
using AColor = global::Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		EditText GetNativeEntry(EntryHandler entryHandler) =>
			(EditText)entryHandler.View;

		string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		Color GetNativeTextColor(EntryHandler entryHandler)
		{
			int currentTextColorInt = GetNativeEntry(entryHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		bool GetNativeIsPassword(EntryHandler entryHandler)
		{
			var inputType = GetNativeEntry(entryHandler).InputType;
			return inputType.HasFlag(InputTypes.TextVariationPassword) || inputType.HasFlag(InputTypes.NumberVariationPassword);
		}

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Hint;
	}
}