#nullable enable
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui.Converters;

namespace Microsoft.Maui
{
	/// <summary>
	/// Used to define the size (width/height) of Grid ColumnDefinition and RowDefinition.
	/// </summary>
	/// <remarks>
	/// GridLength of type <see cref="GridUnitType.Absolute"/> represents exact size.
	/// GridLength of type <see cref="GridUnitType.Auto"/> adapts to fit content size.
	/// GridLength of type <see cref="GridUnitType.Star"/> distributes remaining space proportionally.
	/// This value type is readonly.
	/// </remarks>
	[DebuggerDisplay("{Value}.{GridUnitType}")]
	[TypeConverter(typeof(GridLengthTypeConverter))]
	public readonly struct GridLength
	{
		/// <summary>A ready-to-use GridLength of <see cref="GridUnitType.Auto"/>. Value is ignored.</summary>
		public static readonly GridLength Auto = new GridLength(1, GridUnitType.Auto);

		/// <summary>A ready-to-use GridLength of <see cref="GridUnitType.Star"/>. Distributes available space proportionally.</summary>
		public static readonly GridLength Star = new GridLength(1, GridUnitType.Star);

		/// <summary>Gets the numeric value of the GridLength. Represents an absolute size or weight; ignored for Auto.</summary>
		public readonly double Value { get; }

		/// <summary>Gets the unit type that indicates how the GridLength is interpreted.</summary>
		public readonly GridUnitType GridUnitType { get; }

		/// <summary>Gets a value indicating whether this GridLength uses Absolute units.</summary>
		public bool IsAbsolute
		{
			get { return GridUnitType == GridUnitType.Absolute; }
		}

		/// <summary>Gets a value indicating whether this GridLength uses Auto sizing.</summary>
		public bool IsAuto
		{
			get { return GridUnitType == GridUnitType.Auto; }
		}

		/// <summary>Gets a value indicating whether this GridLength uses Star (proportional) sizing.</summary>
		public bool IsStar
		{
			get { return GridUnitType == GridUnitType.Star; }
		}

		/// <summary>Initializes a new <see cref="GridLength"/> instance that represents an absolute length.</summary>
		/// <param name="value">The absolute size.</param>
		/// <remarks>Equivalent to new GridLength(value, GridUnitType.Absolute).</remarks>
		/// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is less than zero or not a number.</exception>
		public GridLength(double value) : this(value, GridUnitType.Absolute)
		{
		}

		/// <summary>Initializes a new <see cref="GridLength"/> instance with the specified value and unit type.</summary>
		/// <param name="value">The size or weight.</param>
		/// <param name="type">The unit type (Absolute, Star, or Auto).</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is less than zero or not a number.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is not a valid <see cref="GridUnitType"/>.</exception>
		public GridLength(double value, GridUnitType type)
		{
			if (value < 0 || double.IsNaN(value))
				throw new ArgumentException("value is less than 0 or is not a number", nameof(value));
			if ((int)type < (int)GridUnitType.Absolute || (int)type > (int)GridUnitType.Auto)
				throw new ArgumentException("type is not a valid GridUnitType", nameof(type));

			Value = value;
			GridUnitType = type;
		}

		/// <summary>Determines whether the specified object is equal to the current GridLength.</summary>
		/// <param name="obj">The object to compare with this GridLength.</param>
		/// <returns><see langword="true"/> if the specified object is a GridLength equal to this instance; otherwise, <see langword="false"/>.</returns>
		public override bool Equals(object? obj)
		{
			return obj is GridLength other && Equals(other);
		}

		bool Equals(GridLength other)
		{
			return GridUnitType == other.GridUnitType && Math.Abs(Value - other.Value) < double.Epsilon;
		}

		/// <summary>Returns the hash code for this GridLength.</summary>
		/// <returns>A hash code for the current GridLength.</returns>
		public override int GetHashCode()
		{
			return GridUnitType.GetHashCode() * 397 ^ Value.GetHashCode();
		}

		/// <summary>Converts a double to a GridLength using Absolute units.</summary>
		/// <param name="absoluteValue">The absolute size.</param>
		/// <returns>A GridLength instance representing an absolute length.</returns>
		public static implicit operator GridLength(double absoluteValue)
		{
			return new GridLength(absoluteValue);
		}

		/// <summary>Converts a string to a GridLength using the type converter.</summary>
		/// <param name="value">The string value representing a GridLength ("auto", "*", "2*", or a number).</param>
		/// <returns>A GridLength instance parsed from the string.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
		/// <exception cref="FormatException">Thrown if <paramref name="value"/> is not a valid GridLength format.</exception>
		public static implicit operator GridLength(string value)
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));
			
			return Converters.GridLengthTypeConverter.ParseStringToGridLength(value);
		}

		/// <summary>Returns a string that represents this GridLength.</summary>
		/// <returns>A string representation in the format "{Value}.{GridUnitType}".</returns>
		public override string ToString()
		{
			return string.Format("{0}.{1}", Value, GridUnitType);
		}

		/// <summary>Indicates whether two GridLength instances are equal.</summary>
		/// <param name="left">The first GridLength to compare.</param>
		/// <param name="right">The second GridLength to compare.</param>
		/// <returns><see langword="true"/> if the two GridLengths are equal; otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(GridLength left, GridLength right) => left.Equals(right);

		/// <summary>Indicates whether two GridLength instances are not equal.</summary>
		/// <param name="left">The first GridLength to compare.</param>
		/// <param name="right">The second GridLength to compare.</param>
		/// <returns><see langword="true"/> if the two GridLengths differ; otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(GridLength left, GridLength right) => !(left == right);
	}
}