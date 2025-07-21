using System;
using System.ComponentModel;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents safe area settings for each edge of a layout or visual element.
	/// </summary>
	[TypeConverter(typeof(Converters.SafeAreaEdgesTypeConverter))]
	public struct SafeAreaEdges : IEquatable<SafeAreaEdges>
	{
		/// <summary>
		/// Gets or sets the safe area behavior for the left edge.
		/// </summary>
		public SafeAreaRegions Left { get; set; }

		/// <summary>
		/// Gets or sets the safe area behavior for the top edge.
		/// </summary>
		public SafeAreaRegions Top { get; set; }

		/// <summary>
		/// Gets or sets the safe area behavior for the right edge.
		/// </summary>
		public SafeAreaRegions Right { get; set; }

		/// <summary>
		/// Gets or sets the safe area behavior for the bottom edge.
		/// </summary>
		public SafeAreaRegions Bottom { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SafeAreaEdges"/> struct with the same value for all edges.
		/// </summary>
		/// <param name="uniformValue">The value to apply to all edges.</param>
		public SafeAreaEdges(SafeAreaRegions uniformValue)
		{
			Left = uniformValue;
			Top = uniformValue;
			Right = uniformValue;
			Bottom = uniformValue;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SafeAreaEdges"/> struct with horizontal and vertical values.
		/// </summary>
		/// <param name="horizontal">The value to apply to left and right edges.</param>
		/// <param name="vertical">The value to apply to top and bottom edges.</param>
		public SafeAreaEdges(SafeAreaRegions horizontal, SafeAreaRegions vertical)
		{
			Left = horizontal;
			Right = horizontal;
			Top = vertical;
			Bottom = vertical;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SafeAreaEdges"/> struct with individual values for each edge.
		/// </summary>
		/// <param name="left">The value for the left edge.</param>
		/// <param name="top">The value for the top edge.</param>
		/// <param name="right">The value for the right edge.</param>
		/// <param name="bottom">The value for the bottom edge.</param>
		public SafeAreaEdges(SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		/// <summary>
		/// Gets the safe area behavior for the specified edge.
		/// </summary>
		/// <param name="edge">The edge index (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaRegions"/> for the specified edge.</returns>
		public SafeAreaRegions GetEdge(int edge)
		{
			return edge switch
			{
				0 => Left,
				1 => Top,
				2 => Right,
				3 => Bottom,
				_ => SafeAreaRegions.None
			};
		}

		/// <summary>
		/// Sets the safe area behavior for the specified edge.
		/// </summary>
		/// <param name="edge">The edge index (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <param name="value">The value to set.</param>
		public void SetEdge(int edge, SafeAreaRegions value)
		{
			switch (edge)
			{
				case 0:
					Left = value;
					break;
				case 1:
					Top = value;
					break;
				case 2:
					Right = value;
					break;
				case 3:
					Bottom = value;
					break;
			}
		}

		/// <summary>
		/// A <see cref="SafeAreaEdges"/> with all edges set to <see cref="SafeAreaRegions.None"/>.
		/// </summary>
		public static SafeAreaEdges Default => new(SafeAreaRegions.None);

		/// <summary>
		/// A <see cref="SafeAreaEdges"/> with all edges set to <see cref="SafeAreaRegions.None"/>.
		/// </summary>
		public static SafeAreaEdges None => new(SafeAreaRegions.None);

		/// <summary>
		/// A <see cref="SafeAreaEdges"/> with all edges set to <see cref="SafeAreaRegions.All"/>.
		/// </summary>
		public static SafeAreaEdges All => new(SafeAreaRegions.All);

		public static bool operator ==(SafeAreaEdges left, SafeAreaEdges right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SafeAreaEdges left, SafeAreaEdges right)
		{
			return !left.Equals(right);
		}

		public bool Equals(SafeAreaEdges other)
		{
			return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
		}

		public override bool Equals(object? obj)
		{
			return obj is SafeAreaEdges other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Left.GetHashCode();
				hashCode = (hashCode * 397) ^ Top.GetHashCode();
				hashCode = (hashCode * 397) ^ Right.GetHashCode();
				hashCode = (hashCode * 397) ^ Bottom.GetHashCode();
				return hashCode;
			}
		}

		public override string ToString()
		{
			if (Left == Top && Top == Right && Right == Bottom)
				return Left.ToString();

			if (Left == Right && Top == Bottom)
				return $"{Left}, {Top}";

			return $"{Left}, {Top}, {Right}, {Bottom}";
		}
	}
}