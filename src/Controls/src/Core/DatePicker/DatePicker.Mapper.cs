#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class DatePicker
	{
		static DatePicker()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(DatePicker), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;

			// Adjust the mappings to preserve Controls.DatePicker legacy behaviors
#if IOS
			DatePickerHandler.Mapper.ReplaceMapping<DatePicker, IDatePickerHandler>(PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}