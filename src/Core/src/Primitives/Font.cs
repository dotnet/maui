#nullable enable
using System;

namespace Microsoft.Maui
{
	public struct Font
	{
		public string FontFamily { get; private set; }

		public double FontSize { get; private set; }

		public bool Italic { get; private set; }

		public bool IsDefault => FontFamily == null && FontSize == 0 && Italic == false && Weight == FontWeight.Regular;

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
			return new Font { FontFamily = FontFamily, FontSize = size, Italic = Italic, Weight = Weight };
		}

		public Font WithItalic(bool italic)
		{
			return new Font { FontFamily = FontFamily, FontSize = FontSize, Italic = italic, Weight = Weight };
		}

		public Font WithWeight(FontWeight weight)
		{
			return new Font { FontFamily = FontFamily, FontSize = FontSize, Italic = Italic, Weight = weight };
		}

		public Font WithWeight(FontWeight weight, bool italic)
		{
			return new Font { FontFamily = FontFamily, FontSize = FontSize, Italic = italic, Weight = weight };
		}

		public static Font OfSize(string name, double size, FontWeight weight = FontWeight.Regular , bool italic = false)=>
			new() { FontFamily = name, FontSize = size, Weight = weight, Italic = italic };

		public static Font SystemFontOfSize(double size, FontWeight weight = FontWeight.Regular , bool italic = false) =>
			new () { FontSize = size, Weight = weight, Italic = italic };

		public static Font SystemFontOfWeight(FontWeight weight, bool italic = false)
		{
			var result = new Font { Weight = weight, Italic = italic };
			return result;
		}

		bool Equals(Font other)
		{
			return string.Equals(FontFamily, other.FontFamily) && FontSize.Equals(other.FontSize) && Weight == other.Weight && Italic == other.Italic;
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

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = FontFamily != null ? FontFamily.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ FontSize.GetHashCode();
				hashCode = (hashCode * 397) ^ Weight.GetHashCode();
				hashCode = (hashCode * 397) ^ Italic.GetHashCode();

				return hashCode;
			}
		}

		public static bool operator ==(Font left, Font right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Font left, Font right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
			=> $"FontFamily: {FontFamily}, FontSize: {FontSize}, Weight: {Weight}, Italic: {Italic}";
	}
}