#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler
	{
		public static PropertyMapper<IEditor, EditorHandler> EditorMapper = new PropertyMapper<IEditor, EditorHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__
			[nameof(IEditor.Background)] = MapBackground,
#endif
			[nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEditor.Font)] = MapFont,
			[nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEditor.MaxLength)] = MapMaxLength,
			[nameof(IEditor.Placeholder)] = MapPlaceholder,
			[nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(IEditor.Text)] = MapText,
			[nameof(IEditor.TextColor)] = MapTextColor,
		};

		public EditorHandler() : base(EditorMapper)
		{
		}

		public EditorHandler(PropertyMapper? mapper = null) : base(mapper ?? EditorMapper)
		{

		}
	}
}
