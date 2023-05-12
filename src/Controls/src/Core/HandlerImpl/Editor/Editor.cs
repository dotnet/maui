#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		[Obsolete("Use EditorHandler.Mapper instead.")]
		public static IPropertyMapper<IEditor, EditorHandler> ControlsEditorMapper =
			new PropertyMapper<Editor, EditorHandler>(EditorHandler.Mapper);

		static CommandMapper<IEditor, IEditorHandler> ControlsCommandMapper = new(EditorHandler.CommandMapper)
		{
#if ANDROID
			[nameof(IEditor.Focus)] = MapFocus
#endif
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Editor legacy behaviors
#if WINDOWS
			EditorHandler.Mapper.ReplaceMapping<Editor, IEditorHandler>(PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName, MapDetectReadingOrderFromContent);
#endif
			EditorHandler.Mapper.ReplaceMapping<Editor, IEditorHandler>(nameof(Text), MapText);
			EditorHandler.Mapper.ReplaceMapping<Editor, IEditorHandler>(nameof(TextTransform), MapText);

			EditorHandler.CommandMapper = ControlsCommandMapper;
		}
	}
}