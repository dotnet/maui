using System;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines constant values for standard font weights.
	/// </summary>
	public static class FontWeights
	{
		/// <summary>
		/// Represents the default font weight.
		/// </summary>
		public const int Default = -1;

		/// <summary>
		/// Represents a thin font weight (100).
		/// </summary>
		public const int Thin = 100;

		/// <summary>
		/// Represents an extra light font weight (200).
		/// </summary>
		public const int ExtraLight = 200;

		/// <summary>
		/// Represents an ultra light font weight (200), equivalent to ExtraLight.
		/// </summary>
		public const int UltraLight = 200;

		/// <summary>
		/// Represents a light font weight (300).
		/// </summary>
		public const int Light = 300;

		/// <summary>
		/// Represents a semi-light font weight (400), equivalent to Normal.
		/// </summary>
		public const int SemiLight = 400;

		/// <summary>
		/// Represents a normal font weight (400).
		/// </summary>
		public const int Normal = 400;

		/// <summary>
		/// Represents a regular font weight (400), equivalent to Normal.
		/// </summary>
		public const int Regular = 400;

		/// <summary>
		/// Represents a medium font weight (500).
		/// </summary>
		public const int Medium = 500;

		/// <summary>
		/// Represents a demi-bold font weight (600), equivalent to SemiBold.
		/// </summary>
		public const int DemiBold = 600;

		/// <summary>
		/// Represents a semi-bold font weight (600).
		/// </summary>
		public const int SemiBold = 600;

		/// <summary>
		/// Represents a bold font weight (700).
		/// </summary>
		public const int Bold = 700;

		/// <summary>
		/// Represents an extra bold font weight (800).
		/// </summary>
		public const int ExtraBold = 800;

		/// <summary>
		/// Represents an ultra bold font weight (800), equivalent to ExtraBold.
		/// </summary>
		public const int UltraBold = 800;

		/// <summary>
		/// Represents a black font weight (900).
		/// </summary>
		public const int Black = 900;

		/// <summary>
		/// Represents a heavy font weight (900), equivalent to Black.
		/// </summary>
		public const int Heavy = 900;

		/// <summary>
		/// Represents an extra black font weight (950).
		/// </summary>
		public const int ExtraBlack = 950;

		/// <summary>
		/// Represents an ultra black font weight (950), equivalent to ExtraBlack.
		/// </summary>
		public const int UltraBlack = 950;
	}

	/// <summary>
	/// Represents a font with a name, weight, and style.
	/// </summary>
	public struct Font : IFont, IEquatable<IFont>
	{
		/// <summary>
		/// Gets the default font.
		/// </summary>
		public static Font Default
			=> new Font(null);

		/// <summary>
		/// Gets the default bold font.
		/// </summary>
		public static Font DefaultBold
			=> new Font(null, FontWeights.Bold);

		/// <summary>
		/// Initializes a new instance of the <see cref="Font"/> struct with the specified name, weight, and style.
		/// </summary>
		/// <param name="name">The font name or family.</param>
		/// <param name="weight">The font weight (default is Normal).</param>
		/// <param name="styleType">The font style type (default is Normal).</param>
		public Font(string name, int weight = FontWeights.Normal, FontStyleType styleType = FontStyleType.Normal)
		{
			Name = name;
			Weight = weight;
			StyleType = styleType;
		}

		/// <summary>
		/// Gets the font name or family.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the font weight.
		/// </summary>
		public int Weight { get; private set; }

		/// <summary>
		/// Gets the font style type.
		/// </summary>
		public FontStyleType StyleType { get; private set; }

		/// <summary>
		/// Determines whether the specified font is equal to the current font.
		/// </summary>
		/// <param name="other">The font to compare with the current font.</param>
		/// <returns><c>true</c> if the specified font is equal to the current font; otherwise, <c>false</c>.</returns>
		public bool Equals(IFont other)
			=>
			StyleType == other.StyleType &&
			Weight == other.Weight &&
			((Name is null && other.Name is null) || Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase));

		/// <summary>
		/// Determines whether the specified object is equal to the current font.
		/// </summary>
		/// <param name="obj">The object to compare with the current font.</param>
		/// <returns><c>true</c> if the specified object is equal to the current font; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
			=> obj is IFont font && Equals(font);

		/// <summary>
		/// Returns a hash code for this font.
		/// </summary>
		/// <returns>A hash code for the current font.</returns>
		public override int GetHashCode()
			=> (Name, Weight, StyleType).GetHashCode();

		/// <summary>
		/// Gets a value indicating whether this font is a default font (has no specified name).
		/// </summary>
		public bool IsDefault
			=> string.IsNullOrEmpty(Name);

		/// <summary>
		/// Determines whether two specified fonts have the same value.
		/// </summary>
		/// <param name="left">The first font to compare.</param>
		/// <param name="right">The second font to compare.</param>
		/// <returns><c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator ==(Font left, Font right) => left.Equals(right);

		/// <summary>
		/// Determines whether two specified fonts have different values.
		/// </summary>
		/// <param name="left">The first font to compare.</param>
		/// <param name="right">The second font to compare.</param>
		/// <returns><c>true</c> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator !=(Font left, Font right) => !(left == right);
	}
}
