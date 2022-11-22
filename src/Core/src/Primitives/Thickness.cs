#nullable enable
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="Type[@FullName='Microsoft.Maui.Thickness']/Docs/*" />
	[DebuggerDisplay("Left={Left}, Top={Top}, Right={Right}, Bottom={Bottom}, HorizontalThickness={HorizontalThickness}, VerticalThickness={VerticalThickness}")]
	[TypeConverter(typeof(Converters.ThicknessTypeConverter))]
	public struct Thickness
	{
		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='Left']/Docs/*" />
		public double Left { get; set; }

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='Top']/Docs/*" />
		public double Top { get; set; }

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='Right']/Docs/*" />
		public double Right { get; set; }

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='Bottom']/Docs/*" />
		public double Bottom { get; set; }

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='HorizontalThickness']/Docs/*" />
		public double HorizontalThickness => Left + Right;

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='VerticalThickness']/Docs/*" />
		public double VerticalThickness => Top + Bottom;

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='IsEmpty']/Docs/*" />
		public bool IsEmpty => Left == 0 && Top == 0 && Right == 0 && Bottom == 0;

		public bool IsNaN => double.IsNaN(Left) && double.IsNaN(Top) && double.IsNaN(Right) && double.IsNaN(Bottom);

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Thickness(double uniformSize) : this(uniformSize, uniformSize, uniformSize, uniformSize)
		{
		}

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public Thickness(double horizontalSize, double verticalSize) : this(horizontalSize, verticalSize, horizontalSize, verticalSize)
		{
		}

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='Equals']/Docs/*" />
		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is Thickness && Equals((Thickness)obj);
		}

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui/Thickness.xml" path="//Member[@MemberName='Deconstruct']/Docs/*" />
		public void Deconstruct(out double left, out double top, out double right, out double bottom)
		{
			left = Left;
			top = Top;
			right = Right;
			bottom = Bottom;
		}

		public static Thickness Zero = new Thickness(0);

		public static Thickness operator +(Thickness left, double addend) =>
			new Thickness(left.Left + addend, left.Top + addend, left.Right + addend, left.Bottom + addend);

		public static Thickness operator +(Thickness left, Thickness right) =>
			new Thickness(left.Left + right.Left, left.Top + right.Top, left.Right + right.Right, left.Bottom + right.Bottom);

		public static Thickness operator -(Thickness left, double addend) =>
			left + (-addend);
	}
}
