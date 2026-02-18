using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		static TimePicker()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
RemappingHelper.EnsureBaseTypeRemapped(typeof(TimePicker), typeof(VisualElement));

			// Adjust the mappings to preserve Controls.TimePicker legacy behaviors
#if IOS
			TimePickerHandler.Mapper.ReplaceMapping<TimePicker, ITimePickerHandler>(PlatformConfiguration.iOSSpecific.TimePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}