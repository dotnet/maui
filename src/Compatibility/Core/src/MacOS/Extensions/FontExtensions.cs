using System.Diagnostics;
using Microsoft.Maui.Controls.Compatibility.Internals;
using AppKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	public static partial class FontExtensions
	{
		static readonly string DefaultFontName = NSFont.SystemFontOfSize(12).FontName;

		public static NSFont ToNSFont(this Font self) => ToNativeFont(self);

		internal static NSFont ToNSFont(this IFontElement element) => ToNativeFont(element);

		static NSFont _ToNativeFont(string family, float size, FontAttributes attributes)
		{
			NSFont defaultFont = NSFont.SystemFontOfSize(size);
			NSFont font = null;
			NSFontDescriptor descriptor = null;
			var bold = (attributes & FontAttributes.Bold) != 0;
			var italic = (attributes & FontAttributes.Italic) != 0;

			if (family != null && family != DefaultFontName)
			{
				try
				{
					descriptor = new NSFontDescriptor().FontDescriptorWithFamily(family);
					font = NSFont.FromDescription(descriptor, size);

					if (font == null)
					{
						var cleansedFont = CleanseFontName(family);
						font = NSFont.FromFontName(cleansedFont, size);
					}
						
				}
				catch
				{
					Debug.WriteLine("Could not load font named: {0}", family);
				}
			}

			//if we didn't found a Font or Descriptor for the FontFamily use the default one 
			if (font == null)
			{
				font = defaultFont;
				descriptor = defaultFont.FontDescriptor;
			}
		
			if (descriptor == null)
				descriptor = defaultFont.FontDescriptor;


			if (bold || italic)
			{
				var traits = (NSFontSymbolicTraits)0;
				if (bold)
					traits |= NSFontSymbolicTraits.BoldTrait;
				if (italic)
					traits |= NSFontSymbolicTraits.ItalicTrait;

				var fontDescriptorWithTraits = descriptor.FontDescriptorWithSymbolicTraits(traits);

				font = NSFont.FromDescription(fontDescriptorWithTraits, size);
			}
			
			return font.ScreenFontWithRenderingMode(NSFontRenderingMode.AntialiasedIntegerAdvancements);
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