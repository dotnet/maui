#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler
	{
		public static IPropertyMapper<IEditor, EditorHandler> EditorMapper = new PropertyMapper<IEditor, EditorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEditor.Background)] = MapBackground,
			[nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEditor.Font)] = MapFont,
			[nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
			[nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
			[nameof(IEditor.MaxLength)] = MapMaxLength,
			[nameof(IEditor.Placeholder)] = MapPlaceholder,
			[nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
			[nameof(IEditor.Text)] = MapText,
			[nameof(IEditor.TextColor)] = MapTextColor,
			[nameof(IEditor.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
			[nameof(IEditor.VerticalTextAlignment)] = MapVerticalTextAlignment,
			[nameof(IEditor.Keyboard)] = MapKeyboard,
			[nameof(IEditor.CursorPosition)] = MapCursorPosition,
			[nameof(IEditor.SelectionLength)] = MapSelectionLength
		};

		public EditorHandler() : base(EditorMapper)
		{
		}

		public EditorHandler(IPropertyMapper? mapper = null) : base(mapper ?? EditorMapper)
		{

		}
	}
}
