using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.TimePicker legacy behaviors
#if IOS
			TimePickerHandler.Mapper.ReplaceMapping<TimePicker, ITimePickerHandler>(PlatformConfiguration.iOSSpecific.TimePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}