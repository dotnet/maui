#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapCursorColor(IEntryHandler handler, Entry entry)
		{
			Platform.TextExtensions.UpdateCursorColor(handler.PlatformView, entry);
		}

		public static void MapAdjustsFontSizeToFitWidth(IEntryHandler handler, Entry entry)
		{
			Platform.TextExtensions.UpdateAdjustsFontSizeToFitWidth(handler.PlatformView, entry);
		}

		public static void MapText(IEntryHandler handler, Entry entry)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, entry);

			// Any text update requires that we update any attributed string formatting
			handler.UpdateValue(nameof(IEntry.MaxLength));
			handler.UpdateValue(nameof(IEntry.CharacterSpacing));
			handler.UpdateValue(nameof(IEntry.HorizontalTextAlignment));
		}

		public static void MapCursorColor(EntryHandler handler, Entry entry) =>
			MapCursorColor((IEntryHandler)handler, entry);

		public static void MapAdjustsFontSizeToFitWidth(EntryHandler handler, Entry entry) =>
			MapAdjustsFontSizeToFitWidth((IEntryHandler)handler, entry);

		public static void MapText(EntryHandler handler, Entry entry) =>
			MapText((IEntryHandler)handler, entry);
	}
}
