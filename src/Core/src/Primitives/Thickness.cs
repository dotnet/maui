using System.Diagnostics;

namespace Microsoft.Maui
{
	[DebuggerDisplay("Left={Left}, Top={Top}, Right={Right}, Bottom={Bottom}, HorizontalThickness={HorizontalThickness}, VerticalThickness={VerticalThickness}")]
	public struct Thickness
	{
		public double Left { get; set; }

		public double Top { get; set; }

		public double Right { get; set; }

		public double Bottom { get; set; }

		public double HorizontalThickness
		{
			get { return Left + Right; }
		}

		public double VerticalThickness
		{
			get { return Top + Bottom; }
		}

		public bool IsEmpty
		{
			get { return Left == 0 && Top == 0 && Right == 0 && Bottom == 0; }
		}

		public Thickness(double uniformSize) : this(uniformSize, uniformSize, uniformSize, uniformSize)
		{
		}

		public Thickness(double horizontalSize, double verticalSize) : this(horizontalSize, verticalSize, horizontalSize, verticalSize)
		{
		}

		public Thickness(double left, double top, double right, double bottom) : this()
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public static implicit operator Thickness(Size size)
		{
			return new Thickness(size.Width, size.Height, size.Width, size.Height);
		}

		public static implicit operator Thickness(double uniformSize)
		{
			return new Thickness(uniformSize);
		}

		bool Equals(Thickness other)
		{
			return Left.Equals(other.Left) && Top.Equals(other.Top) && Right.Equals(other.Right) && Bottom.Equals(other.Bottom);
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is Thickness && Equals((Thickness)obj);
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

		public static bool operator ==(Thickness left, Thickness right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Thickness left, Thickness right)
		{
			return !left.Equals(right);
		}

		public void Deconstruct(out double left, out double top, out double right, out double bottom)
		{
			left = Left;
			top = Top;
			right = Right;
			bottom = Bottom;
		}

		public static Thickness Zero = new Thickness(0);
	}
}