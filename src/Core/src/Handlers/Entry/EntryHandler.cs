namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler
	{
		public static PropertyMapper<IEntry, EntryHandler> EntryMapper = new PropertyMapper<IEntry, EntryHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEntry.Text)] = MapText,
			[nameof(IEntry.TextColor)] = MapTextColor,
			[nameof(IEntry.IsPassword)] = MapIsPassword,
			[nameof(IEntry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEntry.MaxLength)] = MapMaxLength,
			[nameof(IEntry.Placeholder)] = MapPlaceholder,
			[nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEntry.Font)] = MapFont,
			[nameof(IEntry.ReturnType)] = MapReturnType,
			[nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
			[nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing
		};

		public EntryHandler() : base(EntryMapper)
		{

		}

		public EntryHandler(PropertyMapper? mapper = null) : base(mapper ?? EntryMapper)
		{

		}
	}
}