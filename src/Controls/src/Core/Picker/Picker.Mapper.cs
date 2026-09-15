#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			// Adjust the mappings to preserve Controls.Picker legacy behaviors
#if IOS
			PickerHandler.Mapper.ReplaceMappingForControls<Picker, IPickerHandler>(PlatformConfiguration.iOSSpecific.Picker.UpdateModeProperty.PropertyName, MapUpdateMode);
#elif WINDOWS
			PickerHandler.Mapper.ReplaceMappingForControls<Picker, IPickerHandler>(nameof(Picker.HorizontalOptions), MapHorizontalOptions);
			PickerHandler.Mapper.ReplaceMappingForControls<Picker, IPickerHandler>(nameof(Picker.VerticalOptions), MapVerticalOptions);
#endif
			PickerHandler.Mapper.ReplaceMappingForControls<Picker, IPickerHandler>(nameof(Picker.ItemsSource), MapItemsSource);
		}

		internal static void MapItemsSource(IPickerHandler handler, IPicker view)
		{
			handler.UpdateValue(nameof(IPicker.Items));
		}
	}
}