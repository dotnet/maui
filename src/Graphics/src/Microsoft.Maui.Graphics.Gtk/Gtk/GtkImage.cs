using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics.Platform.Gtk {

	public class GtkImage : IImage {

		public GtkImage(Gdk.Pixbuf pix) {
			_pixbuf = pix;
		}

		private Gdk.Pixbuf? _pixbuf;

		// https://developer.gnome.org/gdk-pixbuf/stable/gdk-pixbuf-The-GdkPixbuf-Structure.html
		public Gdk.Pixbuf? NativeImage => _pixbuf;

		public void Draw(ICanvas canvas, RectF dirtyRect) {
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, (float) Math.Round(dirtyRect.Width), (float) Math.Round(dirtyRect.Height));
		}

		public void Dispose() {
			var previousValue = Interlocked.Exchange(ref _pixbuf, null);
			previousValue?.Dispose();
		}

		public float Width => NativeImage?.Width ?? 0;

		public float Height => NativeImage?.Width ?? 0;

		[GtkMissingImplementation]
		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false) {
			return this;
		}

		[GtkMissingImplementation]
		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false) {
			return this;
		}

		[GtkMissingImplementation]
		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false) {
			return this;
		}

		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1) {
			NativeImage.SaveToStream(stream, format, quality);
		}

		public async Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1) {
			await Task.Run(() => NativeImage.SaveToStream(stream, format, quality));
		}

		public IImage ToImage(int width, int height, float scale = 1f)
		{
			using var context = new GtkBitmapExportContext(width, height, scale);
			context.Canvas.Scale(scale, scale);
			Draw(context.Canvas, new RectF(0, 0, (float)width / scale, (float)height / scale));
			return context.Image;
		}

	}

}
