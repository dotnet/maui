namespace Microsoft.Maui.Platform
{

	// https://docs.gtk.org/gtk3/class.Image.html 

	// GtkImage has nothing like Aspect; maybe an ownerdrawn class is needed 
	// could be: https://docs.gtk.org/gtk3/class.DrawingArea.html
	// or Microsoft.Maui.Graphics.Platform.Gtk.GtkGraphicsView

	public class ImageView : Gtk.Image
	{

		public Gdk.Pixbuf? Image
		{
			get => Pixbuf;
			set => Pixbuf = value;
		}

	}

}