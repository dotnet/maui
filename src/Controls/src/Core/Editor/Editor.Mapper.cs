#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		static Editor()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
			RemappingHelper.EnsureBaseTypeRemapped(typeof(Editor), typeof(VisualElement));

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