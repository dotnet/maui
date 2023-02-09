using Android.Graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FontExtensions
	{
		public static Typeface ToTypeface(this Font self, IFontManager fontManager)
		{
			if (self.IsDefault)
				return fontManager.DefaultTypeface;

			return fontManager.GetTypeface(self) ?? fontManager.DefaultTypeface;
		}

		public static Typeface ToTypeface(this IFontElement self, IFontManager fontManager)
			=> self.ToFont().ToTypeface(fontManager);

		public static Typeface ToTypeface<TFontElement>(this TFontElement self) where TFontElement : Element, IFontElement
			=> self.ToTypeface(self.RequireFontManager());

		internal static Typeface ToTypeface(this string fontfamily, IFontManager fontManager, FontAttributes attr = FontAttributes.None)
			=> fontManager.GetTypeface(Font.OfSize(fontfamily, 0.0).WithAttributes(attr)) ?? fontManager.DefaultTypeface;
	}
}