#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		[Obsolete("Use TimePickerHandler.Mapper instead.")]
		public static IPropertyMapper<ITimePicker, TimePickerHandler> ControlsTimePickerMapper = new PropertyMapper<TimePicker, TimePickerHandler>(TimePickerHandler.Mapper);

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.TimePicker legacy behaviors
#if IOS
			TimePickerHandler.Mapper.ReplaceMapping<TimePicker, ITimePickerHandler>(PlatformConfiguration.iOSSpecific.TimePicker.UpdateModeProperty.PropertyName, MapUpdateMode);
#endif
		}
	}
}