namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapText(EntryHandler handler, Entry entry)
		{
			Platform.TextBoxExtensions.UpdateText(handler.PlatformView, entry);
		}
	}
}
