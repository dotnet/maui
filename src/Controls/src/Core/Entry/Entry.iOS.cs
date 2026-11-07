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
			var wasEmpty = string.IsNullOrEmpty(handler.PlatformView?.Text);
			Platform.TextExtensions.UpdateText(handler.PlatformView, entry);
			EntryHandler.MapFormatting(handler, entry);
			if (wasEmpty != string.IsNullOrEmpty(entry.Text))
				handler.UpdateValue(nameof(IEntry.ClearButtonVisibility));
		}

		public static void MapCursorColor(EntryHandler handler, Entry entry) =>
			MapCursorColor((IEntryHandler)handler, entry);

		public static void MapAdjustsFontSizeToFitWidth(EntryHandler handler, Entry entry) =>
			MapAdjustsFontSizeToFitWidth((IEntryHandler)handler, entry);

		public static void MapText(EntryHandler handler, Entry entry) =>
			MapText((IEntryHandler)handler, entry);
	}
}
