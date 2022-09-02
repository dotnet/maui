namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapDetectReadingOrderFromContent(EntryHandler handler, Entry entry)
		{
			Platform.InputViewExtensions.UpdateDetectReadingOrderFromContent(handler.PlatformView, entry);
		}

		public static void MapText(EntryHandler handler, Entry entry)
		{
			Platform.TextBoxExtensions.UpdateText(handler.PlatformView, entry);
		}
	}
}
