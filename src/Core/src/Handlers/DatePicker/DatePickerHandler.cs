namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler
	{
		public static PropertyMapper<IDatePicker, DatePickerHandler> DatePickerMapper = new PropertyMapper<IDatePicker, DatePickerHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__
			[nameof(IDatePicker.Background)] = MapBackground,
#endif
			[nameof(IDatePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IDatePicker.Date)] = MapDate,
			[nameof(IDatePicker.Font)] = MapFont,
			[nameof(IDatePicker.Format)] = MapFormat,
			[nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
			[nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
			[nameof(IDatePicker.TextColor)] = MapTextColor,
		};

		public DatePickerHandler() : base(DatePickerMapper)
		{

		}

		public DatePickerHandler(PropertyMapper mapper) : base(mapper)
		{

		}
	}
}