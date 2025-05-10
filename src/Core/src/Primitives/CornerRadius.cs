#nullable enable
using System.ComponentModel;
using System.Diagnostics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Contains methods and properties for specifying corner radiuses.
	/// </summary>
	[DebuggerDisplay("TopLeft={TopLeft}, TopRight={TopRight}, BottomLeft={BottomLeft}, BottomRight={BottomRight}")]
	[TypeConverter(typeof(Converters.CornerRadiusTypeConverter))]
	public struct CornerRadius
	{
		bool _isParameterized;

		/// <summary>
		/// Gets the radius of the top left corner.
		/// </summary>
		/// <value>The radius of the top left corner.</value>
		public double TopLeft { get; }

		/// <summary>
		/// Gets the radius of the top right corner.
		/// </summary>
		/// <value>The radius of the top right corner.</value>
		public double TopRight { get; }

		/// <summary>
		/// Gets the radius of the bottom left corner.
		/// </summary>
		/// <value>The radius of the bottom left corner.</value>
		public double BottomLeft { get; }

		/// <summary>
		/// Gets the radius of the bottom right corner.
		/// </summary>
		/// <value>The radius of the bottom right corner.</value>
		public double BottomRight { get; }

		/// <summary>
		/// Creates a new <see cref="CornerRadius"/> such that all four of its corners have the same radius.
		/// </summary>
		/// <param name="uniformRadius">The radius for all four corners.</param>
		public CornerRadius(double uniformRadius) : this(uniformRadius, uniformRadius, uniformRadius, uniformRadius)
		{
		}

		/// <summary>
		/// Creates a new <see cref="CornerRadius"/> such that each of its corners have the specified radiuses.
		/// </summary>
		/// <param name="topLeft">The radius of the top left corner.</param>
		/// <param name="topRight">The radius of the top right corner.</param>
		/// <param name="bottomLeft">The radius of the bottom left corner.</param>
		/// <param name="bottomRight">The radius of the bottom right corner.</param>
		public CornerRadius(double topLeft, double topRight, double bottomLeft, double bottomRight)
		{
			_isParameterized = true;

			TopLeft = topLeft;
			TopRight = topRight;
			BottomLeft = bottomLeft;
			BottomRight = bottomRight;
		}

		/// <summary>
		/// Converts a uniform radius value to a <see cref="CornerRadius"/>.
		/// </summary>
		/// <param name="uniformRadius">The uniform radius to apply to all corners.</param>
		/// <returns>A <see cref="CornerRadius"/> with all corners set to <paramref name="uniformRadius"/>.</returns>
		public static implicit operator CornerRadius(double uniformRadius)
		{
			return new CornerRadius(uniformRadius);
		}

		bool Equals(CornerRadius other)
		{
			if (!_isParameterized && !other._isParameterized)
				return true;

			return TopLeft == other.TopLeft && TopRight == other.TopRight && BottomLeft == other.BottomLeft && BottomRight == other.BottomRight;
		}

		/// <summary>
		/// Compares this corner radius to another object.
		/// </summary>
		/// <param name="obj">The object against which to compare.</param>
		/// <returns><see langword="true"/> if <paramref name="obj"/> has the same effective corner radius values; otherwise, <see langword="false"/>.</returns>
		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;

			return obj is CornerRadius cornerRadius && Equals(cornerRadius);
		}

		/// <summary>
		/// Gets the hashcode for the corner radius.
		/// </summary>
		/// <returns>The hashcode for the corner radius.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = TopLeft.GetHashCode();
				hashCode = (hashCode * 397) ^ TopRight.GetHashCode();
				hashCode = (hashCode * 397) ^ BottomLeft.GetHashCode();
				hashCode = (hashCode * 397) ^ BottomRight.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Compares two <see cref="CornerRadius"/> values for equality.
		/// </summary>
		/// <param name="left">The first corner radius to compare.</param>
		/// <param name="right">The second corner radius to compare.</param>
		/// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have the same corner values; otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(CornerRadius left, CornerRadius right) => left.Equals(right);

		/// <summary>
		/// Compares two <see cref="CornerRadius"/> values for inequality.
		/// </summary>
		/// <param name="left">The first corner radius to compare.</param>
		/// <param name="right">The second corner radius to compare.</param>
		/// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have different corner values; otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(CornerRadius left, CornerRadius right) => !left.Equals(right);

		/// <summary>
		/// Deconstructs the <see cref="CornerRadius"/> into its component corner values.
		/// </summary>
		/// <param name="topLeft">The radius of the top left corner.</param>
		/// <param name="topRight">The radius of the top right corner.</param>
		/// <param name="bottomLeft">The radius of the bottom left corner.</param>
		/// <param name="bottomRight">The radius of the bottom right corner.</param>
		public void Deconstruct(out double topLeft, out double topRight, out double bottomLeft, out double bottomRight)
		{
			topLeft = TopLeft;
			topRight = TopRight;
			bottomLeft = BottomLeft;
			bottomRight = BottomRight;
		}
	}
}