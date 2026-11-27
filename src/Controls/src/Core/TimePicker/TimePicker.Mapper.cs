using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			// Adjust the mappings to preserve Controls.TimePicker legacy behaviors
#if IOS
			TimePickerHandler.Mapper.ReplaceMappingForControls<TimePicker, ITimePickerHandler>(PlatformConfiguration.iOSSpecific.TimePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}