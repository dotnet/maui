#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static IPropertyMapper<IEntry, EntryHandler> ControlsEntryMapper =
			new PropertyMapper<Entry, EntryHandler>(EntryHandler.Mapper)
			{
#if ANDROID
				[PlatformConfiguration.AndroidSpecific.Entry.ImeOptionsProperty.PropertyName] = MapImeOptions,
#elif WINDOWS
				[PlatformConfiguration.WindowsSpecific.InputView.DetectReadingOrderFromContentProperty.PropertyName] = MapDetectReadingOrderFromContent,
#elif IOS
				[PlatformConfiguration.iOSSpecific.Entry.CursorColorProperty.PropertyName] = MapCursorColor,
				[PlatformConfiguration.iOSSpecific.Entry.AdjustsFontSizeToFitWidthProperty.PropertyName] = MapAdjustsFontSizeToFitWidth,
#endif
				[nameof(Text)] = MapText,
				[nameof(TextTransform)] = MapText,
			};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Entry legacy behaviors
			EntryHandler.Mapper = ControlsEntryMapper;

#if ANDROID
			EntryHandler.CommandMapper.PrependToMapping(nameof(IEntry.Focus), MapFocus);
#endif
		}
	}
}
