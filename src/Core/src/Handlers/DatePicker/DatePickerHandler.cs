namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler
	{
		public static PropertyMapper<IDatePicker, DatePickerHandler> DatePickerMapper = new PropertyMapper<IDatePicker, DatePickerHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IDatePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IDatePicker.Date)] = MapDate,
			[nameof(IDatePicker.Font)] = MapFont,
			[nameof(IDatePicker.Format)] = MapFormat,
			[nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
			[nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
			[nameof(IDatePicker.Foreground)] = MapForeground,
		};

		public DatePickerHandler() : base(DatePickerMapper)
		{

		}

		public DatePickerHandler(PropertyMapper mapper) : base(mapper)
		{

		}
	}
}