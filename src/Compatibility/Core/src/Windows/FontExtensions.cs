using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class FontExtensions
	{
		public static void ApplyFont(this Control self, Font font) =>
			self.UpdateFont(font, CompatServiceProvider.FontManager);

		public static void ApplyFont(this TextBlock self, Font font) =>
			self.UpdateFont(font, CompatServiceProvider.FontManager);

		public static FontFamily ToFontFamily(this Font font) =>
			CompatServiceProvider.FontManager.GetFontFamily(font);

		public static FontFamily ToFontFamily(this string fontFamily) =>
			CompatServiceProvider.FontManager.GetFontFamily(Font.OfSize(fontFamily, 0.0));

		internal static void ApplyFont(this Control self, IFontElement element) =>
			self.UpdateFont(element.AsFont(), CompatServiceProvider.FontManager);

		internal static void ApplyFont(this TextBlock self, IFontElement element) =>
			self.UpdateFont(element.AsFont(), CompatServiceProvider.FontManager);

		internal static void ApplyFont(this UI.Xaml.Documents.TextElement self, IFontElement element) =>
			self.UpdateFont(element.AsFont(), CompatServiceProvider.FontManager);

		internal static double GetFontSize(this NamedSize size) => size switch
		{
			NamedSize.Default => CompatServiceProvider.FontManager.DefaultFontSize,
			NamedSize.Micro => 15.667,
			NamedSize.Small => 18.667,
			NamedSize.Medium => 22.667,
			NamedSize.Large => 32,
			NamedSize.Body => 14,
			NamedSize.Caption => 12,
			NamedSize.Header => 46,
			NamedSize.Subtitle => 20,
			NamedSize.Title => 24,
			_ => throw new ArgumentOutOfRangeException(nameof(size)),
		};

		internal static bool IsDefault(this IFontElement self) =>
			self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) && self.FontAttributes == FontAttributes.None;

		static Font AsFont(this IFontElement element) =>
			Font.OfSize(element.FontFamily, element.FontSize).WithAttributes(element.FontAttributes);
	}
}