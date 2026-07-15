using System.IO;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a service for loading images from streams.
	/// </summary>
	public interface IImageLoadingService
	{
		/// <summary>
		/// Creates an image from the specified stream.
		/// </summary>
		/// <param name="stream">The stream containing the image data.</param>
		/// <param name="format">The format of the image data (default is PNG).</param>
		/// <returns>An <see cref="IImage"/> created from the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the image data in the stream is invalid or cannot be decoded.</exception>
		IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png);

		/// <summary>
		/// Creates an image from the specified stream using the supplied options, including control over
		/// EXIF orientation normalization and metadata preservation.
		/// </summary>
		/// <param name="stream">The stream containing the image data.</param>
		/// <param name="options">The options controlling orientation normalization and metadata capture.</param>
		/// <returns>An <see cref="IImage"/> created from the stream.</returns>
#if NETSTANDARD2_0
		IImage FromStream(Stream stream, ImageLoadOptions options);
#else
		IImage FromStream(Stream stream, ImageLoadOptions options)
			=> FromStream(stream);
#endif
	}
}
