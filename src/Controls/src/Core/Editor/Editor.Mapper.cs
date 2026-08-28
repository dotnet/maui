#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Editor
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			// Adjust the mappings to preserve Controls.Editor legacy behaviors
#if WINDOWS
			EditorHandler.Mapper.ReplaceMappingForControls<Editor, IEditorHandler>(PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName, MapDetectReadingOrderFromContent);
#endif
			EditorHandler.Mapper.ReplaceMappingForControls<Editor, IEditorHandler>(nameof(Text), MapText);
			EditorHandler.Mapper.ReplaceMappingForControls<Editor, IEditorHandler>(nameof(TextTransform), MapText);

#if ANDROID
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				EditorHandler2.Mapper.ReplaceMappingForControls<Editor, EditorHandler2>(nameof(Text), MapText);
				EditorHandler2.Mapper.ReplaceMappingForControls<Editor, EditorHandler2>(nameof(TextTransform), MapText);
				EditorHandler2.Mapper.AppendToMappingForControls<Editor, EditorHandler2>(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
				EditorHandler2.CommandMapper.PrependToMappingForControls<Editor, EditorHandler2>(nameof(IEditor.Focus), InputView.MapFocus);
			}
#endif

#if IOS || ANDROID
			EditorHandler.Mapper.AppendToMappingForControls(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
			EditorHandler.Mapper.AppendToMappingForControls(nameof(VisualElement.IsVisible), InputView.MapIsVisible);
#endif

#if IOS || MACCATALYST
			EditorHandler.Mapper.AppendToMappingForControls<Editor, IEditorHandler>(nameof(AutoSize), MapAutoSize);
#endif

#if ANDROID
			EditorHandler.CommandMapper.PrependToMappingForControls(nameof(IEditor.Focus), InputView.MapFocus);
#endif
		}
	}
}