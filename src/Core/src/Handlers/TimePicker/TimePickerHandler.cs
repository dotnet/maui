namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler
	{
		public static IPropertyMapper<ITimePicker, TimePickerHandler> TimePickerMapper = new PropertyMapper<ITimePicker, TimePickerHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__
			[nameof(ITimePicker.Background)] = MapBackground,
#endif
			[nameof(ITimePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITimePicker.Font)] = MapFont,
			[nameof(ITimePicker.Format)] = MapFormat,
			[nameof(ITimePicker.TextColor)] = MapTextColor,
			[nameof(ITimePicker.Time)] = MapTime,
		};

		public TimePickerHandler() : base(TimePickerMapper)
		{

		}

		public TimePickerHandler(IPropertyMapper mapper) : base(mapper ?? TimePickerMapper)
		{

		}
	}
}