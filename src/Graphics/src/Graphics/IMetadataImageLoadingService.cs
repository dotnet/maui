#nullable enable

using System.IO;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// An <see cref="IImageLoadingService"/> that supports loading images with <see cref="ImageLoadOptions"/>,
	/// including control over EXIF orientation normalization and metadata preservation.
	/// </summary>
	public interface IMetadataImageLoadingService : IImageLoadingService
	{
		/// <summary>
		/// Creates an image from the specified stream using the supplied options.
		/// </summary>
		/// <param name="stream">The stream containing the image data.</param>
		/// <param name="options">The options controlling orientation normalization and metadata capture.</param>
		/// <returns>
		/// An <see cref="IImage"/> created from the stream. When
		/// <see cref="ImageLoadOptions.PreserveMetadata"/> is enabled the returned image also implements
		/// <see cref="IImageWithMetadata"/>.
		/// </returns>
		IImage FromStream(Stream stream, ImageLoadOptions options);
	}
}
