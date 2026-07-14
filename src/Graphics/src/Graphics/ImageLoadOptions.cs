#nullable enable

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Options that control how an image is loaded via
	/// <see cref="IMetadataImageLoadingService.FromStream(System.IO.Stream, ImageLoadOptions)"/>.
	/// </summary>
	public readonly record struct ImageLoadOptions
	{
		/// <summary>
		/// When <see langword="true"/>, the loader does not automatically apply the source image's
		/// EXIF orientation; the pixels are returned exactly as they were decoded. When
		/// <see langword="false"/> (the default), the orientation is normalized so the returned image
		/// is upright, matching the behavior of <see cref="IImageLoadingService.FromStream"/>.
		/// </summary>
		public bool DisableRotationNormalization { get; init; }

		/// <summary>
		/// When <see langword="true"/>, image metadata (such as EXIF) is captured during load and made
		/// available through <see cref="IImageWithMetadata.Metadata"/> so it can be re-embedded when the
		/// image is saved. When <see langword="false"/> (the default), metadata is not preserved.
		/// </summary>
		public bool PreserveMetadata { get; init; }
	}
}
