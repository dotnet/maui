using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xamarin.Forms.Core;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public static class FontExtensions
	{
		public static void ApplyFont(this Control self, Font font)
		{
			self.FontSize = font.UseNamedSize ? GetFontSize(font.NamedSize) : font.FontSize;

			self.FontFamily = font.FontFamily.ToFontFamily("FontFamilySemiBold");

			ApplyFontAttributes(self, font.FontAttributes);
		}

		public static void ApplyFontAttributes(this Control self, FontAttributes fontAttributes)
		{
			if (fontAttributes.HasFlag(FontAttributes.Italic))
				self.FontStyle = FontStyles.Italic;
			else
				self.FontStyle = FontStyles.Normal;

			if (fontAttributes.HasFlag(FontAttributes.Bold))
				self.FontWeight = FontWeights.Bold;
			else
				self.FontWeight = FontWeights.Normal;
		}

		public static void ApplyFont(this TextBlock self, Font font)
		{
			self.FontSize = font.UseNamedSize ? GetFontSize(font.NamedSize) : font.FontSize;

			self.FontFamily = font.FontFamily.ToFontFamily("FontFamilyNormal");

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

			self.FontFamily = font.FontFamily.ToFontFamily("FontFamilyNormal");

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

			self.FontFamily = element.FontFamily.ToFontFamily();

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

		static Dictionary<string, FontFamily> FontFamilies = new Dictionary<string, FontFamily>();


		public static FontFamily ToFontFamily(this string fontFamily, string defaultFontResource = "FontFamilySemiBold")
		{
			if (string.IsNullOrWhiteSpace(fontFamily))
				return (FontFamily)System.Windows.Application.Current.Resources[defaultFontResource];

			if (FontFamilies.TryGetValue(fontFamily, out var f))
			{
				return f;
			}

			const string packUri = "pack://application:,,,/";
			if (fontFamily.StartsWith(packUri))
			{
				var fontName = fontFamily.Remove(0, packUri.Length);
				return new FontFamily(new Uri(packUri), fontName);
			}

			var embeddedResult = fontFamily.TryGetFromAssets();

			if (embeddedResult.success)
				return FontFamilies[fontFamily] = embeddedResult.fontFamily;
			//self.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), fontFamily);

			//Cache this puppy!
			var formatted = string.Join(", ", GetAllFontPossibilities(fontFamily));
			var font = new FontFamily(formatted);
			FontFamilies[fontFamily] = font;
			return font;

		}

		static FontFamily CreateFromFile(string file)
		{
			var collection = new PrivateFontCollection();
			collection.AddFontFile(file);
			var family = collection.Families[0];
			var dir = Path.GetDirectoryName(file);
			var urlPath = $"file:////{file}";
			//var uri = new Uri(urlPath);
			return new FontFamily($"{urlPath}#{family.Name}");
		}

		static (bool success, FontFamily fontFamily) TryGetFromAssets(this string fontName)
		{
			//First check Alias
			var (hasFontAlias, fontPostScriptName) = FontRegistrar.HasFont(fontName);
			if (hasFontAlias)
				return (true, CreateFromFile(fontPostScriptName));

			var folders = new[]
			{
				"",
				"Fonts/",
				"fonts/",
			};


			//copied text
			var fontFile = FontFile.FromString(fontName);

			if (!string.IsNullOrWhiteSpace(fontFile.Extension))
			{
				var (hasFont, fontPath) = FontRegistrar.HasFont(fontFile.FileNameWithExtension());
				if (hasFont)
				{
					return (true, CreateFromFile(fontPath));
				}
			}
			else
			{
				foreach (var ext in FontFile.Extensions)
				{
					var formated = fontFile.FileNameWithExtension(ext);
					var (hasFont, fontPath) = FontRegistrar.HasFont(formated);
					if (hasFont)
					{
						return (true, CreateFromFile(fontPath));
					}
				}
			}

			return (false, null);
		}


		static IEnumerable<string> GetAllFontPossibilities(string fontFamily)
		{
			//First check Alias
			var (hasFontAlias, fontPostScriptName) = FontRegistrar.HasFont(fontFamily);
			if (hasFontAlias)
			{
				var file = FontFile.FromString(Path.GetFileName(fontPostScriptName));
				var formated = $"{fontPostScriptName}#{file.GetPostScriptNameWithSpaces()}";
				yield return formated;
				yield return fontFamily;
				yield break;
			}

			const string path = "Assets/Fonts/";
			string[] extensions = new[]
			{
				".ttf",
				".otf",
			};

			var fontFile = FontFile.FromString(fontFamily);
			//If the extension is provided, they know what they want!
			var hasExtension = !string.IsNullOrWhiteSpace(fontFile.Extension);
			if (hasExtension)
			{
				var (hasFont, filePath) = FontRegistrar.HasFont(fontFile.FileNameWithExtension());
				if (hasFont)
				{
					var formated = $"{filePath}#{fontFile.GetPostScriptNameWithSpaces()}";
					yield return formated;
					yield break;
				}
				else
				{
					yield return $"{path}{fontFile.FileNameWithExtension()}";
				}
			}
			foreach (var ext in extensions)
			{
				var (hasFont, filePath) = FontRegistrar.HasFont(fontFile.FileNameWithExtension(ext));
				if (hasFont)
				{
					var formatted = $"{filePath}#{fontFile.GetPostScriptNameWithSpaces()}";
					yield return formatted;
					yield break;
				}
			}

			//Always send the base back
			yield return fontFamily;

			foreach (var ext in extensions)
			{
				var formatted = $"{path}{fontFile.FileNameWithExtension(ext)}#{fontFile.GetPostScriptNameWithSpaces()}";
				yield return formatted;
			}
		}
	}
}
