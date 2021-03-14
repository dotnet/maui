namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler
	{
		public static PropertyMapper<IEntry, EntryHandler> EntryMapper = new PropertyMapper<IEntry, EntryHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEntry.Keyboard)] = MapKeyboard,
			[nameof(IEntry.Text)] = MapText,
			[nameof(IEntry.TextColor)] = MapTextColor,
			[nameof(IEntry.IsPassword)] = MapIsPassword,
			[nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEntry.Placeholder)] = MapPlaceholder,
			[nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEntry.Font)] = MapFont
		};

		public EntryHandler() : base(EntryMapper)
		{

		}

		public EntryHandler(PropertyMapper mapper) : base(mapper ?? EntryMapper)
		{

		}
	}
}