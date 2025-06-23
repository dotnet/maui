namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a paint that uses an image to fill shapes.
	/// </summary>
	public class ImagePaint : Paint
	{
		/// <summary>
		/// Gets or sets the image used for painting.
		/// </summary>
		public IImage Image { get; set; }

		/// <summary>
		/// Gets a value indicating whether this paint has transparent areas.
		/// </summary>
		/// <remarks>
		/// Always returns false as the transparency depends on the actual image content.
		/// </remarks>
		public override bool IsTransparent => false;
	}
}