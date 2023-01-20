using Android.Util;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents the size of a font on Android.
	/// </summary>
	public readonly struct FontSize
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
	}
}
