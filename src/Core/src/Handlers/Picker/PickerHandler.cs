namespace Microsoft.Maui.Handlers
{
	public partial class PickerHandler
	{
		public static PropertyMapper<IPicker, PickerHandler> PickerMapper = new PropertyMapper<IPicker, PickerHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IPicker.Title)] = MapTitle,
			[nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
			[nameof(IPicker.CharacterSpacing)] = MapCharacterSpacing
		};

		public PickerHandler() : base(PickerMapper)
		{

		}

		public PickerHandler(PropertyMapper mapper) : base(mapper)
		{

		}
	}
}