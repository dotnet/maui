using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public static class FontExtensions
	{
		public static void ApplyFont(this Control self, Font font)
		{
			self.FontSize = font.UseNamedSize ? GetFontSize(font.NamedSize) : font.FontSize;

			if (!string.IsNullOrEmpty(font.FontFamily))
				self.FontFamily = new FontFamily(font.FontFamily);
			else
				self.FontFamily = (FontFamily)System.Windows.Application.Current.Resources["FontFamilySemiBold"];

			if (font.FontAttributes.HasFlag(FontAttributes.Italic))
				self.FontStyle = FontStyles.Italic;
			else
				self.FontStyle = FontStyles.Normal;

			if (font.FontAttributes.HasFlag(FontAttributes.Bold))
				self.FontWeight = FontWeights.Bold;
			else
				self.FontWeight = FontWeights.Normal;
		}

		public static void ApplyFont(this TextBlock self, Font font)
		{
			self.FontSize = font.UseNamedSize ? GetFontSize(font.NamedSize) : font.FontSize;

			if (!string.IsNullOrEmpty(font.FontFamily))
				self.FontFamily = new FontFamily(font.FontFamily);
			else
			{
				self.FontFamily = (FontFamily)System.Windows.Application.Current.Resources["FontFamilyNormal"];
			}

			if (font.FontAttributes.HasFlag(FontAttributes.Italic))
				self.FontStyle = FontStyles.Italic;
			else
				self.FontStyle = FontStyles.Normal;

			if (font.FontAttributes.HasFlag(FontAttributes.Bold))
				self.FontWeight = FontWeights.Bold;
			else
				self.FontWeight = FontWeights.Normal;
		}

		public static void ApplyFont(this TextElement self, Font font)
		{
			self.FontSize = font.UseNamedSize ? GetFontSize(font.NamedSize) : font.FontSize;

			if (!string.IsNullOrEmpty(font.FontFamily))
				self.FontFamily = new FontFamily(font.FontFamily);
			else
				self.FontFamily = (FontFamily)System.Windows.Application.Current.Resources["FontFamilyNormal"];

			if (font.FontAttributes.HasFlag(FontAttributes.Italic))
				self.FontStyle = FontStyles.Italic;
			else
				self.FontStyle = FontStyles.Normal;

			if (font.FontAttributes.HasFlag(FontAttributes.Bold))
				self.FontWeight = FontWeights.Bold;
			else
				self.FontWeight = FontWeights.Normal;
		}

		internal static void ApplyFont(this Control self, IFontElement element)
		{
			self.FontSize = element.FontSize;

			if (!string.IsNullOrEmpty(element.FontFamily))
				self.FontFamily = new FontFamily(element.FontFamily);
			else
				self.FontFamily = (FontFamily)System.Windows.Application.Current.Resources["FontFamilySemiBold"];

			if (element.FontAttributes.HasFlag(FontAttributes.Italic))
				self.FontStyle = FontStyles.Italic;
			else
				self.FontStyle = FontStyles.Normal;

			if (element.FontAttributes.HasFlag(FontAttributes.Bold))
				self.FontWeight = FontWeights.Bold;
			else
				self.FontWeight = FontWeights.Normal;
		}

		internal static double GetFontSize(this NamedSize size)
		{
			switch (size)
			{
				case NamedSize.Default:
					return (double)System.Windows.Application.Current.Resources["ControlContentThemeFontSize"];
				case NamedSize.Micro:
					return (double)System.Windows.Application.Current.Resources["FontSizeSmall"] - 3;
				case NamedSize.Small:
					return (double)System.Windows.Application.Current.Resources["FontSizeSmall"];
				case NamedSize.Medium:
					return (double)System.Windows.Application.Current.Resources["FontSizeNormal"];
					// use normal instead of medium as this is the default
				case NamedSize.Large:
					return (double)System.Windows.Application.Current.Resources["FontSizeLarge"];
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
