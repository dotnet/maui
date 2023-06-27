#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		public static IPropertyMapper<ITimePicker, TimePickerHandler> ControlsTimePickerMapper = new PropertyMapper<TimePicker, TimePickerHandler>(TimePickerHandler.Mapper)
		{
#if IOS
			[PlatformConfiguration.iOSSpecific.TimePicker.UpdateModeProperty.PropertyName] = MapUpdateMode,
#endif
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.TimePicker legacy behaviors
			TimePickerHandler.Mapper = ControlsTimePickerMapper;
		}
	}
}