namespace Microsoft.Maui.Graphics.Native.Gtk {

	public class GtkGraphicsView : global::Gtk.EventBox {

		private IDrawable? _drawable;
		private RectangleF _dirtyRect;
		private Color? _backgroundColor;

		public GtkGraphicsView() {
			AppPaintable = true;
			VisibleWindow = false;
		}

		protected override bool OnDrawn(Cairo.Context context) {
			if (_drawable == null) {
				return base.OnDrawn(context);
			}

			// ensure cr does not get disposed before it is passed back to Gtk
			var canvas = new NativeCanvas {Context = context};

			canvas.SaveState();

			if (_backgroundColor != null) {
				canvas.FillColor = _backgroundColor;
				canvas.FillRectangle(_dirtyRect);
			} else {
				canvas.ClipRectangle(_dirtyRect);
			}

			canvas.RestoreState();
			Drawable?.Draw(canvas, _dirtyRect);

			return base.OnDrawn(context);
		}

		public Color? BackgroundColor {
			get => _backgroundColor;
			set {
				_backgroundColor = value;
				QueueDraw();
			}
		}

		public IDrawable? Drawable {
			get => _drawable;
			set {
				_drawable = value;
				QueueDraw();
			}
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation) {
			_dirtyRect.Width = allocation.Width;
			_dirtyRect.Height = allocation.Height;
			base.OnSizeAllocated(allocation);
		}

	}

}
