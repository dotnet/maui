using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		internal override void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(TimePicker)))
			{
				base.RemapForControls(remapped);

				// Adjust the mappings to preserve Controls.TimePicker legacy behaviors
#if IOS
				TimePickerHandler.Mapper.ReplaceMapping<TimePicker, ITimePickerHandler>(PlatformConfiguration.iOSSpecific.TimePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
			}
		}
	}
}