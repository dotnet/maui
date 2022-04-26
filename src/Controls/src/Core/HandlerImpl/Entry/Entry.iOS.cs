namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapText(EntryHandler handler, Entry entry)
		{
			Platform.TextExtensions.UpdateText(handler.PlatformView, entry);
		}
	}
}
