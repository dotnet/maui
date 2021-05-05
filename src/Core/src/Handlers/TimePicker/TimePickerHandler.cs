namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler
	{
		public static PropertyMapper<ITimePicker, TimePickerHandler> TimePickerMapper = new PropertyMapper<ITimePicker, TimePickerHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ITimePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITimePicker.Font)] = MapFont,
			[nameof(ITimePicker.Format)] = MapFormat,
			[nameof(ITimePicker.TextColor)] = MapTextColor,
			[nameof(ITimePicker.Time)] = MapTime,
		};

		public TimePickerHandler() : base(TimePickerMapper)
		{

		}

		public TimePickerHandler(PropertyMapper mapper) : base(mapper ?? TimePickerMapper)
		{

		}
	}
}