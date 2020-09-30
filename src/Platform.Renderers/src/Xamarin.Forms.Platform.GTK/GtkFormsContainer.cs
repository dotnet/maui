using System;
using Cairo;
using Gdk;
using Gtk;

namespace Xamarin.Forms.Platform.GTK
{
	/// <summary>
	/// A generic container to embed visual elements.
	/// </summary>
	public class GtkFormsContainer : Gtk.EventBox
	{
		Color _backgroundColor;

		public GtkFormsContainer()
		{
			VisibleWindow = false;
			BackgroundColor = Color.Transparent;
		}

		Color BackgroundColor
		{
			get => _backgroundColor;
			set
			{
				_backgroundColor = value;
				QueueDraw();
			}
		}

		public void SetBackgroundColor(Color color)
		{
			BackgroundColor = color;
		}

		/// <summary>
		/// Subclasses can override this method to draw custom content over the background.
		/// </summary>
		/// <param name="clipArea">The clipped area that needs a redraw.</param>
		/// <param name="cr">Context.</param>
		protected virtual void Draw(Gdk.Rectangle clipArea, Context cr)
		{
		}

		protected override bool OnExposeEvent(EventExpose evnt)
		{
			using (var cr = CairoHelper.Create(GdkWindow))
			{
				// Windowless widgets receive expose events with the whole area of
				// of it's container, so we firt clip it to the allocation of the
				// widget it self
				var clipBox = Clip(evnt.Area);
				// Draw first the background with the color defined in BackgroundColor
				cr.Rectangle(clipBox.X, clipBox.Y, clipBox.Width, clipBox.Height);
				cr.Clip();
				cr.Save();
				cr.SetSourceRGBA(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
				cr.Operator = Operator.Over;
				cr.Paint();
				cr.Restore();
				// Let subclasses perform their own drawing operations
				cr.Translate(Allocation.X, Allocation.Y);
				Draw(clipBox, cr);
				// And finally forward the expose event to the children
				return base.OnExposeEvent(evnt);
			}
		}

		Gdk.Rectangle Clip(Gdk.Rectangle area)
		{
			int startX = Math.Max(area.X, Allocation.X);
			int endX = Math.Min(area.X + area.Width, Allocation.X + Allocation.Width);
			int startY = Math.Max(area.Y, Allocation.Y);
			int endY = Math.Min(area.Y + area.Height, Allocation.Y + Allocation.Height);
			return new Gdk.Rectangle(startX, startY, endX - startX, endY - startY);
		}
	}
}
