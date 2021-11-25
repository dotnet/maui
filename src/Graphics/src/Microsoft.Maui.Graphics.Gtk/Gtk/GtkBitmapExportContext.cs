using System.IO;
using System.Threading;

namespace Microsoft.Maui.Graphics.Platform.Gtk {

	public class GtkBitmapExportContext : BitmapExportContext {

		private PlatformCanvas _canvas;
		private Cairo.ImageSurface _surface;
		private Cairo.Context _context;
		private Gdk.Pixbuf? _pixbuf;

		public GtkBitmapExportContext(int width, int height, float dpi) : base(width, height, dpi) {
			_surface = new Cairo.ImageSurface(Cairo.Format.Argb32, width, height);
			_context = new Cairo.Context(_surface);

			_canvas = new PlatformCanvas {
				Context = _context
			};
		}

		public ImageFormat Format => ImageFormat.Png;

		public override ICanvas Canvas => _canvas;

		/// <summary>
		/// writes a pixbuf to stream
		/// </summary>
		/// <param name="stream"></param>
		public override void WriteToStream(Stream stream) {
			if (_pixbuf != null) {
				_pixbuf.SaveToStream(stream, Format);
			} else {
				_pixbuf = _surface.SaveToStream(stream, Format);
			}
		}

		private GtkImage? _image;

		public override IImage? Image {
			get {
				_pixbuf ??= _surface.CreatePixbuf();

				if (_pixbuf != null) return _image ??= new GtkImage(_pixbuf);

				return _image;
			}
		}

		public override void Dispose() {
			_canvas?.Dispose();
			_context?.Dispose();
			_surface?.Dispose();

			if (_pixbuf != null) {
				var previousValue = Interlocked.Exchange(ref _pixbuf, null);
				previousValue?.Dispose();
			}

			base.Dispose();
		}

	}

}
