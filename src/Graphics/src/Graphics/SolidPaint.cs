namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a paint that fills shapes with a solid color.
	/// </summary>
	public class SolidPaint : Paint
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SolidPaint"/> class with default values.
		/// </summary>
		public SolidPaint()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SolidPaint"/> class with the specified color.
		/// </summary>
		/// <param name="color">The color to fill shapes with.</param>
		public SolidPaint(Color color)
		{
			Color = color;
		}

		/// <summary>
		/// Gets or sets the color used to fill shapes.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Gets a value indicating whether this paint has transparent areas.
		/// </summary>
		/// <remarks>
		/// A solid paint is considered transparent if its color's alpha value is less than 1.
		/// </remarks>
		public override bool IsTransparent
		{
			get
			{
				return Color.Alpha < 1;
			}
		}

		public override string ToString()
		{
			return $"[{nameof(SolidPaint)}: Color={Color}]";
		}
	}
}