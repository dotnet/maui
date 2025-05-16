namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents an abstract base class for different types of paints that can be used to fill shapes.
	/// </summary>
	public abstract class Paint
	{
		/// <summary>
		/// Gets or sets the background color of the paint.
		/// </summary>
		public Color BackgroundColor { get; set; }

		/// <summary>
		/// Gets or sets the foreground color of the paint.
		/// </summary>
		public Color ForegroundColor { get; set; }

		/// <summary>
		/// Gets a value indicating whether this paint has transparent areas.
		/// </summary>
		/// <remarks>
		/// Derived classes should override this property to correctly indicate their transparency.
		/// </remarks>
		public virtual bool IsTransparent { get; }
	}
}