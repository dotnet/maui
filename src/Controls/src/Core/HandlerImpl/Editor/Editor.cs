namespace Microsoft.Maui.Controls
{
	public partial class Editor 
	{
		public static IPropertyMapper<IEditor, EditorHandler> ControlsEditorMapper = 
			new PropertyMapper<Editor, EditorHandler>(EditorHandler.Mapper)
			{
#if __IOS__
			[nameof(AutoSize)] = MapAutoSize,
#endif
			[nameof(Text)] = MapText,
			[nameof(TextTransform)] = MapText,
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Editor legacy behaviors
			EditorHandler.Mapper = ControlsEditorMapper;
		}
	}
}