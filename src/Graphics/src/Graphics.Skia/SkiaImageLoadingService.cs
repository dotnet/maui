using System.IO;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides image loading functionality using SkiaSharp.
	/// </summary>
	public class SkiaImageLoadingService : IImageLoadingService
	{
		/// <summary>
		/// Creates a new image from a stream.
		/// </summary>
		/// <param name="stream">The stream containing the image data.</param>
		/// <param name="formatHint">Optional hint about the image format.</param>
		/// <returns>A new <see cref="IImage"/> instance containing the image from the stream.</returns>
		public IImage FromStream(Stream stream, ImageFormat formatHint = ImageFormat.Png)
		{
			return SkiaImage.FromStream(stream, formatHint);
		}
	}
}
