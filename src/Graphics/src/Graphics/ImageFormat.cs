namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies the format of an image.
	/// </summary>
	public enum ImageFormat
	{
		/// <summary>
		/// Portable Network Graphics format, which supports lossless compression and transparency.
		/// </summary>
		Png,

		/// <summary>
		/// Joint Photographic Experts Group format, which uses lossy compression optimized for photographs.
		/// </summary>
		Jpeg,

		/// <summary>
		/// Graphics Interchange Format, which supports animation and a limited color palette with transparency.
		/// </summary>
		Gif,

		/// <summary>
		/// Tagged Image File Format, which supports multiple images, layers, and various compression methods.
		/// </summary>
		Tiff,

		/// <summary>
		/// Bitmap format, which stores pixel data with minimal or no compression.
		/// </summary>
		Bmp
	}
}
