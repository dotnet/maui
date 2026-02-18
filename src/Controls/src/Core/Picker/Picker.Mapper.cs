#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		static Picker()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
RemappingHelper.EnsureBaseTypeRemapped(typeof(Picker), typeof(VisualElement));

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