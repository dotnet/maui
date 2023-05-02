using System;
using Android.Util;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents the size of a font on Android.
	/// </summary>
	public readonly struct FontSize : IEquatable<FontSize>
	{
		/// <summary>
		/// Creates a new <see cref="FontSize"/> instance.
		/// </summary>
		/// <param name="value">The font size.</param>
		/// <param name="unit">The unit in which the font size is expressed.</param>
		public FontSize(float value, ComplexUnitType unit)
		{
			Value = value;
			Unit = unit;
		}

		/// <summary>
		/// The font size.
		/// </summary>
		public float Value { get; }

		/// <summary>
		/// The unit in which the font size is expressed.
		/// </summary>
		public ComplexUnitType Unit { get; }

		public bool Equals(FontSize other) => Value == other.Value && Unit == other.Unit;

		public override bool Equals(object? obj) => obj is FontSize other && Equals(other);

		public override int GetHashCode() => Value.GetHashCode() ^ Unit.GetHashCode();

		public static bool operator ==(FontSize left, FontSize right) => left.Equals(right);

		public static bool operator !=(FontSize left, FontSize right) => !(left == right);
	}
}
