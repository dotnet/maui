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
			MapFormatting(handler, entry);
		}

		public static void MapCursorColor(EntryHandler handler, Entry entry) =>
			MapCursorColor((IEntryHandler)handler, entry);

		public static void MapAdjustsFontSizeToFitWidth(EntryHandler handler, Entry entry) =>
			MapAdjustsFontSizeToFitWidth((IEntryHandler)handler, entry);

		public static void MapText(EntryHandler handler, Entry entry) =>
			MapText((IEntryHandler)handler, entry);

		private static void MapFormatting(IEntryHandler handler, IEntry entry)
		{
			handler.PlatformView?.UpdateMaxLength(entry);

			// Update all of the attributed text formatting properties
			handler.PlatformView?.UpdateCharacterSpacing(entry);

			// Setting any of those may have removed text alignment settings,
			// so we need to make sure those are applied, too
			handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
		}
	}
}
