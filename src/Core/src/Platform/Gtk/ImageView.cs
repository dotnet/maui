using System;
using System.Net.Mime;
using Cairo;
using Gtk;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Rectangle = Gdk.Rectangle;

namespace Microsoft.Maui.Platform
{

	// https://docs.gtk.org/gtk3/class.Image.html 

	// GtkImage has nothing like Aspect; maybe an ownerdrawn class is needed 
	// could be: https://docs.gtk.org/gtk3/class.DrawingArea.html
	// or Microsoft.Maui.Graphics.Platform.Gtk.GtkGraphicsView

	public class ImageView : Gtk.DrawingArea
	{

		public ImageView()
		{
			CanFocus = true;
			AddEvents((int)Gdk.EventMask.AllEventsMask);
		}

		Gdk.Pixbuf? Pixbuf;

		public Gdk.Pixbuf? Image
		{
			get => Pixbuf;
			set
			{
				Pixbuf = value;
				QueueResize();
			}
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
			var a = Allocation;
			var stc = this.StyleContext;
			stc.RenderBackground(cr, 0, 0, a.Width, a.Height);

			if (Image is not { } image)
				return true;

			// HACK: Gtk sends sometimes a draw event while the widget reallocates.
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

			return base.OnDrawn(cr);
		}

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimum_size, out int natural_size)
		{
			base.OnAdjustSizeRequest(orientation, out minimum_size, out natural_size);

			if (Image is not { })
				return;

		}

		protected override void OnRealized()
		{
			base.OnRealized();
		}

		protected override SizeRequestMode OnGetRequestMode()
		{
			return SizeRequestMode.HeightForWidth;
		}

		protected override void OnSizeAllocated(Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

		}

		protected override void OnGetPreferredHeightForWidth(int width, out int minimum_height, out int natural_height)
		{
			base.OnGetPreferredHeightForWidth(width, out minimum_height, out natural_height);

			if (Image is { })
			{

				minimum_height = Image.Height * width / Image.Width;
				natural_height = Math.Max(Image.Height, minimum_height);
			}
		}

		protected override void OnGetPreferredWidthForHeight(int height, out int minimum_width, out int natural_width)
		{
			base.OnGetPreferredWidthForHeight(height, out minimum_width, out natural_width);

			if (Image is { })
			{
				minimum_width = Image.Width * height / Image.Height;
				natural_width = Math.Max(Image.Width, minimum_width);

			}
		}

	}

}