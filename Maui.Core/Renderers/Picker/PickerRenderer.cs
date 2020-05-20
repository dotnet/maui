namespace System.Maui.Platform
{
	public partial class PickerRenderer
	{
		public static PropertyMapper<IPicker> PickerMapper = new PropertyMapper<IPicker>(ViewRenderer.ViewMapper)
		{
			[nameof(IPicker.Title)] = MapPropertyTitle,
			[nameof(IPicker.TitleColor)] = MapPropertyTitleColor,
			[nameof(IPicker.TextColor)] = MapPropertyTextColor,
			[nameof(IPicker.SelectedIndex)] = MapPropertySelectedIndex
		};

		public PickerRenderer() : base(PickerMapper)
		{

		}

		public PickerRenderer(PropertyMapper mapper) : base(mapper)
		{

		}
	}
}