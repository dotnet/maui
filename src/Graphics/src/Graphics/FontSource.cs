#nullable enable

using System;

namespace Microsoft.Maui.Graphics
{
	public struct FontSource : IEquatable<FontSource>
	{
		public FontSource(string filename, int weight = FontWeights.Normal, FontStyleType fontStyleType = FontStyleType.Normal)
		{
			Name = filename;
			Weight = weight;
			FontStyleType = fontStyleType;
		}
		public readonly string Name;
		public readonly int Weight;
		public readonly FontStyleType FontStyleType;

		public bool Equals(FontSource other)
			=> Name.Equals(other.Name)
				&& Weight.Equals(other.Weight)
				&& FontStyleType.Equals(other.FontStyleType);

		public override int GetHashCode()
			=> Name.GetHashCode() ^ Weight.GetHashCode() ^ FontStyleType.GetHashCode();

		public override bool Equals(object? obj) => obj is FontSource other && Equals(other);

		public static bool operator ==(FontSource left, FontSource right) => left.Equals(right);

		public static bool operator !=(FontSource left, FontSource right) => !(left == right);
	}
}
