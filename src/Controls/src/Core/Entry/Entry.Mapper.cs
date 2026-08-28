#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			// Adjust the mappings to preserve Controls.Entry legacy behaviors
#if ANDROID
			EntryHandler.Mapper.ReplaceMappingForControls<Entry, IEntryHandler>(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty.PropertyName, MapImeOptions);
#elif WINDOWS
			EntryHandler.Mapper.ReplaceMappingForControls<Entry, IEntryHandler>(PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName, MapDetectReadingOrderFromContent);
#elif IOS
			EntryHandler.Mapper.ReplaceMappingForControls<Entry, IEntryHandler>(PlatformConfiguration.iOSSpecific.Entry.CursorColorProperty.PropertyName, MapCursorColor);
			EntryHandler.Mapper.ReplaceMappingForControls<Entry, IEntryHandler>(PlatformConfiguration.iOSSpecific.Entry.AdjustsFontSizeToFitWidthProperty.PropertyName, MapAdjustsFontSizeToFitWidth);
#endif
			EntryHandler.Mapper.ReplaceMappingForControls<Entry, IEntryHandler>(nameof(Text), MapText);
			EntryHandler.Mapper.ReplaceMappingForControls<Entry, IEntryHandler>(nameof(TextTransform), MapText);

			// Material3 Entry Handler mappings
#if ANDROID
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				EntryHandler2.Mapper.ReplaceMappingForControls<Entry, EntryHandler2>(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty.PropertyName, MapImeOptions);
				EntryHandler2.Mapper.ReplaceMappingForControls<Entry, EntryHandler2>(nameof(Text), MapText);
				EntryHandler2.Mapper.ReplaceMappingForControls<Entry, EntryHandler2>(nameof(TextTransform), MapText);
				EntryHandler2.Mapper.AppendToMappingForControls(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
				EntryHandler2.Mapper.AppendToMappingForControls(nameof(VisualElement.IsVisible), InputView.MapIsVisible);
				EntryHandler2.CommandMapper.PrependToMappingForControls(nameof(IEntry.Focus), InputView.MapFocus);
			}
#endif

#if IOS || ANDROID
			EntryHandler.Mapper.AppendToMappingForControls(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
			EntryHandler.Mapper.AppendToMappingForControls(nameof(VisualElement.IsVisible), InputView.MapIsVisible);
#endif

#if ANDROID
			EntryHandler.CommandMapper.PrependToMappingForControls(nameof(IEntry.Focus), InputView.MapFocus);
#endif
		}
	}
}
