using System;
using System.Collections.Generic;
using Android.Graphics;

namespace Xamarin.Forms.Platform.Android
{
	public static class FontExtensions
	{
		static readonly Dictionary<Tuple<string, FontAttributes>, Typeface> Typefaces = new Dictionary<Tuple<string, FontAttributes>, Typeface>();

		static Typeface s_defaultTypeface;

		public static float ToScaledPixel(this Font self)
		{
			if (self.IsDefault)
				return 14;

			if (self.UseNamedSize)
			{
				switch (self.NamedSize)
				{
					case NamedSize.Micro:
						return 10;
					case NamedSize.Small:
						return 12;
					case NamedSize.Default:
					case NamedSize.Medium:
						return 14;
					case NamedSize.Large:
						return 18;
				}
			}

			return (float)self.FontSize;
		}

		public static Typeface ToTypeface(this Font self)
		{
			if (self.IsDefault)
				return s_defaultTypeface ?? (s_defaultTypeface = Typeface.Default);

			var key = new Tuple<string, FontAttributes>(self.FontFamily, self.FontAttributes);
			Typeface result;
			if (Typefaces.TryGetValue(key, out result))
				return result;

			var style = TypefaceStyle.Normal;
			if ((self.FontAttributes & (FontAttributes.Bold | FontAttributes.Italic)) == (FontAttributes.Bold | FontAttributes.Italic))
				style = TypefaceStyle.BoldItalic;
			else if ((self.FontAttributes & FontAttributes.Bold) != 0)
				style = TypefaceStyle.Bold;
			else if ((self.FontAttributes & FontAttributes.Italic) != 0)
				style = TypefaceStyle.Italic;

			if (self.FontFamily != null)
				result = Typeface.Create(self.FontFamily, style);
			else
				result = Typeface.Create(Typeface.Default, style);

			Typefaces[key] = result;
			return result;
		}

		internal static bool IsDefault(this IFontElement self)
		{
			return self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) && self.FontAttributes == FontAttributes.None;
		}

		internal static Typeface ToTypeface(this IFontElement self)
		{
			var key = new Tuple<string, FontAttributes>(self.FontFamily, self.FontAttributes);
			Typeface result;
			if (Typefaces.TryGetValue(key, out result))
				return result;

			var style = TypefaceStyle.Normal;
			if ((self.FontAttributes & (FontAttributes.Bold | FontAttributes.Italic)) == (FontAttributes.Bold | FontAttributes.Italic))
				style = TypefaceStyle.BoldItalic;
			else if ((self.FontAttributes & FontAttributes.Bold) != 0)
				style = TypefaceStyle.Bold;
			else if ((self.FontAttributes & FontAttributes.Italic) != 0)
				style = TypefaceStyle.Italic;

			if (self.FontFamily != null)
				result = Typeface.Create(self.FontFamily, style);
			else
				result = Typeface.Create(Typeface.Default, style);

			Typefaces[key] = result;
			return result;
		}
	}
}