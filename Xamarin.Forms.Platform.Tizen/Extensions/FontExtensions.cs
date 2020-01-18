using Xamarin.Forms.Core;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class FontExtensions
	{
		public static string ToNativeFontFamily(this string self)
		{
			if (string.IsNullOrEmpty(self))
				return null;

			var cleansedFont = CleanseFontName(self);
			int index = cleansedFont.LastIndexOf('-');
			if (index != -1)
			{
				string font = cleansedFont.Substring(0, index);
				string style = cleansedFont.Substring(index+1);
				return $"{font}:style={style}";
			}
			else
			{
				return cleansedFont;
			}
		}

		static string CleanseFontName(string fontName)
		{
			var fontFile = FontFile.FromString(fontName);

			if (!string.IsNullOrWhiteSpace(fontFile.Extension))
			{
				var (hasFont, _) = FontRegistrar.HasFont(fontFile.FileNameWithExtension());
				if (hasFont)
					return fontFile.PostScriptName;
			}
			else
			{
				foreach (var ext in FontFile.Extensions)
				{
					var formated = fontFile.FileNameWithExtension(ext);
					var (hasFont, filePath) = FontRegistrar.HasFont(formated);
					if (hasFont)
						return fontFile.PostScriptName;
				}
			}
			return fontFile.PostScriptName;
		}
	}
}
