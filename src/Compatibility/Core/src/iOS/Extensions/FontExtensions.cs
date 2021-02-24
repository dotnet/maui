using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls.Compatibility.Internals;
using UIKit;
using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public static partial class FontExtensions
	{
		static readonly string _defaultFontName = UIFont.SystemFontOfSize(12).Name;

		public static UIFont ToUIFont(this Font self) => ToNativeFont(self);

		internal static UIFont ToUIFont(this IFontElement element) => ToNativeFont(element);

		static UIFont _ToNativeFont(string family, float size, FontAttributes attributes)
		{
			var bold = (attributes & FontAttributes.Bold) != 0;
			var italic = (attributes & FontAttributes.Italic) != 0;

			if (family != null && family != _defaultFontName)
			{
				try
				{
					UIFont result = null;
					if (UIFont.FamilyNames.Contains(family))
					{
						var descriptor = new UIFontDescriptor().CreateWithFamily(family);

						if (bold || italic)
						{
							var traits = (UIFontDescriptorSymbolicTraits)0;
							if (bold)
								traits = traits | UIFontDescriptorSymbolicTraits.Bold;
							if (italic)
								traits = traits | UIFontDescriptorSymbolicTraits.Italic;

							descriptor = descriptor.CreateWithTraits(traits);
							result = UIFont.FromDescriptor(descriptor, size);
							if (result != null)
								return result;
						}
					}

					var cleansedFont = CleanseFontName(family);
					result = UIFont.FromName(cleansedFont, size);
					if (family.StartsWith(".SFUI", System.StringComparison.InvariantCultureIgnoreCase))
					{
						var fontWeight = family.Split('-').LastOrDefault();

						if (!string.IsNullOrWhiteSpace(fontWeight) && System.Enum.TryParse<UIFontWeight>(fontWeight, true, out var uIFontWeight))
						{
							result = UIFont.SystemFontOfSize(size, uIFontWeight);
							return result;
						}

						result = UIFont.SystemFontOfSize(size, UIFontWeight.Regular);
						return result;
					}
					if(result == null)
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

		internal static string CleanseFontName(string fontName)
		{

			//First check Alias
			var (hasFontAlias, fontPostScriptName) = FontRegistrar.HasFont(fontName);
			if (hasFontAlias)
				return fontPostScriptName;

			var fontFile = FontFile.FromString(fontName);

			if (!string.IsNullOrWhiteSpace(fontFile.Extension))
			{
				var (hasFont, filePath) = FontRegistrar.HasFont(fontFile.FileNameWithExtension());
				if (hasFont)
					return filePath ?? fontFile.PostScriptName;
			}
			else
			{
				foreach (var ext in FontFile.Extensions)
				{

					var formated = fontFile.FileNameWithExtension(ext);
					var (hasFont, filePath) = FontRegistrar.HasFont(formated);
					if (hasFont)
						return filePath;
				}
			}
			return fontFile.PostScriptName;
		}
	}
}