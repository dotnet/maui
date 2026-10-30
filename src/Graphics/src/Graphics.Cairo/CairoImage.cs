using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// An <see cref="IImage"/> backed by a Cairo <c>ARGB32</c> (premultiplied) image surface.
	/// </summary>
	/// <remarks>
	/// Decoding and encoding are done by gdk-pixbuf (PNG, JPEG, ... whatever loaders the runtime ships);
	/// everything else (resizing, drawing) is Cairo.
	/// </remarks>
	public class CairoImage : IImage
	{
		private CairoImageSurface _surface;
		private readonly bool _ownsSurface;

		/// <summary>
		/// Wraps a Cairo image surface.
		/// </summary>
		/// <param name="surface">An <see cref="CairoFormat.Argb32"/> or <see cref="CairoFormat.Rgb24"/> surface.</param>
		/// <param name="ownsSurface">Whether this image disposes <paramref name="surface"/> when it is disposed.</param>
		public CairoImage(CairoImageSurface surface, bool ownsSurface = true)
		{
			_surface = surface ?? throw new ArgumentNullException(nameof(surface));
			_ownsSurface = ownsSurface;
		}

		/// <summary>
		/// Gets the underlying Cairo surface, or <see langword="null"/> once this image is disposed.
		/// </summary>
		public CairoImageSurface PlatformRepresentation => _surface;

		/// <inheritdoc/>
		public float Width => _surface?.Width ?? 0;

		/// <inheritdoc/>
		public float Height => _surface?.Height ?? 0;

		/// <inheritdoc/>
		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false) =>
			Downsize(maxWidthOrHeight, maxWidthOrHeight, disposeOriginal);

		/// <inheritdoc/>
		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			ThrowIfDisposed();

			var width = Width;
			var height = Height;
			if (width <= 0 || height <= 0)
				return this;

			var scale = Math.Min(1f, Math.Min(maxWidth / width, maxHeight / height));
			if (scale >= 1f)
				return this;

			var newWidth = Math.Max(1, (float)Math.Round(width * scale));
			var newHeight = Math.Max(1, (float)Math.Round(height * scale));
			return Resize(newWidth, newHeight, ResizeMode.Stretch, disposeOriginal);
		}

		/// <inheritdoc/>
		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
		{
			ThrowIfDisposed();

			var pixelWidth = Math.Max(1, (int)width);
			var pixelHeight = Math.Max(1, (int)height);

			var target = new CairoImageSurface(CairoFormat.Argb32, pixelWidth, pixelHeight);
			using (var context = new CairoContext(target))
			using (var canvas = new CairoCanvas(context))
			{
				var fx = pixelWidth / Width;
				var fy = pixelHeight / Height;

				var w = Width;
				var h = Height;

				if (resizeMode == ResizeMode.Fit)
				{
					var f = Math.Min(fx, fy);
					w *= f;
					h *= f;
				}
				else if (resizeMode == ResizeMode.Bleed)
				{
					var f = Math.Max(fx, fy);
					w *= f;
					h *= f;
				}
				else
				{
					w = pixelWidth;
					h = pixelHeight;
				}

				canvas.DrawImage(this, (pixelWidth - w) / 2, (pixelHeight - h) / 2, w, h);
			}

			target.Flush();
			var result = new CairoImage(target);
			if (disposeOriginal)
				Dispose();
			return result;
		}

		/// <inheritdoc/>
		/// <exception cref="PlatformNotSupportedException">For <see cref="ImageFormat.Gif"/>: gdk-pixbuf cannot write GIF.</exception>
		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			var bytes = Encode(format, quality);
			stream.Write(bytes, 0, bytes.Length);
		}

		/// <inheritdoc/>
		public async Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			var bytes = Encode(format, quality);
			await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
		}

		private byte[] Encode(ImageFormat format, float quality)
		{
			ThrowIfDisposed();

			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException(nameof(quality), "quality must be in the range of 0..1");

			string type;
			string[] keys = Array.Empty<string>();
			string[] values = Array.Empty<string>();
			var flatten = false;

			switch (format)
			{
				case ImageFormat.Png:
					type = "png";
					break;
				case ImageFormat.Jpeg:
					type = "jpeg";
					keys = new[] { "quality" };
					values = new[] { ((int)Math.Round(quality * 100)).ToString(System.Globalization.CultureInfo.InvariantCulture) };
					flatten = true;
					break;
				case ImageFormat.Bmp:
					type = "bmp";
					break;
				case ImageFormat.Tiff:
					type = "tiff";
					break;
				case ImageFormat.Gif:
					throw new PlatformNotSupportedException($"gdk-pixbuf does not support writing {format} images.");
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}

			using (var pixbuf = ToPixbuf(flatten))
				return pixbuf.SaveToBuffer(type, keys, values);
		}

		/// <summary>
		/// Converts this image to a <see cref="Gdk.Pixbuf"/> (non-premultiplied RGBA).
		/// </summary>
		/// <param name="flattenOnWhite">When <see langword="true"/> the result has no alpha channel: transparent areas become white.</param>
		/// <returns>A new pixbuf, owned by the caller.</returns>
		public Gdk.Pixbuf ToPixbuf(bool flattenOnWhite = false)
		{
			ThrowIfDisposed();

			var surface = _surface;
			surface.Flush();
			var width = surface.Width;
			var height = surface.Height;
			var stride = surface.Stride;
			var hasAlpha = !flattenOnWhite;

			var pixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, hasAlpha, 8, width, height);
			var channels = hasAlpha ? 4 : 3;
			var rowStride = pixbuf.Rowstride;
			var source = new int[width];
			var row = new byte[rowStride];
			var destination = pixbuf.Pixels;
			var opaque = surface.Format == CairoFormat.Rgb24;

			for (var y = 0; y < height; y++)
			{
				Marshal.Copy(surface.DataPtr + y * stride, source, 0, width);

				for (var x = 0; x < width; x++)
				{
					var argb = (uint)source[x];
					var a = opaque ? 255u : argb >> 24;
					var r = (argb >> 16) & 0xFF;
					var g = (argb >> 8) & 0xFF;
					var b = argb & 0xFF;

					if (flattenOnWhite)
					{
						// premultiplied over white: c + (255 - a)
						r = Math.Min(255u, r + (255u - a));
						g = Math.Min(255u, g + (255u - a));
						b = Math.Min(255u, b + (255u - a));
					}
					else if (a != 0 && a != 255)
					{
						r = Math.Min(255u, (r * 255u + a / 2) / a);
						g = Math.Min(255u, (g * 255u + a / 2) / a);
						b = Math.Min(255u, (b * 255u + a / 2) / a);
					}

					var o = x * channels;
					row[o] = (byte)r;
					row[o + 1] = (byte)g;
					row[o + 2] = (byte)b;
					if (hasAlpha)
						row[o + 3] = (byte)a;
				}

				Marshal.Copy(row, 0, destination + y * rowStride, rowStride);
			}

			return pixbuf;
		}

		/// <summary>
		/// Creates an image from a <see cref="Gdk.Pixbuf"/>.
		/// </summary>
		/// <param name="pixbuf">The pixbuf (8 bits per sample, 3 or 4 channels). It is copied, not retained.</param>
		/// <returns>A new image.</returns>
		public static CairoImage FromPixbuf(Gdk.Pixbuf pixbuf)
		{
			if (pixbuf == null)
				throw new ArgumentNullException(nameof(pixbuf));

			var width = pixbuf.Width;
			var height = pixbuf.Height;
			var channels = pixbuf.NChannels;
			var hasAlpha = pixbuf.HasAlpha;
			var rowStride = pixbuf.Rowstride;
			var source = pixbuf.Pixels;

			var surface = new CairoImageSurface(hasAlpha ? CairoFormat.Argb32 : CairoFormat.Rgb24, width, height);
			var destination = surface.DataPtr;
			var stride = surface.Stride;
			var row = new byte[rowStride];
			var output = new int[width];

			for (var y = 0; y < height; y++)
			{
				Marshal.Copy(source + y * rowStride, row, 0, rowStride);

				for (var x = 0; x < width; x++)
				{
					var o = x * channels;
					uint r = row[o];
					uint g = row[o + 1];
					uint b = row[o + 2];
					uint a = hasAlpha ? row[o + 3] : 255u;

					if (a != 255)
					{
						r = (r * a + 127) / 255;
						g = (g * a + 127) / 255;
						b = (b * a + 127) / 255;
					}

					output[x] = unchecked((int)((a << 24) | (r << 16) | (g << 8) | b));
				}

				Marshal.Copy(output, 0, destination + y * stride, width);
			}

			surface.MarkDirty();
			return new CairoImage(surface);
		}

		/// <summary>
		/// Decodes an image from a stream (PNG, JPEG, ... anything gdk-pixbuf can read). EXIF orientation is applied.
		/// </summary>
		/// <param name="stream">The stream, read from its current position to the end.</param>
		/// <param name="formatHint">Ignored: the format is detected from the data.</param>
		/// <returns>The image, or <see langword="null"/> when the data cannot be decoded.</returns>
		public static IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			byte[] bytes;
			if (stream is MemoryStream memory && memory.TryGetBuffer(out var segment) && segment.Offset == 0 && memory.Position == 0 && segment.Count == segment.Array.Length)
			{
				bytes = segment.Array;
			}
			else
			{
				using (var copy = new MemoryStream())
				{
					stream.CopyTo(copy);
					bytes = copy.ToArray();
				}
			}

			return FromBytes(bytes);
		}

		/// <summary>
		/// Decodes an image from memory.
		/// </summary>
		/// <param name="bytes">The encoded image.</param>
		/// <returns>The image, or <see langword="null"/> when the data cannot be decoded.</returns>
		public static CairoImage FromBytes(byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));
			if (bytes.Length == 0)
				return null;

			Gdk.Pixbuf pixbuf = null;
			Gdk.Pixbuf oriented = null;
			try
			{
				pixbuf = new Gdk.Pixbuf(bytes);
				oriented = pixbuf.ApplyEmbeddedOrientation() ?? pixbuf;
				return FromPixbuf(oriented);
			}
			catch (GLib.GException)
			{
				return null;
			}
			finally
			{
				if (oriented != null && !ReferenceEquals(oriented, pixbuf))
					oriented.Dispose();
				pixbuf?.Dispose();
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			var surface = Interlocked.Exchange(ref _surface, null);
			if (_ownsSurface)
				surface?.Dispose();
		}

		/// <inheritdoc/>
		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.DrawImage(this, dirtyRect.Left, dirtyRect.Top, (float)Math.Round(dirtyRect.Width), (float)Math.Round(dirtyRect.Height));
		}

		/// <inheritdoc/>
		/// <returns>This image: it already is the platform image.</returns>
		public IImage ToPlatformImage() => this;

		private void ThrowIfDisposed()
		{
			if (_surface == null)
				throw new ObjectDisposedException(nameof(CairoImage));
		}
	}
}
