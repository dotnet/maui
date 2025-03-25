#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class DatePicker
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.DatePicker legacy behaviors
#if IOS
			DatePickerHandler.Mapper.ReplaceMapping<DatePicker, IDatePickerHandler>(PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}