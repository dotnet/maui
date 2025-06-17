namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides a standard implementation of the <see cref="ITextAttributes"/> interface for styling text.
	/// </summary>
	public class StandardTextAttributes : ITextAttributes
	{
		/// <summary>
		/// Gets or sets the font used for rendering text.
		/// </summary>
		public IFont Font { get; set; }

		/// <summary>
		/// Gets or sets the size of the font in points.
		/// </summary>
		public float FontSize { get; set; }

		/// <summary>
		/// Gets or sets the horizontal alignment of the text.
		/// </summary>
		public HorizontalAlignment HorizontalAlignment { get; set; }

		/// <summary>
		/// Gets or sets the margin around the text.
		/// </summary>
		public float Margin { get; set; }

		/// <summary>
		/// Gets or sets the color of the text.
		/// </summary>
		public Color TextFontColor { get; set; }

		/// <summary>
		/// Gets or sets the vertical alignment of the text.
		/// </summary>
		public VerticalAlignment VerticalAlignment { get; set; }
	}
}
