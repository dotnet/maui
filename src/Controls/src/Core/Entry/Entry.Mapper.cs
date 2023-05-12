#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		[Obsolete("Use EntryHandler.Mapper instead.")]
		public static IPropertyMapper<IEntry, EntryHandler> ControlsEntryMapper =
			new PropertyMapper<Entry, EntryHandler>(EntryHandler.Mapper);

		static CommandMapper<IEntry, IEntryHandler> ControlsCommandMapper = new(EntryHandler.CommandMapper)
		{
#if ANDROID
			[nameof(IEntry.Focus)] = MapFocus
#endif
		};

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
			EntryHandler.Mapper.ReplaceMapping<Entry, IEntryHandler>(nameof(TextTransform), MapText);

			EntryHandler.CommandMapper = ControlsCommandMapper;
		}
	}
}
