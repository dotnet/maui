#nullable enable
using System;

namespace Microsoft.Maui
{
	public struct Font
	{
		public string Family { get; private set; }

		public double Size { get; private set; }

		public FontSlant Slant { get; private set; }

		public bool IsDefault => Family == null && Size == 0 && Slant == FontSlant.Default && Weight == FontWeight.Regular;

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
			return new Font { Family = Family, Size = size, Slant = Slant, Weight = Weight };
		}

		public Font WithSlant(FontSlant fontSlant)
		{
			return new Font { Family = Family, Size = Size, Slant = fontSlant, Weight = Weight };
		}

		public Font WithWeight(FontWeight weight)
		{
			return new Font { Family = Family, Size = Size, Slant = Slant, Weight = weight };
		}

		public Font WithWeight(FontWeight weight, FontSlant fontSlant)
		{
			return new Font { Family = Family, Size = Size, Slant = fontSlant, Weight = weight };
		}

		public static Font OfSize(string name, double size, FontWeight weight = FontWeight.Regular, FontSlant fontSlant = FontSlant.Default) =>
			new() { Family = name, Size = size, Weight = weight, Slant = fontSlant };

		public static Font SystemFontOfSize(double size, FontWeight weight = FontWeight.Regular, FontSlant fontSlant = FontSlant.Default) =>
			new() { Size = size, Weight = weight, Slant = fontSlant };

		public static Font SystemFontOfWeight(FontWeight weight, FontSlant fontSlant = FontSlant.Default)
		{
			var result = new Font { Weight = weight, Slant = fontSlant };
			return result;
		}

		bool Equals(Font other)
		{
			return string.Equals(Family, other.Family) && Size.Equals(other.Size) && Weight == other.Weight && Slant == other.Slant;
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

		public override int GetHashCode() => (Family, Size, Weight, Slant).GetHashCode();

		public static bool operator ==(Font left, Font right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Font left, Font right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
			=> $"Family: {Family}, Size: {Size}, Weight: {Weight}, Slant: {Slant}";
	}
}