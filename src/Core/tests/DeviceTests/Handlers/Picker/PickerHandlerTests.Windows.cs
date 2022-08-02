using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		ComboBox GetNativePicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView; 
		
		UI.Xaml.HorizontalAlignment GetNativeHorizontalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).HorizontalContentAlignment;

		UI.Xaml.VerticalAlignment GetNativeVerticalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).VerticalAlignment;
	}
}