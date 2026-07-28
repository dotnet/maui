#nullable enable

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Options that control how an image is loaded via
	/// <see cref="IImageLoadingService.FromStream(System.IO.Stream, ImageLoadOptions)"/>.
	/// </summary>
	public class ImageLoadOptions
	{
		/// <summary>
		/// Gets or sets a value indicating whether EXIF orientation normalization is disabled.
		/// When <see langword="true"/>, the loader does not automatically apply the source image's EXIF
		/// orientation; the pixels are returned exactly as they were decoded. When <see langword="false"/>
		/// (the default), the orientation is normalized so the returned image is upright, matching the
		/// behavior of <see cref="IImageLoadingService.FromStream(System.IO.Stream, ImageFormat)"/>.
		/// </summary>
		public bool DisableRotationNormalization { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether image metadata (such as EXIF) is captured during load.
		/// When <see langword="true"/>, the metadata is made available through <see cref="IImage.Metadata"/>
		/// so it can be re-embedded when the image is saved. When <see langword="false"/> (the default),
		/// metadata is not preserved.
		/// </summary>
		public bool PreserveMetadata { get; set; }
	}
}
