#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Editor legacy behaviors
#if WINDOWS
			EditorHandler.Mapper.ReplaceMapping<Editor, IEditorHandler>(PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName, MapDetectReadingOrderFromContent);
#endif
			EditorHandler.Mapper.ReplaceMapping<Editor, IEditorHandler>(nameof(Text), MapText);
			EditorHandler.Mapper.ReplaceMapping<Editor, IEditorHandler>(nameof(TextTransform), MapText);

#if IOS || ANDROID
			EditorHandler.Mapper.AppendToMapping(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
#endif

#if ANDROID
			EditorHandler.CommandMapper.PrependToMapping(nameof(IEditor.Focus), InputView.MapFocus);
#endif
		}
	}
}