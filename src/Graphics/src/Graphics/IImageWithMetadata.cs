#nullable enable

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// An <see cref="IImage"/> that can carry <see cref="IImageMetadata"/> (such as EXIF) and re-embed
	/// it when saving. Images returned from
	/// <see cref="IMetadataImageLoadingService.FromStream(Stream, ImageLoadOptions)"/> implement this
	/// interface, as do the images produced by transforming them (for example
	/// <see cref="IImage.Downsize(float, bool)"/>).
	/// </summary>
	public interface IImageWithMetadata : IImage
	{
		/// <summary>
		/// Gets the metadata captured when the image was loaded, or <see langword="null"/> if the image
		/// was not loaded with <see cref="ImageLoadOptions.PreserveMetadata"/> enabled or has no metadata.
		/// </summary>
		IImageMetadata? Metadata { get; }

		/// <summary>
		/// Saves the image to a stream in the specified format using the supplied options, optionally
		/// re-embedding the captured <see cref="Metadata"/>.
		/// </summary>
		/// <param name="stream">The stream to save the image to.</param>
		/// <param name="format">The format to save the image in.</param>
		/// <param name="options">The options controlling quality and metadata preservation.</param>
		void Save(Stream stream, ImageFormat format, ImageSaveOptions options);

		/// <summary>
		/// Asynchronously saves the image to a stream in the specified format using the supplied options,
		/// optionally re-embedding the captured <see cref="Metadata"/>.
		/// </summary>
		/// <param name="stream">The stream to save the image to.</param>
		/// <param name="format">The format to save the image in.</param>
		/// <param name="options">The options controlling quality and metadata preservation.</param>
		/// <returns>A task representing the asynchronous save operation.</returns>
		Task SaveAsync(Stream stream, ImageFormat format, ImageSaveOptions options);
	}
}
