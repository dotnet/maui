namespace Microsoft.Maui.Controls
{
	public partial class Editor 
	{
		public static IPropertyMapper<IEditor, EditorHandler> ControlsEditorMapper = 
			new PropertyMapper<Editor, EditorHandler>(EditorHandler.EditorMapper)
		{
			[nameof(Text)] = MapText,
			[nameof(TextTransform)] = MapText,
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Editor legacy behaviors
			EditorHandler.EditorMapper = ControlsEditorMapper;
		}
	}
}