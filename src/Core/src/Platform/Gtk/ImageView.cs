using Cairo;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Platform
{

	// https://docs.gtk.org/gtk3/class.Image.html 

	// GtkImage has nothing like Aspect; maybe an ownerdrawn class is needed 
	// could be: https://docs.gtk.org/gtk3/class.DrawingArea.html
	// or Microsoft.Maui.Graphics.Platform.Gtk.GtkGraphicsView

	public class ImageView : Gtk.DrawingArea
	{
		Gdk.Pixbuf? Pixbuf;

		public Gdk.Pixbuf? Image
		{
			get => Pixbuf;
			set => Pixbuf = value;
		}

		Aspect _aspect;

		public Aspect Aspect
		{
			get => _aspect;
			set
			{
				_aspect = value;
			}
		}

		protected override bool OnDrawn(Context cr)
		{
			if (Image is not { } image)
				return true;
			
			var a = Allocation;

			// HACK: Gtk sends sometimes an expose/draw event while the widget reallocates.
			//       In that case we would draw in the wrong area, which may lead to artifacts
			//       if no other widget updates it. Alternative: we could clip the
			//       allocation bounds, but this may have other issues.
			if (a.Width == 1 && a.Height == 1 && a.X == -1 && a.Y == -1) // the allocation coordinates on reallocation
				return base.OnDrawn(cr);

			var xalign = 0;
			var yalign = 0;
			int x = (int)((a.Width - (float)image.Width) * xalign);
			int y = (int)((a.Height - (float)image.Height) * yalign);
			if (x < 0) x = 0;
			if (y < 0) y = 0;

			cr.DrawPixbuf(image, x, y, a.Width, a.Height);
			
			return true;
		}
	}

}