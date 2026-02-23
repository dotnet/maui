using System;
using System.ComponentModel;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents safe area settings for each edge of a layout or visual element.
	/// </summary>
	[TypeConverter(typeof(Converters.SafeAreaEdgesTypeConverter))]
	public readonly struct SafeAreaEdges : IEquatable<SafeAreaEdges>
	{
		/// <summary>
		/// Gets the safe area behavior for the left edge.
		/// </summary>
		public SafeAreaRegions Left { get; }

		/// <summary>
		/// Gets the safe area behavior for the top edge.
		/// </summary>
		public SafeAreaRegions Top { get; }

		/// <summary>
		/// Gets the safe area behavior for the right edge.
		/// </summary>
		public SafeAreaRegions Right { get; }

		/// <summary>
		/// Gets the safe area behavior for the bottom edge.
		/// </summary>
		public SafeAreaRegions Bottom { get; }

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

		internal static bool IsSoftInput(SafeAreaRegions region)
		{
			if (region == SafeAreaRegions.Default)
				return false;
			if (region == SafeAreaRegions.All)
				return true;
			return (region & SafeAreaRegions.SoftInput) == SafeAreaRegions.SoftInput;
		}

		internal static bool IsOnlySoftInput(SafeAreaRegions region)
		{
			// Check if the region is ONLY SoftInput, not combined with other flags or All
			return region == SafeAreaRegions.SoftInput;
		}

		internal static bool IsContainer(SafeAreaRegions region)
		{
			if (region == SafeAreaRegions.Default)
				return false;
			if (region == SafeAreaRegions.All)
				return true;
			return (region & SafeAreaRegions.Container) == SafeAreaRegions.Container;
		}

		/// <summary>
		/// Gets the safe area behavior for the specified edge.
		/// </summary>
		/// <param name="edge">The edge index (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaRegions"/> for the specified edge.</returns>
		internal SafeAreaRegions GetEdge(int edge)
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
		/// A <see cref="SafeAreaEdges"/> with all edges set to <see cref="SafeAreaRegions.None"/>.
		/// </summary>
		public static SafeAreaEdges Default { get; } = new(SafeAreaRegions.Default);

		/// <summary>
		/// A <see cref="SafeAreaEdges"/> with all edges set to <see cref="SafeAreaRegions.None"/>.
		/// </summary>
		public static SafeAreaEdges None { get; } = new(SafeAreaRegions.None);

		/// <summary>
		/// A <see cref="SafeAreaEdges"/> with all edges set to <see cref="SafeAreaRegions.All"/>.
		/// </summary>
		public static SafeAreaEdges All { get; } = new(SafeAreaRegions.All);

		/// <summary>
		/// A <see cref="SafeAreaEdges"/> with all edges set to <see cref="SafeAreaRegions.All"/>.
		/// </summary>
		internal static SafeAreaEdges Container { get; } = new(SafeAreaRegions.Container);

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

#if NETSTANDARD2_0
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
#else
		public override int GetHashCode() =>
			HashCode.Combine(Left, Top, Right, Bottom);
#endif
		public override string ToString() =>
			$"{Left}, {Top}, {Right}, {Bottom}";
	}
}