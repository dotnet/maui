namespace Microsoft.Maui.Controls
{
	public partial class DatePicker
	{
		public static IPropertyMapper<IDatePicker, DatePickerHandler> ControlsDatePickerMapper =
			new PropertyMapper<DatePicker, DatePickerHandler>(DatePickerHandler.Mapper)
			{
				[nameof(TextTransform)] = MapText,
			};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.DatePicker legacy behaviors
			DatePickerHandler.Mapper = ControlsDatePickerMapper;
		}
	}
}