using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="IImage"/> interface.
	/// </summary>
	public static class ImageExtensions
	{
		/// <summary>
		/// Converts an image to a byte array in the specified format.
		/// </summary>
		/// <param name="target">The image to convert.</param>
		/// <param name="format">The format to encode the image in (default is PNG).</param>
		/// <param name="quality">The quality setting for lossy formats like JPEG, ranging from 0 to 1 (default is 1 for maximum quality).</param>
		/// <returns>A byte array containing the encoded image data, or null if the target image is null.</returns>
		public static byte[] AsBytes(this IImage target, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (target == null)
				return null;

			using (var stream = new MemoryStream())
			{
				target.Save(stream, format, quality);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Converts an image to a stream in the specified format.
		/// </summary>
		/// <param name="target">The image to convert.</param>
		/// <param name="format">The format to encode the image in (default is PNG).</param>
		/// <param name="quality">The quality setting for lossy formats like JPEG, ranging from 0 to 1 (default is 1 for maximum quality).</param>
		/// <returns>A memory stream containing the encoded image data, or null if the target image is null.</returns>
		public static Stream AsStream(this IImage target, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (target == null)
				return null;

			var stream = new MemoryStream();
			target.Save(stream, format, quality);
			stream.Position = 0;

			return stream;
		}

		public static async Task<byte[]> AsBytesAsync(this IImage target, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (target == null)
				return null;

			using (var stream = new MemoryStream())
			{
				await target.SaveAsync(stream, format, quality);
				return stream.ToArray();
			}
		}

		public static string AsBase64(this IImage target, ImageFormat format = ImageFormat.Png, float quality = 1)
		{
			if (target == null)
				return null;

			var bytes = target.AsBytes(format, quality);
			return Convert.ToBase64String(bytes);
		}

		public static Paint AsPaint(this IImage target)
		{
			if (target == null)
				return null;

			return new ImagePaint { Image = target };
		}

		public static void SetFillImage(this ICanvas canvas, IImage image)
		{
			if (canvas != null)
			{
				var paint = image.AsPaint();
				if (paint != null)
				{
					canvas.SetFillPaint(paint, RectF.Zero);
				}
				else
				{
					canvas.FillColor = Colors.White;
				}
			}
		}
	}
}
