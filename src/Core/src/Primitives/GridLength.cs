#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="Type[@FullName='Microsoft.Maui.GridLength']/Docs/*" />
	[DebuggerDisplay("{Value}.{GridUnitType}")]
	[TypeConverter(typeof(GridLengthTypeConverter))]
	public readonly struct GridLength
	{
		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='Auto']/Docs/*" />
		public static readonly GridLength Auto = new GridLength(1, GridUnitType.Auto);

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='Star']/Docs/*" />
		public static readonly GridLength Star = new GridLength(1, GridUnitType.Star);

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='Value']/Docs/*" />
		public readonly double Value { get; }

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='GridUnitType']/Docs/*" />
		public readonly GridUnitType GridUnitType { get; }

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='IsAbsolute']/Docs/*" />
		public bool IsAbsolute
		{
			get { return GridUnitType == GridUnitType.Absolute; }
		}

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='IsAuto']/Docs/*" />
		public bool IsAuto
		{
			get { return GridUnitType == GridUnitType.Auto; }
		}

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='IsStar']/Docs/*" />
		public bool IsStar
		{
			get { return GridUnitType == GridUnitType.Star; }
		}

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public GridLength(double value) : this(value, GridUnitType.Absolute)
		{
		}

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public GridLength(double value, GridUnitType type)
		{
			if (value < 0 || double.IsNaN(value))
				throw new ArgumentException("value is less than 0 or is not a number", "value");
			if ((int)type < (int)GridUnitType.Absolute || (int)type > (int)GridUnitType.Auto)
				throw new ArgumentException("type is not a valid GridUnitType", "type");

			Value = value;
			GridUnitType = type;
		}

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='Equals']/Docs/*" />
		public override bool Equals(object? obj)
		{
			return obj is GridLength && Equals((GridLength)obj);
		}

		bool Equals(GridLength other)
		{
			return GridUnitType == other.GridUnitType && Math.Abs(Value - other.Value) < double.Epsilon;
		}

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
		public override int GetHashCode()
		{
			return GridUnitType.GetHashCode() * 397 ^ Value.GetHashCode();
		}

		public static implicit operator GridLength(double absoluteValue)
		{
			return new GridLength(absoluteValue);
		}

		/// <include file="../../docs/Microsoft.Maui/GridLength.xml" path="//Member[@MemberName='ToString']/Docs/*" />
		public override string ToString()
		{
			return string.Format("{0}.{1}", Value, GridUnitType);
		}

		public static bool operator ==(GridLength left, GridLength right) => left.Equals(right);

		public static bool operator !=(GridLength left, GridLength right) => !(left == right);

		private sealed class GridLengthTypeConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
				=> sourceType == typeof(double);

			public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
				=> value switch
				{
					double d => (GridLength)d,
					_ => throw new NotSupportedException(),
				};

			public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => false;
			public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType) => throw new NotSupportedException();
		}
	}
}