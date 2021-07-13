namespace Microsoft.Maui
{

	// https://developer.gnome.org/gtk3/stable/GtkImage.html

	// GtkImage has nothing like Aspect; maybe an ownerdrawn class is needed 
	// could be: https://developer.gnome.org/gtk3/stable/GtkDrawingArea.html
	// or Microsoft.Maui.Graphics.Native.Gtk.GtkGraphicsView

	public class ImageView : Gtk.Image
	{

		public Gdk.Pixbuf? Image
		{
			get => Pixbuf;
			set => Pixbuf = value;
		}

	}

}