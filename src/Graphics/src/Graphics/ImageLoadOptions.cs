#nullable enable

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Options that control how an image is loaded via
	/// <see cref="IImageLoadingService.FromStream(System.IO.Stream, ImageLoadOptions)"/>.
	/// </summary>
	public readonly record struct ImageLoadOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ImageLoadOptions"/> struct.
		/// </summary>
		/// <param name="disableRotationNormalization">
		/// When <see langword="true"/>, the loader does not automatically apply the source image's EXIF
		/// orientation; the pixels are returned exactly as decoded. When <see langword="false"/> (the
		/// default), the orientation is normalized so the returned image is upright.
		/// </param>
		/// <param name="preserveMetadata">
		/// When <see langword="true"/>, image metadata (such as EXIF) is captured during load and made
		/// available through <see cref="IImage.Metadata"/>. When <see langword="false"/> (the default),
		/// metadata is not preserved.
		/// </param>
		public ImageLoadOptions(bool disableRotationNormalization = false, bool preserveMetadata = false)
		{
			DisableRotationNormalization = disableRotationNormalization;
			PreserveMetadata = preserveMetadata;
		}

		/// <summary>
		/// When <see langword="true"/>, the loader does not automatically apply the source image's
		/// EXIF orientation; the pixels are returned exactly as they were decoded. When
		/// <see langword="false"/> (the default), the orientation is normalized so the returned image
		/// is upright, matching the behavior of <see cref="IImageLoadingService.FromStream(System.IO.Stream, ImageFormat)"/>.
		/// </summary>
		public bool DisableRotationNormalization { get; }

		/// <summary>
		/// When <see langword="true"/>, image metadata (such as EXIF) is captured during load and made
		/// available through <see cref="IImage.Metadata"/> so it can be re-embedded when the
		/// image is saved. When <see langword="false"/> (the default), metadata is not preserved.
		/// </summary>
		public bool PreserveMetadata { get; }
	}
}
