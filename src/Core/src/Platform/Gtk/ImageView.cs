namespace Microsoft.Maui.Native
{

	// https://docs.gtk.org/gtk3/class.Imgage.html 

	// GtkImage has nothing like Aspect; maybe an ownerdrawn class is needed 
	// could be: https://docs.gtk.org/gtk3/class.DrawingArea.html
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