namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines attributes for styling and positioning text in a graphics context.
	/// </summary>
	public interface ITextAttributes
	{
		/// <summary>
		/// Gets or sets the font used for rendering text.
		/// </summary>
		IFont Font { get; set; }

		/// <summary>
		/// Gets or sets the size of the font in points.
		/// </summary>
		float FontSize { get; set; }

		/// <summary>
		/// Gets or sets the margin around the text.
		/// </summary>
		float Margin { get; set; }

		/// <summary>
		/// Gets or sets the color of the text.
		/// </summary>
		Color TextFontColor { get; set; }

		/// <summary>
		/// Gets or sets the horizontal alignment of the text.
		/// </summary>
		HorizontalAlignment HorizontalAlignment { get; set; }

		/// <summary>
		/// Gets or sets the vertical alignment of the text.
		/// </summary>
		VerticalAlignment VerticalAlignment { get; set; }
	}
}
