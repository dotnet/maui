using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics.Platform
{
#if NETSTANDARD || NETSTANDARD2_0 || NETSTANDARD2_1 || (NET6_0_OR_GREATER && !PLATFORM) || TIZEN || TIZEN40
	public class PlatformImage : IImage
	{
		private readonly byte[] _bytes;
		private readonly int _width;
		private readonly int _height;
		private readonly ImageFormat _originalFormat;

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

		public byte[] Bytes => _bytes;

		public IImage Downsize(float maxWidthOrHeight, bool disposeOriginal = false)
		{
			throw new PlatformNotSupportedException();
		}

		public IImage Downsize(float maxWidth, float maxHeight, bool disposeOriginal = false)
		{
			throw new PlatformNotSupportedException();
		}

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

		public void Dispose()
		{
			// Do nothing
		}

		public float Width => _width;

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
