#nullable enable
using System;
using System.Diagnostics;

namespace Microsoft.Maui
{
	[DebuggerDisplay("{Value}.{GridUnitType}")]
	public struct GridLength
	{
		public static GridLength Auto
		{
			get { return new GridLength(1, GridUnitType.Auto); }
		}

		public static GridLength Star
		{
			get { return new GridLength(1, GridUnitType.Star); }
		}

		public double Value { get; }

		public GridUnitType GridUnitType { get; }

		public bool IsAbsolute
		{
			get { return GridUnitType == GridUnitType.Absolute; }
		}

		public bool IsAuto
		{
			get { return GridUnitType == GridUnitType.Auto; }
		}

		public bool IsStar
		{
			get { return GridUnitType == GridUnitType.Star; }
		}

		public GridLength(double value) : this(value, GridUnitType.Absolute)
		{
		}

		public GridLength(double value, GridUnitType type)
		{
			if (value < 0 || double.IsNaN(value))
				throw new ArgumentException("value is less than 0 or is not a number", "value");
			if ((int)type < (int)GridUnitType.Absolute || (int)type > (int)GridUnitType.Auto)
				throw new ArgumentException("type is not a valid GridUnitType", "type");

			Value = value;
			GridUnitType = type;
		}

		public override bool Equals(object? obj)
		{
			return obj is GridLength && Equals((GridLength)obj);
		}

		bool Equals(GridLength other)
		{
			return GridUnitType == other.GridUnitType && Math.Abs(Value - other.Value) < double.Epsilon;
		}

		public override int GetHashCode()
		{
			return GridUnitType.GetHashCode() * 397 ^ Value.GetHashCode();
		}

		public static implicit operator GridLength(double absoluteValue)
		{
			return new GridLength(absoluteValue);
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}", Value, GridUnitType);
		}
	}
}