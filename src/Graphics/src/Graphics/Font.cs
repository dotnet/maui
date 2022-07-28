using System;

namespace Microsoft.Maui.Graphics
{
	public static class FontWeights
	{
		public const int Default = -1;

		public const int Thin = 100;
		public const int ExtraLight = 200;
		public const int UltraLight = 200;
		public const int Light = 300;
		public const int SemiLight = 400;
		public const int Normal = 400;
		public const int Regular = 400;
		public const int Medium = 500;
		public const int DemiBold = 600;
		public const int SemiBold = 600;
		public const int Bold = 700;
		public const int ExtraBold = 800;
		public const int UltraBold = 800;
		public const int Black = 900;
		public const int Heavy = 900;
		public const int ExtraBlack = 950;
		public const int UltraBlack = 950;
	}

	public struct Font : IFont, IEquatable<IFont>
	{
		public static Font Default
			=> new Font(null);

		public static Font DefaultBold
			=> new Font(null, FontWeights.Bold);

		public Font(string name, int weight = FontWeights.Normal, FontStyleType styleType = FontStyleType.Normal)
		{
			Name = name;
			Weight = weight;
			StyleType = styleType;
		}

		public string Name { get; private set; }
		public int Weight { get; private set; }
		public FontStyleType StyleType { get; private set; }

		public bool Equals(IFont other)
			=> StyleType == other.StyleType && Weight == other.Weight && Name.Equals(other.Name);

		public override bool Equals(object obj)
			=> obj is IFont font && Equals(font);

		public override int GetHashCode()
			=> (Name, Weight, StyleType).GetHashCode();

		public bool IsDefault
			=> string.IsNullOrEmpty(Name);
	}
}
