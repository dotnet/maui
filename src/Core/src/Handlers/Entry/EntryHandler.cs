namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler
	{
		public static PropertyMapper<IEntry, EntryHandler> EntryMapper = new PropertyMapper<IEntry, EntryHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
			[nameof(IEntry.Font)] = MapFont,
			[nameof(IEntry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IEntry.IsPassword)] = MapIsPassword,
			[nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEntry.MaxLength)] = MapMaxLength,
			[nameof(IEntry.Placeholder)] = MapPlaceholder,
			[nameof(IEntry.ReturnType)] = MapReturnType,
			[nameof(IEntry.Text)] = MapText,
			[nameof(IEntry.TextColor)] = MapTextColor,
			[nameof(IEntry.TextTransform)] = MapTextTransform
		};

		public EntryHandler() : base(EntryMapper)
		{

		}

		public EntryHandler(PropertyMapper? mapper = null) : base(mapper ?? EntryMapper)
		{

		}
	}
}