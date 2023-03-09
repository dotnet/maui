namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapDetectReadingOrderFromContent(IEntryHandler handler, Entry entry)
		{
			Platform.InputViewExtensions.UpdateDetectReadingOrderFromContent(handler.PlatformView, entry);
		}

		public static void MapText(IEntryHandler handler, Entry entry)
		{
			Platform.TextBoxExtensions.UpdateText(handler.PlatformView, entry);
		}

		public static void MapDetectReadingOrderFromContent(EntryHandler handler, Entry entry) =>
			MapDetectReadingOrderFromContent((IEntryHandler)handler, entry);

		public static void MapText(EntryHandler handler, Entry entry) =>
			MapText((IEntryHandler)handler, entry);
	}
}
