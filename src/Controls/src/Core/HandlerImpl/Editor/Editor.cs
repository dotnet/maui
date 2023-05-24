#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		public static IPropertyMapper<IEditor, EditorHandler> ControlsEditorMapper =
			new PropertyMapper<Editor, EditorHandler>(EditorHandler.Mapper)
			{
#if WINDOWS
				[PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName] = MapDetectReadingOrderFromContent,
#endif
				[nameof(Text)] = MapText,
				[nameof(TextTransform)] = MapText,
			};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Editor legacy behaviors
			EditorHandler.Mapper = ControlsEditorMapper;

#if ANDROID
			EditorHandler.CommandMapper.PrependToMapping(nameof(IEditor.Focus), MapFocus);
#endif
		}
	}
}