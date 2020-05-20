namespace System.Maui.Platform
{
	public partial class TimePickerRenderer
	{
		public static PropertyMapper<ITimePicker> TimePickerMapper = new PropertyMapper<ITimePicker>(LabelRenderer.ITextMapper)
		{
#if NETCOREAPP
			[nameof(ITimePicker.SelectedTime)] = MapPropertySelectedTime,
			[nameof(ITimePicker.Color)] = MapPropertyColor,
#elif __MACOS__
			[nameof(ITimePicker.SelectedTime)] = MapPropertySelectedTime,
#else
			[nameof(ITimePicker.SelectedTime)] = LabelRenderer.MapPropertyText,
#endif

		};

		public TimePickerRenderer() : base(TimePickerMapper)
		{

		}

		public TimePickerRenderer(PropertyMapper mapper) : base(mapper)
		{
		}

	}
}
