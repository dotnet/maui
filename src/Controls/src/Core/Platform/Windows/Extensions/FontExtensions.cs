#nullable disable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FontExtensions
	{
		internal static void ApplyFont(this UI.Xaml.Documents.TextElement self, Font font, IFontManager fontManager) =>
			self.UpdateFont(font, fontManager);

		public static void ApplyFont(this Control self, Font font, IFontManager fontManager) =>
			self.UpdateFont(font, fontManager);

		public static void ApplyFont(this TextBlock self, Font font, IFontManager fontManager) =>
			self.UpdateFont(font, fontManager);

		public static FontFamily ToFontFamily(this string fontFamily, IFontManager fontManager) =>
			fontManager.GetFontFamily(Font.OfSize(fontFamily, 0.0));

		internal static void ApplyFont<TFontElement>(this Control self, TFontElement element) where TFontElement : Element, IFontElement
			=> self.UpdateFont(element.ToFont(), element.RequireFontManager());

		internal static void ApplyFont<TFontElement>(this TextBlock self, TFontElement element) where TFontElement : Element, IFontElement
			=> self.UpdateFont(element.ToFont(), element.RequireFontManager());

		internal static void ApplyFont<TFontElement>(this UI.Xaml.Documents.TextElement self, TFontElement element) where TFontElement : Element, IFontElement
			=> self.UpdateFont(element.ToFont(), element.RequireFontManager());

		[Obsolete]
		internal static double GetFontSize(this NamedSize size) => size switch
		{
			NamedSize.Default => (double)UI.Xaml.Application.Current.Resources["ControlContentThemeFontSize"],
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
	}
}