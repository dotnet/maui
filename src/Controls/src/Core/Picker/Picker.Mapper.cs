#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Picker legacy behaviors
#if IOS
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(PlatformConfiguration.iOSSpecific.Picker.UpdateModeProperty.PropertyName, MapUpdateMode);
#elif WINDOWS
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.HorizontalOptions), MapHorizontalOptions);
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.VerticalOptions), MapVerticalOptions);
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.BorderColor), MapBorderColor);
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.BorderThickness), MapBorderThickness);
#elif ANDROID
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.BorderColor), MapBorderColor);
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.BorderThickness), MapBorderThickness);
#endif
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.ItemsSource), MapItemsSource);
		}

		internal static void MapItemsSource(IPickerHandler handler, IPicker view)
		{
			handler.UpdateValue(nameof(IPicker.Items));
		}
	}
}