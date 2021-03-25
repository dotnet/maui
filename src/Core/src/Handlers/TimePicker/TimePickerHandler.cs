namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler
	{
		public static PropertyMapper<ITimePicker, TimePickerHandler> TimePickerMapper = new PropertyMapper<ITimePicker, TimePickerHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ITimePicker.Format)] = MapFormat,
			[nameof(ITimePicker.Time)] = MapTime,
			[nameof(ITimePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITimePicker.Font)] = MapFont
		};

		public TimePickerHandler() : base(TimePickerMapper)
		{

		}

		public TimePickerHandler(PropertyMapper mapper) : base(mapper ?? TimePickerMapper)
		{

		}
	}
}