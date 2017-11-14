namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class PageContainer : Gtk.Object
    {
        public PageContainer(Xamarin.Forms.Page element, int index)
        {
            Page = element;
            Index = index;
        }

        public Xamarin.Forms.Page Page { get; }

        public int Index { get; set; }
    }
}