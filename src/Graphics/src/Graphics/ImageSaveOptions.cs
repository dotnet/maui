#nullable enable

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Options that control how an image is saved via
	/// <see cref="IImage.SaveAsync(System.IO.Stream, ImageFormat, ImageSaveOptions)"/>.
	/// </summary>
	public class ImageSaveOptions
	{
		/// <summary>
		/// Gets or sets the quality level (from <c>0.0</c> to <c>1.0</c>) used for lossy formats such as
		/// JPEG. Defaults to <c>1.0</c>. Values outside this range are clamped when the image is saved.
		/// </summary>
		public float Quality { get; set; } = 1f;

		/// <summary>
		/// Gets or sets a value indicating whether metadata captured at load time (see
		/// <see cref="ImageLoadOptions.PreserveMetadata"/>) is re-embedded into the saved image.
		/// When <see langword="false"/> (the default), metadata is not written.
		/// </summary>
		/// <remarks>
		/// Metadata support is platform and format dependent. Android and Windows support JPEG;
		/// iOS and Mac Catalyst support JPEG and PNG. Implementations without a metadata backend
		/// save pixel data only.
		/// </remarks>
		public bool PreserveMetadata { get; set; }
	}
}
