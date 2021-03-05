using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using UIKit;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		readonly ConcurrentDictionary<(string family, float size, FontAttributes attributes), UIFont> _fonts =
			new ConcurrentDictionary<(string family, float size, FontAttributes attributes), UIFont>();

		readonly IFontRegistrar _fontRegistrar;

		UIFont? _defaultFont;

		public FontManager(IFontRegistrar fontRegistrar)
		{
			_fontRegistrar = fontRegistrar;
		}

		public UIFont DefaultFont =>
			_defaultFont ??= UIFont.SystemFontOfSize(12);

		public UIFont GetFont(Font font)
		{
			var size = (float)font.FontSize;
			if (font.UseNamedSize)
			{
				switch (font.NamedSize)
				{
					case NamedSize.Micro:
						size = 12;
						break;
					case NamedSize.Small:
						size = 14;
						break;
					case NamedSize.Medium:
						size = 17; // as defined by iOS documentation
						break;
					case NamedSize.Large:
						size = 22;
						break;
					default:
						size = 17;
						break;
				}
			}

			return GetFont(font.FontFamily, size, font.FontAttributes, CreateFont);
		}

		UIFont GetFont(string family, float size, FontAttributes attributes, Func<(string, float, FontAttributes), UIFont> factory)
		{
			return _fonts.GetOrAdd((family, size, attributes), factory);
		}

		UIFont CreateFont((string family, float size, FontAttributes attributes) fontKey)
		{
			var (family, size, attributes) = fontKey;
			var bold = (attributes & FontAttributes.Bold) != 0;
			var italic = (attributes & FontAttributes.Italic) != 0;

			if (family != null && family != DefaultFont.FamilyName)
			{
				try
				{
					UIFont? result = null;
					if (UIFont.FamilyNames.Contains(family))
					{
						var descriptor = new UIFontDescriptor().CreateWithFamily(family);

						if (bold || italic)
						{
							var traits = (UIFontDescriptorSymbolicTraits)0;
							if (bold)
								traits |= UIFontDescriptorSymbolicTraits.Bold;
							if (italic)
								traits |= UIFontDescriptorSymbolicTraits.Italic;

							descriptor = descriptor.CreateWithTraits(traits);
							result = UIFont.FromDescriptor(descriptor, size);
							if (result != null)
								return result;
						}
					}

					var cleansedFont = CleanseFontName(family);
					result = UIFont.FromName(cleansedFont, size);
					if (family.StartsWith(".SFUI", StringComparison.InvariantCultureIgnoreCase))
					{
						var fontWeight = family.Split('-').LastOrDefault();

						if (!string.IsNullOrWhiteSpace(fontWeight) && Enum.TryParse<UIFontWeight>(fontWeight, true, out var uIFontWeight))
						{
							result = UIFont.SystemFontOfSize(size, uIFontWeight);
							return result;
						}

						result = UIFont.SystemFontOfSize(size, UIFontWeight.Regular);
						return result;
					}
					if (result == null)
						result = UIFont.FromName(family, size);
					if (result != null)
						return result;
				}
				catch
				{
					Debug.WriteLine("Could not load font named: {0}", family);
				}
			}

			if (bold && italic)
			{
				var defaultFont = UIFont.SystemFontOfSize(size);

				var descriptor = defaultFont.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold | UIFontDescriptorSymbolicTraits.Italic);
				return UIFont.FromDescriptor(descriptor, 0);
			}

			if (italic)
				return UIFont.ItalicSystemFontOfSize(size);

			if (bold)
				return UIFont.BoldSystemFontOfSize(size);

			return UIFont.SystemFontOfSize(size);
		}

		string? CleanseFontName(string fontName)
		{
			// First check Alias
			var (hasFontAlias, fontPostScriptName) = _fontRegistrar.HasFont(fontName);
			if (hasFontAlias)
				return fontPostScriptName;

			var fontFile = FontFile.FromString(fontName);

			if (!string.IsNullOrWhiteSpace(fontFile.Extension))
			{
				var (hasFont, filePath) = _fontRegistrar.HasFont(fontFile.FileNameWithExtension());
				if (hasFont)
					return filePath ?? fontFile.PostScriptName;
			}
			else
			{
				foreach (var ext in FontFile.Extensions)
				{

					var formated = fontFile.FileNameWithExtension(ext);
					var (hasFont, filePath) = _fontRegistrar.HasFont(formated);
					if (hasFont)
						return filePath;
				}
			}

			return fontFile.PostScriptName;
		}
	}
}