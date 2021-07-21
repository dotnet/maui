namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler
	{
		public static PropertyMapper<IPicker, PickerHandler> PickerMapper = new(ViewMapper)
		{
#if __ANDROID__
			[nameof(IPicker.Background)] = MapBackground,
#endif
			[nameof(IPicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IPicker.Font)] = MapFont,
			[nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
			[nameof(IPicker.TextColor)] = MapTextColor,
			[nameof(IPicker.Title)] = MapTitle,
			[nameof(IPicker.TitleColor)] = MapTitleColor,
			[nameof(IPicker.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
		};

		public static CommandMapper<IPicker, PickerHandler> PickerCommandMapper = new(ViewCommandMapper)
		{
			["Reload"] = MapReload
		};

		public PickerHandler() : base(PickerMapper, PickerCommandMapper)
		{

		}

		public PickerHandler(PropertyMapper mapper) : base(mapper)
		{

		}
	}
}