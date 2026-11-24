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
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty.PropertyName, MapImeOptionsOrReturnType);
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(nameof(ReturnType), MapReturnType);
#elif WINDOWS
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName, MapDetectReadingOrderFromContent);
#elif IOS
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(PlatformConfiguration.iOSSpecific.Entry.CursorColorProperty.PropertyName, MapCursorColor);
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(PlatformConfiguration.iOSSpecific.Entry.AdjustsFontSizeToFitWidthProperty.PropertyName, MapAdjustsFontSizeToFitWidth);
#endif
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(nameof(Text), MapText);
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(nameof(TextTransform), MapText);

			// Material3 Entry Handler mappings
#if ANDROID
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				EntryHandler2.Mapper.ReplaceMapping<Entry, EntryHandler2>(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty.PropertyName, MapImeOptionsOrReturnType);
				EntryHandler2.Mapper.ReplaceMapping<Entry, EntryHandler2>(nameof(ReturnType), MapReturnType);
				EntryHandler2.Mapper.ReplaceMapping<Entry, EntryHandler2>(nameof(Text), MapText);
				EntryHandler2.Mapper.ReplaceMapping<Entry, EntryHandler2>(nameof(TextTransform), MapText);
				EntryHandler2.Mapper.AppendToMapping(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
				EntryHandler2.Mapper.AppendToMapping(nameof(VisualElement.IsVisible), InputView.MapIsVisible);
				EntryHandler2.CommandMapper.PrependToMapping(nameof(IEntry.Focus), InputView.MapFocus);
			}
#endif

#if IOS || ANDROID
			EntryHandler.Mapper.AppendToMapping(nameof(VisualElement.IsFocused), InputView.MapIsFocused);
			EntryHandler.Mapper.AppendToMapping(nameof(VisualElement.IsVisible), InputView.MapIsVisible);
#endif

#if ANDROID
			EntryHandler.CommandMapper.PrependToMapping(nameof(IEntry.Focus), InputView.MapFocus);
#endif
		}

#if ANDROID
		static void MapImeOptionsOrReturnType(IEntryHandler handler, Entry entry)
		{
			if (entry.IsSetExplicitly(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty))
				MapImeOptions(handler, entry);
			else
				UpdateReturnTypeFromImeOptions(handler);
		}

		static void MapImeOptionsOrReturnType(EntryHandler2 handler, Entry entry)
		{
			if (entry.IsSetExplicitly(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty))
				MapImeOptions(handler, entry);
			else
				UpdateReturnTypeFromImeOptions(handler);
		}

		static void UpdateReturnTypeFromImeOptions(IElementHandler handler)
		{
			// Clearing ImeOptions restores ReturnType; during initialization it may already be mapped.
			if (!PropertyMapperPassScope.IsRedundantBulkPassRedirect(
				handler, PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty.PropertyName, nameof(ReturnType)))
				handler.UpdateValue(nameof(ReturnType));
		}

		static void MapReturnType(IEntryHandler handler, Entry entry)
		{
			if (!entry.IsSetExplicitly(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty))
				EntryHandler.MapReturnType(handler, entry);
		}

		static void MapReturnType(EntryHandler2 handler, Entry entry)
		{
			if (!entry.IsSetExplicitly(PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty))
				EntryHandler2.MapReturnType(handler, entry);
		}
#endif
	}
}
