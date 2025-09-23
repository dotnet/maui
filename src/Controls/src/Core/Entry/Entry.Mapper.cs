#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		internal static new void RemapForControls()
		{
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
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(nameof(TextTransform), MapTextTransform);

#if IOS || ANDROID
			EntryHandler.Mapper.AppendToMapping(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
#endif

#if ANDROID
			EntryHandler.CommandMapper.PrependToMapping(nameof(IEntry.Focus), InputView.MapFocus);
#endif
		}

		static void MapTextTransform(IEntryHandler handler, Entry entry)
		{
			if (entry.IsConnectingHandler())
			{
				// If we're connecting the handler, we don't want to map the text multiple times.
				return;
			}

			MapText(handler, entry);
		}
	}
}