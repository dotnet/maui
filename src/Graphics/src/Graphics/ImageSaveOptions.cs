#nullable enable

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Options that control how an image is saved via
	/// <see cref="IImageWithMetadata.SaveAsync(System.IO.Stream, ImageFormat, ImageSaveOptions)"/>.
	/// </summary>
	public readonly record struct ImageSaveOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ImageSaveOptions"/> struct with the default
		/// quality of <c>1.0</c>. Note that <c>default(ImageSaveOptions)</c> bypasses this constructor
		/// and therefore has a <see cref="Quality"/> of <c>0.0</c>.
		/// </summary>
		public ImageSaveOptions()
		{
		}

		/// <summary>
		/// Gets the quality level (from <c>0.0</c> to <c>1.0</c>) used for lossy formats such as JPEG.
		/// Defaults to <c>1.0</c> when created with <c>new ImageSaveOptions()</c>.
		/// </summary>
		public float Quality { get; init; } = 1f;

		/// <summary>
		/// When <see langword="true"/>, metadata that was captured at load time (see
		/// <see cref="ImageLoadOptions.PreserveMetadata"/>) is re-embedded into the saved image. When
		/// <see langword="false"/> (the default), metadata is not written.
		/// </summary>
		public bool PreserveMetadata { get; init; }
	}
}
