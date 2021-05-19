using System.IO;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public class NativeGraphicsService : IGraphicsService {

		public static NativeGraphicsService Instance = new NativeGraphicsService();

		private static Cairo.Context? _sharedContext;

		public Cairo.Context SharedContext {
			get {
				if (_sharedContext == null) {
					using var sf = new Cairo.ImageSurface(Cairo.Format.ARGB32, 1, 1);
					_sharedContext = new Cairo.Context(sf);
				}

				return _sharedContext;
			}
		}

		public string SystemFontName => NativeFontService.Instance.SystemFontName;

		public string BoldSystemFontName => NativeFontService.Instance.BoldSystemFontName;

		private static TextLayout? _textLayout;

		public TextLayout SharedTextLayout => _textLayout ??= new TextLayout(SharedContext) {
			HeightForWidth = true
		};

		public SizeF GetStringSize(string value, string fontName, float textWidth) {
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			lock (SharedTextLayout) {
				SharedTextLayout.FontFamily = fontName;

				return SharedTextLayout.GetSize(value, textWidth);
			}

		}

		public SizeF GetStringSize(string value, string fontName, float textWidth, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment) {
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			lock (SharedTextLayout) {
				SharedTextLayout.FontFamily = fontName;
				SharedTextLayout.HorizontalAlignment = horizontalAlignment;
				SharedTextLayout.VerticalAlignment = verticalAlignment;

				return SharedTextLayout.GetSize(value, textWidth);
			}
		}

		public IImage LoadImageFromStream(Stream stream, ImageFormat format = ImageFormat.Png) {
			var px = new Gdk.Pixbuf(stream);
			var img = new GtkImage(px);

			return img;
		}

		public BitmapExportContext CreateBitmapExportContext(int width, int height, float displayScale = 1) {
			return new GtkBitmapExportContext(width, height, displayScale);
		}

	}

}
