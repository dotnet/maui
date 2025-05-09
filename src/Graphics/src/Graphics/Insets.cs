using System;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents inset distances from the four edges of a rectangle.
	/// </summary>
	public class Insets
	{
		private double _bottom;
		private double _left;
		private double _right;
		private double _top;

		/// <summary>
		/// Initializes a new instance of the <see cref="Insets"/> class with the specified values.
		/// </summary>
		/// <param name="top">The top inset value.</param>
		/// <param name="left">The left inset value.</param>
		/// <param name="bottom">The bottom inset value.</param>
		/// <param name="right">The right inset value.</param>
		public Insets(double top, double left, double bottom, double right)
		{
			_top = top;
			_left = left;
			_bottom = bottom;
			_right = right;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Insets"/> class by copying values from another instance.
		/// </summary>
		/// <param name="insets">The insets to copy values from.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="insets"/> is null.</exception>
		public Insets(Insets insets) : this(insets.Top, insets.Left, insets.Bottom, insets.Right)
		{
		}

		/// <summary>
		/// Gets or sets the top inset value.
		/// </summary>
		public double Top
		{
			get => _top;
			set => _top = value;
		}

		/// <summary>
		/// Gets or sets the left inset value.
		/// </summary>
		public double Left
		{
			get => _left;
			set => _left = value;
		}

		/// <summary>
		/// Gets or sets the bottom inset value.
		/// </summary>
		public double Bottom
		{
			get => _bottom;
			set => _bottom = value;
		}

		/// <summary>
		/// Gets or sets the right inset value.
		/// </summary>
		public double Right
		{
			get => _right;
			set => _right = value;
		}

		/// <summary>
		/// The sum of the left and right insets.
		/// </summary>
		public double Horizontal => _left + _right;

		/// <summary>
		/// The sum of the top and bottom insets.
		/// </summary>
		public double Vertical => _top + _bottom;

		/// <summary>
		/// Determines if all inset values are equal to the specified value.
		/// </summary>
		/// <param name="value">The value to compare against.</param>
		/// <returns><c>true</c> if all inset values are equal to the specified value; otherwise, <c>false</c>.</returns>
		public bool AllValuesAreEqualTo(double value)
		{
			return Math.Abs(_top - value) < GeometryUtil.Epsilon && Math.Abs(_left - value) < GeometryUtil.Epsilon && Math.Abs(_right - value) < GeometryUtil.Epsilon &&
				   Math.Abs(_bottom - value) < GeometryUtil.Epsilon;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current insets.
		/// </summary>
		/// <param name="obj">The object to compare with the current insets.</param>
		/// <returns><c>true</c> if the specified object is equal to the current insets; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Insets vCompareTo)
			{
				return Math.Abs(vCompareTo.Top - Top) < GeometryUtil.Epsilon && Math.Abs(vCompareTo.Left - Left) < GeometryUtil.Epsilon && Math.Abs(vCompareTo.Bottom - Bottom) < GeometryUtil.Epsilon &&
					   Math.Abs(vCompareTo.Right - Right) < GeometryUtil.Epsilon;
			}

			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public override int GetHashCode()
		{
			return (int)_top ^ (int)_left + (int)_bottom ^ (int)_right;
		}

		/// <summary>
		/// Converts the insets values to a string representation that can be parsed.
		/// </summary>
		/// <returns>A string with comma-separated inset values in the order: top, left, bottom, right.</returns>
		public string ToParsableString()
		{
			return _top.ToString(CultureInfo.InvariantCulture) + "," + _left.ToString(CultureInfo.InvariantCulture) + "," + _bottom.ToString(CultureInfo.InvariantCulture) + "," +
				   _right.ToString(CultureInfo.InvariantCulture);
		}

		public override string ToString()
		{
			return $"[Insets: Top={_top}, Left={_left}, Bottom={_bottom}, Right={_right}]";
		}

		public static Insets Parse(string value)
		{
			try
			{
				var values = value.Split(',');
				double top = double.Parse(values[0], CultureInfo.InvariantCulture);
				double left = double.Parse(values[1], CultureInfo.InvariantCulture);
				double bottom = double.Parse(values[2], CultureInfo.InvariantCulture);
				double right = double.Parse(values[3], CultureInfo.InvariantCulture);
				return new Insets(top, left, bottom, right);
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc);
				return new Insets(0, 0, 0, 0);
			}
		}
	}
}
