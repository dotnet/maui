namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler
	{
		public static PropertyMapper<IEditor, EditorHandler> EditorMapper = new PropertyMapper<IEditor, EditorHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IEditor.Text)] = MapText,
			[nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IEditor.MaxLength)] = MapMaxLength,
			[nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled
		};

		public EditorHandler() : base(EditorMapper)
		{

		}

		public EditorHandler(PropertyMapper mapper) : base(mapper ?? EditorMapper)
		{

		}
	}
}