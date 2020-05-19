namespace System.Maui.Platform.GTK.Controls
{
	public class PageContainer : Gtk.Object
	{
		public PageContainer(System.Maui.Page element, int index)
		{
			Page = element;
			Index = index;
		}

		public System.Maui.Page Page { get; }

		public int Index { get; set; }
	}
}
