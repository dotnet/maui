#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		[Obsolete("Use PickerHandler.Mapper instead.")]
		public static IPropertyMapper<IPicker, PickerHandler> ControlsPickerMapper = new PropertyMapper<Picker, PickerHandler>(PickerHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Picker legacy behaviors
#if IOS
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(PlatformConfiguration.iOSSpecific.Picker.UpdateModeProperty.PropertyName, MapUpdateMode);
#elif WINDOWS
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.HorizontalOptions), MapHorizontalOptions);
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.VerticalOptions), MapVerticalOptions);
#endif
			PickerHandler.Mapper.ReplaceMapping<Picker, IPickerHandler>(nameof(Picker.ItemsSource), MapItemsSource);
		}

		internal static void MapItemsSource(IPickerHandler handler, IPicker view)
		{
			handler.UpdateValue(nameof(IPicker.Items));
		}
	}
}