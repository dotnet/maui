#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class DatePicker
	{
		internal override void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(DatePicker)))
			{
				base.RemapForControls(remapped);

				// Adjust the mappings to preserve Controls.DatePicker legacy behaviors
#if IOS
				DatePickerHandler.Mapper.ReplaceMapping<DatePicker, IDatePickerHandler>(PlatformConfiguration.iOSSpecific.DatePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
			}
		}
	}
}