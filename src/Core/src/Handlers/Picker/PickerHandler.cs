namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler
	{
		public static IPropertyMapper<IPicker, PickerHandler> PickerMapper = new PropertyMapper<IPicker, PickerHandler>(ViewMapper)
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
			[nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
		};

		public static CommandMapper<IPicker, PickerHandler> PickerCommandMapper = new(ViewCommandMapper)
		{
			["Reload"] = MapReload
		};

		static PickerHandler()
		{
#if __IOS__
			PickerMapper.PrependToMapping(nameof(IView.FlowDirection), (h, __) => h.UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment)));
#endif
		}

		public PickerHandler() : base(PickerMapper, PickerCommandMapper)
		{

		}

		public PickerHandler(IPropertyMapper mapper) : base(mapper)
		{

		}
	}
}