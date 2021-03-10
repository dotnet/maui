using System;

namespace Microsoft.Maui
{
	public struct Font
	{
		public string FontFamily { get; private set; }

		public double FontSize { get; private set; }

		public NamedSize NamedSize { get; private set; }

		public FontAttributes FontAttributes { get; private set; }

		public bool IsDefault
		{
			get { return FontFamily == null && FontSize == 0 && NamedSize == NamedSize.Default && FontAttributes == FontAttributes.None; }
		}

		public bool UseNamedSize
		{
			get { return FontSize <= 0; }
		}

		public static Font Default
		{
			get { return default(Font); }
		}

		public Font WithSize(double size)
		{
			return new Font { FontFamily = FontFamily, FontSize = size, NamedSize = 0, FontAttributes = FontAttributes };
		}

		public Font WithSize(NamedSize size)
		{
			if (size <= 0)
				throw new ArgumentOutOfRangeException("size");

			return new Font { FontFamily = FontFamily, FontSize = 0, NamedSize = size, FontAttributes = FontAttributes };
		}

		public Font WithAttributes(FontAttributes fontAttributes)
		{
			return new Font { FontFamily = FontFamily, FontSize = FontSize, NamedSize = NamedSize, FontAttributes = fontAttributes };
		}

		public static Font OfSize(string name, double size)
		{
			var result = new Font { FontFamily = name, FontSize = size };
			return result;
		}

		public static Font OfSize(string name, NamedSize size)
		{
			var result = new Font { FontFamily = name, NamedSize = size };
			return result;
		}

		public static Font SystemFontOfSize(double size)
		{
			var result = new Font { FontSize = size };
			return result;
		}

		public static Font SystemFontOfSize(NamedSize size)
		{
			var result = new Font { NamedSize = size };
			return result;
		}

		public static Font SystemFontOfSize(double size, FontAttributes attributes)
		{
			var result = new Font { FontSize = size, FontAttributes = attributes };
			return result;
		}

		public static Font SystemFontOfSize(NamedSize size, FontAttributes attributes)
		{
			var result = new Font { NamedSize = size, FontAttributes = attributes };
			return result;
		}

		bool Equals(Font other)
		{
			return string.Equals(FontFamily, other.FontFamily) && FontSize.Equals(other.FontSize) && NamedSize == other.NamedSize && FontAttributes == other.FontAttributes;
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
				hashCode = (hashCode * 397) ^ NamedSize.GetHashCode();
				hashCode = (hashCode * 397) ^ FontAttributes.GetHashCode();

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
		{
			return string.Format("FontFamily: {0}, FontSize: {1}, NamedSize: {2}, FontAttributes: {3}", FontFamily, FontSize, NamedSize, FontAttributes);
		}
	}
}