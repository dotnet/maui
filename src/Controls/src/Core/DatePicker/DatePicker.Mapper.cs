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
RemappingHelper.EnsureBaseTypeRemapped(typeof(DatePicker), typeof(VisualElement));

			// Adjust the mappings to preserve Controls.DatePicker legacy behaviors
#if IOS
			DatePickerHandler.Mapper.ReplaceMapping<DatePicker, IDatePickerHandler>(PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}