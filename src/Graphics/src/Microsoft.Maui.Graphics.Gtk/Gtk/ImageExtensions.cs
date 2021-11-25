using System;
using System.IO;

namespace Microsoft.Maui.Graphics.Platform.Gtk {

	public static class ImageExtensions {

		public static string ToImageExtension(this ImageFormat imageFormat) =>
			imageFormat switch {
				ImageFormat.Bmp => "bmp",
				ImageFormat.Png => "png",
				ImageFormat.Jpeg => "jpeg",
				ImageFormat.Gif => "gif",
				ImageFormat.Tiff => "tiff",
				_ => throw new ArgumentOutOfRangeException(nameof(imageFormat), imageFormat, null)
			};

		public static Gdk.Pixbuf? SaveToStream(this Cairo.ImageSurface? surface, Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1) {
			if (surface == null)
				return null;
			try {
				var px = surface.CreatePixbuf();
				SaveToStream(px, stream, format, quality);

				return px;
			} catch (Exception ex) {
				Logger.Error(ex);

				return default;
			}
		}

		public static bool SaveToStream(this Gdk.Pixbuf? pixbuf, Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1) {
			if (pixbuf == null)
				return false;

			try {
				var puf = pixbuf.SaveToBuffer(format.ToImageExtension());
				stream.Write(puf, 0, puf.Length);
				puf = null;
			} catch (Exception ex) {
				Logger.Error(ex);

				return false;
			}

			return true;
		}

		public static Gdk.Pixbuf? CreatePixbuf(this Cairo.ImageSurface? surface) {
			if (surface == null)
				return null;

			var surfaceData = surface.Data;
			var nbytes = surface.Format == Cairo.Format.Argb32 ? 4 : 3;
			var pixData = new byte[surfaceData.Length / 4 * nbytes];

			var i = 0;
			var n = 0;
			var stride = surface.Stride;
			var ncols = surface.Width;

			if (BitConverter.IsLittleEndian) {
				var row = surface.Height;

				while (row-- > 0) {
					var prevPos = n;
					var col = ncols;

					while (col-- > 0) {
						var alphaFactor = nbytes == 4 ? 255d / surfaceData[n + 3] : 1;
						pixData[i] = (byte) (surfaceData[n + 2] * alphaFactor + 0.5);
						pixData[i + 1] = (byte) (surfaceData[n + 1] * alphaFactor + 0.5);
						pixData[i + 2] = (byte) (surfaceData[n + 0] * alphaFactor + 0.5);

						if (nbytes == 4)
							pixData[i + 3] = surfaceData[n + 3];

						n += 4;
						i += nbytes;
					}

					n = prevPos + stride;
				}
			} else {
				var row = surface.Height;

				while (row-- > 0) {
					var prevPos = n;
					var col = ncols;

					while (col-- > 0) {
						var alphaFactor = nbytes == 4 ? 255d / surfaceData[n + 3] : 1;
						pixData[i] = (byte) (surfaceData[n + 1] * alphaFactor + 0.5);
						pixData[i + 1] = (byte) (surfaceData[n + 2] * alphaFactor + 0.5);
						pixData[i + 2] = (byte) (surfaceData[n + 3] * alphaFactor + 0.5);

						if (nbytes == 4)
							pixData[i + 3] = surfaceData[n + 0];

						n += 4;
						i += nbytes;
					}

					n = prevPos + stride;
				}
			}

			return new Gdk.Pixbuf(pixData, Gdk.Colorspace.Rgb, nbytes == 4, 8, surface.Width, surface.Height, surface.Width * nbytes, null);
		}

		public static Cairo.Pattern? CreatePattern(this Gdk.Pixbuf? pixbuf, double scaleFactor) {
			if (pixbuf == null)
				return null;

			using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, (int) (pixbuf.Width * scaleFactor), (int) (pixbuf.Height * scaleFactor));
			using var context = new Cairo.Context(surface);
			context.Scale(surface.Width / (double) pixbuf.Width, surface.Height / (double) pixbuf.Height);
			Gdk.CairoHelper.SetSourcePixbuf(context, pixbuf, 0, 0);
			context.Paint();
			surface.Flush();

			var pattern = new Cairo.SurfacePattern(surface);

			var matrix = new Cairo.Matrix();
			matrix.Scale(scaleFactor, scaleFactor);
			pattern.Matrix = matrix;

			return pattern;

		}

	}

}
