#nullable enable

using System;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a font source with name, weight, and style information.
	/// </summary>
	public struct FontSource : IEquatable<FontSource>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FontSource"/> struct.
		/// </summary>
		/// <param name="filename">The font file name or font family name.</param>
		/// <param name="weight">The font weight (default is Normal).</param>
		/// <param name="fontStyleType">The font style type (default is Normal).</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="filename"/> is null.</exception>
		public FontSource(string filename, int weight = FontWeights.Normal, FontStyleType fontStyleType = FontStyleType.Normal)
		{
			Name = filename;
			Weight = weight;
			FontStyleType = fontStyleType;
		}

		/// <summary>
		/// Gets the font file name or font family name.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// Gets the font weight.
		/// </summary>
		public readonly int Weight;

		/// <summary>
		/// Gets the font style type.
		/// </summary>
		public readonly FontStyleType FontStyleType;

		/// <summary>
		/// Determines whether the specified font source is equal to the current font source.
		/// </summary>
		/// <param name="other">The font source to compare with the current font source.</param>
		/// <returns><c>true</c> if the specified font source is equal to the current font source; otherwise, <c>false</c>.</returns>
		public bool Equals(FontSource other)
			=> Name.Equals(other.Name, StringComparison.Ordinal)
				&& Weight.Equals(other.Weight)
				&& FontStyleType.Equals(other.FontStyleType);

		/// <summary>
		/// Returns the hash code for this font source.
		/// </summary>
		/// <returns>A hash code for the current font source.</returns>
		public override int GetHashCode() => Name.GetHashCode(
#if !NETSTANDARD2_0
					StringComparison.Ordinal
#endif
				)
				^ Weight.GetHashCode() ^ FontStyleType.GetHashCode();

		/// <summary>
		/// Determines whether the specified object is equal to the current font source.
		/// </summary>
		/// <param name="obj">The object to compare with the current font source.</param>
		/// <returns><c>true</c> if the specified object is equal to the current font source; otherwise, <c>false</c>.</returns>
		public override bool Equals(object? obj) => obj is FontSource other && Equals(other);

		/// <summary>
		/// Determines whether two specified font sources have the same value.
		/// </summary>
		/// <param name="left">The first font source to compare.</param>
		/// <param name="right">The second font source to compare.</param>
		/// <returns><c>true</c> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator ==(FontSource left, FontSource right) => left.Equals(right);

		/// <summary>
		/// Determines whether two specified font sources have different values.
		/// </summary>
		/// <param name="left">The first font source to compare.</param>
		/// <param name="right">The second font source to compare.</param>
		/// <returns><c>true</c> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator !=(FontSource left, FontSource right) => !(left == right);
	}
}
