using Android.Graphics;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public static class FontExtensions
	{
		public static Typeface ToTypeface(this Font self)
			=> CompatServiceProvider.FontManager.GetTypeface(self);

		internal static Typeface ToTypeface(this string fontfamily, FontAttributes attr = FontAttributes.None)
			=> CompatServiceProvider.FontManager.GetTypeface(Font.OfSize(fontfamily, 0.0).WithAttributes(attr));

		internal static bool IsDefault(this IFontElement self)
			=> self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) && self.FontAttributes == FontAttributes.None;

		internal static Typeface ToTypeface(this IFontElement self)
		{
			if (self.IsDefault())
				return CompatServiceProvider.FontManager.DefaultTypeface;

			var font = Font.OfSize(self.FontFamily, self.FontSize).WithAttributes(self.FontAttributes);

			return CompatServiceProvider.FontManager.GetTypeface(font);
		}
	}
}