#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		static Entry()
		{
			// Force VisualElement's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(Entry), typeof(VisualElement));
#endif
			VisualElement.s_forceStaticConstructor = true;

			// Adjust the mappings to preserve Controls.Entry legacy behaviors
#if ANDROID
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty.PropertyName, MapImeOptions);
#elif WINDOWS
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName, MapDetectReadingOrderFromContent);
#elif IOS
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(PlatformConfiguration.iOSSpecific.Entry.CursorColorProperty.PropertyName, MapCursorColor);
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(PlatformConfiguration.iOSSpecific.Entry.AdjustsFontSizeToFitWidthProperty.PropertyName, MapAdjustsFontSizeToFitWidth);
#endif
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(nameof(Text), MapText);
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(nameof(TextTransform), MapText);

#if IOS || ANDROID
			EntryHandler.Mapper.AppendToMapping(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
#endif

#if ANDROID
			EntryHandler.CommandMapper.PrependToMapping(nameof(IEntry.Focus), InputView.MapFocus);
#endif
		}
	}
}
