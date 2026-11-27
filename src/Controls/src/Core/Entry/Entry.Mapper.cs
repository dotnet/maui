#nullable disable
#if ANDROID
using System.Runtime.CompilerServices;
#endif
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
			RemapReturnTypeForAndroid(EntryHandler.Mapper);
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
				RemapReturnTypeForAndroid(EntryHandler2.Mapper);
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
		static readonly ConditionalWeakTable<IPropertyMapper<IElement, IElementHandler>, object> s_returnTypeRemappedMappers = new();

		internal static void RemapReturnTypeForAndroid(IPropertyMapper<IElement, IElementHandler> mapper)
		{
			// Controls setup can run for multiple app builders. Wrap each mapper only once.
			lock (s_returnTypeRemappedMappers)
			{
				if (s_returnTypeRemappedMappers.TryGetValue(mapper, out _))
					return;

				mapper.ModifyMapping<Entry, IElementHandler>(nameof(ReturnType), static (handler, entry, previousMapping) =>
				{
					if (!HasImeOptionsOverride(entry))
						previousMapping?.Invoke(handler, entry);
				});
				s_returnTypeRemappedMappers.Add(mapper, new object());
			}
		}

		static bool HasImeOptionsOverride(Entry entry) =>
			PlatformConfiguration.AndroidSpecific.Entry.GetImeOptions(entry) != PlatformConfiguration.AndroidSpecific.ImeFlags.Default;

		static void MapImeOptionsOrReturnType(IEntryHandler handler, Entry entry)
		{
			if (HasImeOptionsOverride(entry))
				MapImeOptions(handler, entry);
			else
				UpdateReturnTypeFromImeOptions(handler);
		}

		static void MapImeOptionsOrReturnType(EntryHandler2 handler, Entry entry)
		{
			if (HasImeOptionsOverride(entry))
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
#endif
	}
}
