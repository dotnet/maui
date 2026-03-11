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

#if ANDROID
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				EditorHandler2.Mapper.ReplaceMapping<Editor, EditorHandler2>(nameof(Text), MapText);
				EditorHandler2.Mapper.ReplaceMapping<Editor, EditorHandler2>(nameof(TextTransform), MapText);
				EditorHandler2.Mapper.AppendToMapping<Editor, EditorHandler2>(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
				EditorHandler2.CommandMapper.PrependToMapping<Editor, EditorHandler2>(nameof(IEditor.Focus), InputView.MapFocus);
			}
#endif

#if IOS || ANDROID
			EditorHandler.Mapper.AppendToMapping(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
			EditorHandler.Mapper.AppendToMapping(nameof(VisualElement.IsVisible), InputView.MapIsVisible);
#endif

#if ANDROID
			EditorHandler.CommandMapper.PrependToMapping(nameof(IEditor.Focus), InputView.MapFocus);
#endif
		}
	}
}