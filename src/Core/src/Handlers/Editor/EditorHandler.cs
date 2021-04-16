#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler
	{
		public static PropertyMapper<IEditor, EditorHandler> EditorMapper = new PropertyMapper<IEditor, EditorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEditor.Font)] = MapFont,
			[nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEditor.MaxLength)] = MapMaxLength,
			[nameof(IEditor.Placeholder)] = MapPlaceholder,
			[nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(IEditor.Text)] = MapText,
			[nameof(IEditor.Foreground)] = MapForeground,
		};

		public EditorHandler() : base(EditorMapper)
		{
		}

		public EditorHandler(PropertyMapper? mapper = null) : base(mapper ?? EditorMapper)
		{

		}
	}
}