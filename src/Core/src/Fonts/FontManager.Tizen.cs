using System;
using System.Collections.Concurrent;
using System.IO;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		readonly ConcurrentDictionary<(string? family, float size, FontSlant slant), string> _fonts = new();

		readonly IFontRegistrar _fontRegistrar;
		readonly IServiceProvider? _serviceProvider;

		public double DefaultFontSize => 14; // 14sp

		public FontManager(IFontRegistrar fontRegistrar, IServiceProvider? serviceProvider = null)
		{
			_fontRegistrar = fontRegistrar;
			_serviceProvider = serviceProvider;
		}


		public string GetFont(Font font)
		{
			var size = font.Size <= 0 || double.IsNaN(font.Size)
				? (float)DefaultFontSize
				: (float)font.Size;

			return GetFont(font.Family, size, font.Slant, GetNativeFontFamily);
		}

		public string GetFontFamily(string? fontFamliy)
		{
			if (string.IsNullOrEmpty(fontFamliy))
				return "";

			var cleansedFont = CleanseFontName(fontFamliy ?? string.Empty);
			if (cleansedFont == null)
				return "";

			int index = cleansedFont.LastIndexOf('-');
			if (index != -1)
			{
				string font = cleansedFont.Substring(0, index);
				string style = cleansedFont.Substring(index + 1);
				return $"{font}:style={style}";
			}
			else
			{
				return cleansedFont;
			}
		}

		string GetFont(string? family, float size, FontSlant slant, Func<(string?, float, FontSlant), string> factory)
		{
			return _fonts.GetOrAdd((family, size, slant), factory);
		}

		string GetNativeFontFamily((string? family, float size, FontSlant slant) fontKey)
		{
			if (string.IsNullOrEmpty(fontKey.family))
				return "";

			var cleansedFont = CleanseFontName(fontKey.family ?? string.Empty);

			if (cleansedFont == null)
				return "";

			int index = cleansedFont.LastIndexOf('-');
			if (index != -1)
			{
				string font = cleansedFont.Substring(0, index);
				string style = cleansedFont.Substring(index + 1);
				return $"{font}:style={style}";
			}
			else
			{
				return cleansedFont;
			}
		}

		string? CleanseFontName(string fontName)
		{
			// First check Alias
			if (_fontRegistrar.GetFont(fontName) is string fontPostScriptName)
				return fontPostScriptName;

			var fontFile = FontFile.FromString(fontName);

			if (!string.IsNullOrWhiteSpace(fontFile.Extension))
			{
				if (_fontRegistrar.GetFont(fontFile.FileNameWithExtension()) is string filePath)
					return filePath ?? fontFile.PostScriptName;
			}
			else
			{
				foreach (var ext in FontFile.Extensions)
				{

					var formatted = fontFile.FileNameWithExtension(ext);
					if (_fontRegistrar.GetFont(formatted) is string filePath)
						return filePath;
				}
			}

			return fontFile.PostScriptName;
		}
	}
}