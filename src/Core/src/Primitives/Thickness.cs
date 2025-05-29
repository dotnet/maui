#nullable enable
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Struct defining thickness for each edge of a rectangle.
	/// </summary>
	[DebuggerDisplay("Left={Left}, Top={Top}, Right={Right}, Bottom={Bottom}, HorizontalThickness={HorizontalThickness}, VerticalThickness={VerticalThickness}")]
	[TypeConverter(typeof(Converters.ThicknessTypeConverter))]
	public struct Thickness
	{
		/// <summary>
		/// Gets or sets the thickness of the left edge.
		/// </summary>
		public double Left { get; set; }

		/// <summary>
		/// Gets or sets the thickness of the top edge.
		/// </summary>
		public double Top { get; set; }

		/// <summary>
		/// Gets or sets the thickness of the right edge.
		/// </summary>
		public double Right { get; set; }

		/// <summary>
		/// Gets or sets the thickness of the bottom edge.
		/// </summary>
		public double Bottom { get; set; }

		/// <summary>
		/// Gets the total horizontal thickness (Left + Right).
		/// </summary>
		public double HorizontalThickness => Left + Right;

		/// <summary>
		/// Gets the total vertical thickness (Top + Bottom).
		/// </summary>
		public double VerticalThickness => Top + Bottom;

		/// <summary>
		/// Gets a value indicating whether all edges have zero thickness.
		/// </summary>
		public bool IsEmpty => Left == 0 && Top == 0 && Right == 0 && Bottom == 0;

		/// <summary>
		/// Gets a value indicating whether any edge thickness is not a number (NaN).
		/// </summary>
		public bool IsNaN => double.IsNaN(Left) && double.IsNaN(Top) && double.IsNaN(Right) && double.IsNaN(Bottom);

		/// <summary>
		/// Creates a new <see cref="Thickness"/> with the same uniform size on all edges.
		/// </summary>
		/// <param name="uniformSize">The uniform thickness for all edges.</param>
		public Thickness(double uniformSize) : this(uniformSize, uniformSize, uniformSize, uniformSize)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Thickness"/> with specified horizontal and vertical sizes.
		/// </summary>
		/// <param name="horizontalSize">Thickness for left and right edges.</param>
		/// <param name="verticalSize">Thickness for top and bottom edges.</param>
		public Thickness(double horizontalSize, double verticalSize) : this(horizontalSize, verticalSize, horizontalSize, verticalSize)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Thickness"/> with individual edge thicknesses.
		/// </summary>
		/// <param name="left">Thickness of the left edge.</param>
		/// <param name="top">Thickness of the top edge.</param>
		/// <param name="right">Thickness of the right edge.</param>
		/// <param name="bottom">Thickness of the bottom edge.</param>
		public Thickness(double left, double top, double right, double bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		/// <summary>
		/// Implicitly converts a <see cref="Microsoft.Maui.Graphics.Size"/> to a uniform <see cref="Thickness"/>.
		/// </summary>
		/// <param name="size">The size whose width and height become edge thicknesses.</param>
		public static implicit operator Thickness(Size size) => new Thickness(size.Width, size.Height, size.Width, size.Height);

		/// <summary>
		/// Implicitly converts a <see cref="double"/> to a uniform <see cref="Thickness"/>.
		/// </summary>
		/// <param name="uniformSize">The uniform thickness value.</param>
		public static implicit operator Thickness(double uniformSize) => new Thickness(uniformSize);

		/// <summary>
		/// Deconstructs the thickness into individual edge values.
		/// </summary>
		/// <param name="left">When this method returns, contains the left thickness value.</param>
		/// <param name="top">When this method returns, contains the top thickness value.</param>
		/// <param name="right">When this method returns, contains the right thickness value.</param>
		/// <param name="bottom">When this method returns, contains the bottom thickness value.</param>
		public void Deconstruct(out double left, out double top, out double right, out double bottom)
		{
			left = Left;
			top = Top;
			right = Right;
			bottom = Bottom;
		}

		/// <summary>
		/// A thickness with all edges set to zero.
		/// </summary>
		public static Thickness Zero = new Thickness(0);

		/// <summary>
		/// Adds a uniform value to all edges of the thickness.
		/// </summary>
		/// <param name="left">The original thickness.</param>
		/// <param name="addend">The value to add to each edge.</param>
		/// <returns>A new thickness with adjusted edges.</returns>
		public static Thickness operator +(Thickness left, double addend) => new Thickness(left.Left + addend, left.Top + addend, left.Right + addend, left.Bottom + addend);

		/// <summary>
		/// Adds two thickness instances by summing their corresponding edges.
		/// </summary>
		/// <param name="left">The first thickness.</param>
		/// <param name="right">The second thickness.</param>
		/// <returns>A new thickness with summed edges.</returns>
		public static Thickness operator +(Thickness left, Thickness right) => new Thickness(left.Left + right.Left, left.Top + right.Top, left.Right + right.Right, left.Bottom + right.Bottom);

		/// <summary>
		/// Subtracts a uniform value from all edges of the thickness.
		/// </summary>
		/// <param name="left">The original thickness.</param>
		/// <param name="addend">The value to subtract from each edge.</param>
		/// <returns>A new thickness with adjusted edges.</returns>
		public static Thickness operator -(Thickness left, double addend) => new Thickness(left.Left - addend, left.Top - addend, left.Right - addend, left.Bottom - addend);

		/// <summary>
		/// Determines whether two <see cref="Thickness"/> instances have the same values.
		/// </summary>
		/// <param name="left">The first thickness to compare.</param>
		/// <param name="right">The second thickness to compare.</param>
		/// <returns><see langword="true"/> if both thicknesses are equal; otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(Thickness left, Thickness right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Determines whether two <see cref="Thickness"/> instances have different values.
		/// </summary>
		/// <param name="left">The first thickness to compare.</param>
		/// <param name="right">The second thickness to compare.</param>
		/// <returns><see langword="true"/> if the thicknesses differ; otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(Thickness left, Thickness right)
		{
			return !left.Equals(right);
		}

		bool Equals(Thickness other)
		{
			return Left.Equals(other.Left) && Top.Equals(other.Top) && Right.Equals(other.Right) && Bottom.Equals(other.Bottom);
		}

		/// <summary>
		/// Determines whether this instance and a specified object have the same values.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Thickness"/> equal to this instance; otherwise, <see langword="false"/>.</returns>
		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;
			return obj is Thickness && Equals((Thickness)obj);
		}

		/// <summary>
		/// Returns the hash code for this <see cref="Thickness"/>.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
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
	}
}
