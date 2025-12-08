using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics.Platform
{
#if NETSTANDARD || NETSTANDARD2_0 || NETSTANDARD2_1 || (NET6_0_OR_GREATER && !PLATFORM) || TIZEN || TIZEN40
	/// <summary>
	/// Provides a platform-agnostic implementation of an image.
	/// </summary>
	public class PlatformImage : IImage
	{
		private readonly byte[] _bytes;
		private readonly int _width;
		private readonly int _height;
		private readonly ImageFormat _originalFormat;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatformImage"/> class with the specified bytes and format.
		/// </summary>
		/// <param name="bytes">The raw image data.</param>
		/// <param name="originalFormat">The format of the image data (default is PNG).</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="bytes"/> is null.</exception>
		public PlatformImage(byte[] bytes, ImageFormat originalFormat = ImageFormat.Png)
		{
			_bytes = bytes;
			_originalFormat = originalFormat;

			if (originalFormat == ImageFormat.Jpeg)
			{
				GetJpegDimension(out _width, out _height);
			}
			else
			{
				GetPngDimension(out _width, out _height);
			}
		}

		/// <summary>
		/// Gets the raw image data.
		/// </summary>
		/// <summary>
		/// Gets the raw image data.
		/// </summary>
		public byte[] Bytes => _bytes;

		/// <summary>
		/// Downsizes the image to fit within the specified maximum dimension while maintaining aspect ratio.
		/// </summary>
		/// <param name="maxWidthOrHeight">The maximum width or height of the resulting image.</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after downsizing.</param>
		/// <returns>A new <see cref="IImage"/> with the downsized dimensions.</returns>
		/// <exception cref="PlatformNotSupportedException">This functionality is not supported in this platform implementation.</exception>
		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			throw new PlatformNotSupportedException();
		}

		/// <summary>
		/// Downsizes the image to fit within the specified maximum width and height while maintaining aspect ratio.
		/// </summary>
		/// <param name="maxWidth">The maximum width of the resulting image.</param>
		/// <param name="maxHeight">The maximum height of the resulting image.</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after downsizing.</param>
		/// <returns>A new <see cref="IImage"/> with the downsized dimensions.</returns>
		/// <exception cref="PlatformNotSupportedException">This functionality is not supported in this platform implementation.</exception>
		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			throw new PlatformNotSupportedException();
		}

		/// <summary>
		/// Resizes the image to the specified dimensions using the specified resize mode.
		/// </summary>
		/// <param name="width">The width of the resulting image.</param>
		/// <param name="height">The height of the resulting image.</param>
		/// <param name="resizeMode">The mode to use when resizing the image.</param>
		/// <param name="disposeOriginal">Whether to dispose the original image after resizing.</param>
		/// <returns>A new <see cref="IImage"/> with the resized dimensions.</returns>
		/// <exception cref="PlatformNotSupportedException">This functionality is not supported in this platform implementation.</exception>
		public IImage Resize(float width, float height, ResizeMode resizeMode = ResizeMode.Fit, bool disposeOriginal = false)
		{
			throw new PlatformNotSupportedException();
		}

		/// <summary>
		/// Saves the contents of this image to the provided <see cref="Stream"/> object.
		/// </summary>
		/// <param name="stream">The destination stream the bytes of this image will be saved to.</param>
		/// <param name="format">The destination format of the image.</param>
		/// <param name="quality">The destination quality of the image.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="quality"/> is less than 0 or more than 1.</exception>
		/// <exception cref="NotImplementedException">Thrown when the provided value in <paramref name="format"/> does not match with the original format for this image.</exception>
		/// <remarks>The <paramref name="quality"/> value is currently unused.</remarks>
		public void Save(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException(nameof(quality), "quality must be in the range of 0..1");

			if (format == _originalFormat)
			{
				stream.Write(_bytes, 0, _bytes.Length);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <inheritdoc cref="Save"/>
		public Task SaveAsync(Stream stream, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (quality < 0 || quality > 1)
				throw new ArgumentOutOfRangeException(nameof(quality), "quality must be in the range of 0..1");

			if (format == _originalFormat)
			{
				Save(stream, format, quality);
				return Task.FromResult(true);
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Releases all resources used by the image.
		/// </summary>
		public void Dispose()
		{
			// Do nothing
		}

		/// <summary>
		/// Gets the width of the image.
		/// </summary>
		public float Width => _width;

		/// <summary>
		/// Gets the height of the image.
		/// </summary>
		public float Height => _height;

		private void GetPngDimension(out int width, out int height)
		{
			width = 0;
			height = 0;

			// Look into the byte array and get the size of the image
			for (int i = 0; i <= 3; i++)
			{
				width = _bytes[i] | width << 8;
				height = _bytes[i + 4] | height << 8;
			}
		}

		private void GetJpegDimension(out int width, out int height)
		{
			width = 0;
			height = 0;

			bool found = false;
			bool eof = false;

			var stream = new MemoryStream(_bytes);
			var reader = new BinaryReader(stream);

			while (!found || eof)
			{
				// read 0xFF and the type
				reader.ReadByte();
				byte type = reader.ReadByte();

				// get length
				int len;
				switch (type)
				{
					// start and end of the image
					case 0xD8:
					case 0xD9:
						len = 0;
						break;

					// restart interval
					case 0xDD:
						len = 2;
						break;

					// the next two bytes is the length
					default:
						int lenHi = reader.ReadByte();
						int lenLo = reader.ReadByte();
						len = (lenHi << 8 | lenLo) - 2;
						break;
				}

				// EOF?
				if (type == 0xD9)
					eof = true;

				// process the data
				if (len > 0)
				{
					// read the data
					byte[] data = reader.ReadBytes(len);

					// this is what we are looking for
					if (type == 0xC0)
					{
						width = data[1] << 8 | data[2];
						height = data[3] << 8 | data[4];
						found = true;
					}
				}
			}

			reader.Dispose();
			stream.Dispose();
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			throw new PlatformNotSupportedException();
		}

		public static IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (stream)
				{
					stream.CopyTo(memoryStream);
				}

				return new PlatformImage(memoryStream.ToArray(), format);
			}
		}

		public IImage ToPlatformImage()
			=> this;
	}
#endif
}
