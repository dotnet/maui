#nullable enable
using System;

namespace Microsoft.Maui
{
	public struct Font
	{
		public string FontFamily { get; private set; }

		public double FontSize { get; private set; }

		public FontSlant FontSlant { get; private set; }

		public bool IsDefault => FontFamily == null && FontSize == 0 && FontSlant == FontSlant.Default && Weight == FontWeight.Regular;

		static Font _default = default(Font).WithWeight(FontWeight.Regular);
		public static Font Default => _default;

		FontWeight _weight;
		public FontWeight Weight
		{
			get => _weight <= 0 ? FontWeight.Regular : _weight;
			private set => _weight = value;
		}

		public Font WithSize(double size)
		{
			return new Font { FontFamily = FontFamily, FontSize = size, FontSlant = FontSlant, Weight = Weight };
		}

		public Font WithSlant(FontSlant fontSlant)
		{
			return new Font { FontFamily = FontFamily, FontSize = FontSize, FontSlant = fontSlant, Weight = Weight };
		}

		public Font WithWeight(FontWeight weight)
		{
			return new Font { FontFamily = FontFamily, FontSize = FontSize, FontSlant = FontSlant, Weight = weight };
		}

		public Font WithWeight(FontWeight weight, FontSlant fontSlant)
		{
			return new Font { FontFamily = FontFamily, FontSize = FontSize, FontSlant = fontSlant, Weight = weight };
		}

		public static Font OfSize(string name, double size, FontWeight weight = FontWeight.Regular, FontSlant fontSlant = FontSlant.Default) =>
			new() { FontFamily = name, FontSize = size, Weight = weight, FontSlant = fontSlant };

		public static Font SystemFontOfSize(double size, FontWeight weight = FontWeight.Regular, FontSlant fontSlant = FontSlant.Default) =>
			new() { FontSize = size, Weight = weight, FontSlant = fontSlant };

		public static Font SystemFontOfWeight(FontWeight weight, FontSlant fontSlant = FontSlant.Default)
		{
			var result = new Font { Weight = weight, FontSlant = fontSlant };
			return result;
		}

		bool Equals(Font other)
		{
			return string.Equals(FontFamily, other.FontFamily) && FontSize.Equals(other.FontSize) && Weight == other.Weight && FontSlant == other.FontSlant;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Font)obj);
		}

		public override int GetHashCode() => (FontFamily, FontSize, Weight, FontSlant).GetHashCode();

		public static bool operator ==(Font left, Font right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Font left, Font right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
			=> $"FontFamily: {FontFamily}, FontSize: {FontSize}, Weight: {Weight}, FontSlant: {FontSlant}";
	}
}