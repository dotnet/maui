using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		NativePicker GetNativePicker(PickerHandler pickerHandler) =>
			(NativePicker)pickerHandler.View;

		string GetNativeTitle(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Hint;
	}
}