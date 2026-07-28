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
		/// JPEG. Defaults to <c>1.0</c>.
		/// </summary>
		public float Quality { get; set; } = 1f;

		/// <summary>
		/// Gets or sets a value indicating whether metadata captured at load time (see
		/// <see cref="ImageLoadOptions.PreserveMetadata"/>) is re-embedded into the saved image.
		/// When <see langword="false"/> (the default), metadata is not written.
		/// </summary>
		public bool PreserveMetadata { get; set; }
	}
}
