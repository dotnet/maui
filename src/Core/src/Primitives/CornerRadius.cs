#nullable enable
using System.ComponentModel;
using System.Diagnostics;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="Type[@FullName='Microsoft.Maui.CornerRadius']/Docs/*" />
	[DebuggerDisplay("TopLeft={TopLeft}, TopRight={TopRight}, BottomLeft={BottomLeft}, BottomRight={BottomRight}")]
	[TypeConverter(typeof(Converters.CornerRadiusTypeConverter))]
	public struct CornerRadius
	{
		bool _isParameterized;

		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='TopLeft']/Docs/*" />
		public double TopLeft { get; }
		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='TopRight']/Docs/*" />
		public double TopRight { get; }
		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='BottomLeft']/Docs/*" />
		public double BottomLeft { get; }
		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='BottomRight']/Docs/*" />
		public double BottomRight { get; }

		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public CornerRadius(double uniformRadius) : this(uniformRadius, uniformRadius, uniformRadius, uniformRadius)
		{
		}

		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public CornerRadius(double topLeft, double topRight, double bottomLeft, double bottomRight)
		{
			_isParameterized = true;

			TopLeft = topLeft;
			TopRight = topRight;
			BottomLeft = bottomLeft;
			BottomRight = bottomRight;
		}

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

		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='Equals']/Docs/*" />
		public override bool Equals(object? obj)
		{
			if (obj is null)
				return false;

			return obj is CornerRadius cornerRadius && Equals(cornerRadius);
		}

		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
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

		public static bool operator ==(CornerRadius left, CornerRadius right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(CornerRadius left, CornerRadius right)
		{
			return !left.Equals(right);
		}

		/// <include file="../../docs/Microsoft.Maui/CornerRadius.xml" path="//Member[@MemberName='Deconstruct']/Docs/*" />
		public void Deconstruct(out double topLeft, out double topRight, out double bottomLeft, out double bottomRight)
		{
			topLeft = TopLeft;
			topRight = TopRight;
			bottomLeft = BottomLeft;
			bottomRight = BottomRight;
		}
	}
}