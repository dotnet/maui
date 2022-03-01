namespace Microsoft.Maui.Controls
{
	public partial class TimePicker
	{
		public static IPropertyMapper<ITimePicker, TimePickerHandler> ControlsTimePickerMapper =
			new PropertyMapper<TimePicker, TimePickerHandler>(TimePickerHandler.TimePickerMapper)
			{
				[nameof(TextTransform)] = MapText,
			};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.TimePicker legacy behaviors
			TimePickerHandler.TimePickerMapper = ControlsTimePickerMapper;
		}
	}
}