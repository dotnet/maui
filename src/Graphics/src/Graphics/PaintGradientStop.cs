using System;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a color stop in a gradient paint.
	/// </summary>
	public class PaintGradientStop : IComparable<PaintGradientStop>
	{
		private Color _color;
		private float _offset;

		/// <summary>
		/// Initializes a new instance of the <see cref="PaintGradientStop"/> class with the specified offset and color.
		/// </summary>
		/// <param name="offset">The position of the color stop along the gradient, typically between 0.0 and 1.0.</param>
		/// <param name="color">The color at this stop position.</param>
		public PaintGradientStop(float offset, Color color)
		{
			_color = color;
			_offset = offset;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PaintGradientStop"/> class by copying another gradient stop.
		/// </summary>
		/// <param name="source">The gradient stop to copy.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
		public PaintGradientStop(PaintGradientStop source)
		{
			_color = source._color;
			_offset = source._offset;
		}

		/// <summary>
		/// Gets or sets the color at this gradient stop.
		/// </summary>
		public Color Color
		{
			get => _color;
			set => _color = value;
		}

		/// <summary>
		/// Gets or sets the position of this gradient stop along the gradient.
		/// </summary>
		/// <remarks>
		/// Typically between 0.0 (start of gradient) and 1.0 (end of gradient).
		/// </remarks>
		public float Offset
		{
			get => _offset;
			set => _offset = value;
		}

		/// <summary>
		/// Compares the current gradient stop with another gradient stop based on their offset values.
		/// </summary>
		/// <param name="obj">The gradient stop to compare with this instance.</param>
		/// <returns>
		/// A value less than zero if this instance's offset is less than the other instance's offset;
		/// zero if they are equal; a value greater than zero if this instance's offset is greater.
		/// </returns>
		public int CompareTo(PaintGradientStop obj)
		{
			if (_offset < obj._offset)
				return -1;
			if (_offset > obj._offset)
				return 1;

			return 0;
		}
	}
}
