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
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(TimePicker), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;

			// Adjust the mappings to preserve Controls.TimePicker legacy behaviors
#if IOS
			TimePickerHandler.Mapper.ReplaceMapping<TimePicker, ITimePickerHandler>(PlatformConfiguration.iOSSpecific.TimePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}