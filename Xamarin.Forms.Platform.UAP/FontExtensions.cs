using System;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using WApplication = Windows.UI.Xaml.Application;

namespace Xamarin.Forms.Platform.UWP
{
	public static class FontExtensions
	{
		public static void ApplyFont(this Control self, Font font)
		{
			self.FontSize = font.UseNamedSize ? font.NamedSize.GetFontSize() : font.FontSize;
			self.FontFamily = !string.IsNullOrEmpty(font.FontFamily) ? new FontFamily(font.FontFamily) : (FontFamily)WApplication.Current.Resources["ContentControlThemeFontFamily"];
			self.FontStyle = font.FontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;
			self.FontWeight = font.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}

		public static void ApplyFont(this TextBlock self, Font font)
		{
			self.FontSize = font.UseNamedSize ? font.NamedSize.GetFontSize() : font.FontSize;
			self.FontFamily = !string.IsNullOrEmpty(font.FontFamily) ? new FontFamily(font.FontFamily) : (FontFamily)WApplication.Current.Resources["ContentControlThemeFontFamily"];
			self.FontStyle = font.FontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;
			self.FontWeight = font.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}

		public static void ApplyFont(this Windows.UI.Xaml.Documents.TextElement self, Font font)
		{
			self.FontSize = font.UseNamedSize ? font.NamedSize.GetFontSize() : font.FontSize;
			self.FontFamily = !string.IsNullOrEmpty(font.FontFamily) ? new FontFamily(font.FontFamily) : (FontFamily)WApplication.Current.Resources["ContentControlThemeFontFamily"];
			self.FontStyle = font.FontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;
			self.FontWeight = font.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}

		internal static void ApplyFont(this Control self, IFontElement element)
		{
			self.FontSize = element.FontSize;
			self.FontFamily = !string.IsNullOrEmpty(element.FontFamily) ? new FontFamily(element.FontFamily) : (FontFamily)WApplication.Current.Resources["ContentControlThemeFontFamily"];
			self.FontStyle = element.FontAttributes.HasFlag(FontAttributes.Italic) ? FontStyle.Italic : FontStyle.Normal;
			self.FontWeight = element.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}

		internal static double GetFontSize(this NamedSize size)
		{
			// These are values pulled from the mapped sizes on Windows Phone, WinRT has no equivalent sizes, only intents.
			switch (size)
			{
				case NamedSize.Default:
					return (double)WApplication.Current.Resources["ControlContentThemeFontSize"];
				case NamedSize.Micro:
					return 18.667 - 3;
				case NamedSize.Small:
					return 18.667;
				case NamedSize.Medium:
					return 22.667;
				case NamedSize.Large:
					return 32;
				default:
					throw new ArgumentOutOfRangeException("size");
			}
		}

		internal static bool IsDefault(this IFontElement self)
		{
			return self.FontFamily == null && self.FontSize == Device.GetNamedSize(NamedSize.Default, typeof(Label), true) && self.FontAttributes == FontAttributes.None;
		}
	}
}