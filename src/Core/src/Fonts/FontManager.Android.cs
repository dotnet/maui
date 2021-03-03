using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Android.Graphics;
using AApplication = Android.App.Application;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		readonly ConcurrentDictionary<(string fontFamily, FontAttributes fontAttributes), Typeface?> _typefaces =
			new ConcurrentDictionary<(string fontFamily, FontAttributes fontAttributes), Typeface?>();

		readonly IFontRegistrar _fontRegistrar;

		Typeface? _defaultTypeface;

		public FontManager(IFontRegistrar fontRegistrar)
		{
			_fontRegistrar = fontRegistrar;
		}

		public Typeface DefaultTypeface => _defaultTypeface ??= Typeface.Default!;

		public Typeface? GetTypeface(Font font)
		{
			if (font.IsDefault || (font.FontAttributes == FontAttributes.None && string.IsNullOrEmpty(font.FontFamily)))
				return DefaultTypeface;

			return _typefaces.GetOrAdd((font.FontFamily, font.FontAttributes), CreateTypeface);
		}

		public float GetScaledPixel(Font font)
		{
			if (font.IsDefault)
				return 14;

			if (font.UseNamedSize)
			{
				switch (font.NamedSize)
				{
					case NamedSize.Micro:
						return 10;

					case NamedSize.Small:
						return 12;

					case NamedSize.Default:
					case NamedSize.Medium:
						return 14;

					case NamedSize.Large:
						return 18;
				}
			}

			return (float)font.FontSize;
		}

		(bool success, Typeface? typeface) TryGetFromAssets(string fontName)
		{
			//First check Alias
			var (hasFontAlias, fontPostScriptName) = _fontRegistrar.HasFont(fontName);
			if (hasFontAlias)
				return (true, Typeface.CreateFromFile(fontPostScriptName));

			var isAssetFont = IsAssetFontFamily(fontName);
			if (isAssetFont)
			{
				return LoadTypefaceFromAsset(fontName);
			}

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
				var (hasFont, fontPath) = _fontRegistrar.HasFont(fontFile.FileNameWithExtension());
				if (hasFont)
				{
					return (true, Typeface.CreateFromFile(fontPath));
				}
			}
			else
			{
				foreach (var ext in FontFile.Extensions)
				{
					var formated = fontFile.FileNameWithExtension(ext);
					var (hasFont, fontPath) = _fontRegistrar.HasFont(formated);
					if (hasFont)
					{
						return (true, Typeface.CreateFromFile(fontPath));
					}

					foreach (var folder in folders)
					{
						formated = $"{folder}{fontFile.FileNameWithExtension()}#{fontFile.PostScriptName}";
						var result = LoadTypefaceFromAsset(formated);
						if (result.success)
							return result;
					}

				}
			}

			return (false, null);
		}

		(bool success, Typeface? typeface) LoadTypefaceFromAsset(string fontfamily)
		{
			try
			{
				var result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(fontfamily));
				return (true, result);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return (false, null);
			}
		}

		bool IsAssetFontFamily(string name)
		{
			return name != null && (name.Contains(".ttf#") || name.Contains(".otf#"));
		}

		Typeface? CreateTypeface((string fontFamily, FontAttributes fontAttributes) familyAttributePair)
		{
			var (fontFamily, fontAttributes) = familyAttributePair;
			fontFamily ??= string.Empty;

			Typeface? result;

			if (string.IsNullOrWhiteSpace(fontFamily))
			{
				var style = ToTypefaceStyle(fontAttributes);
				result = Typeface.Create(Typeface.Default, style);
			}
			else if (IsAssetFontFamily(fontFamily))
			{
				result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(fontFamily));
			}
			else
			{
				fontFamily ??= string.Empty;
				var (success, typeface) = TryGetFromAssets(fontFamily);
				if (success)
				{
					return typeface;
				}
				else
				{
					var style = ToTypefaceStyle(fontAttributes);
					return Typeface.Create(fontFamily, style);
				}
			}

			return result;
		}

		TypefaceStyle ToTypefaceStyle(FontAttributes attrs)
		{
			var style = TypefaceStyle.Normal;
			if ((attrs & (FontAttributes.Bold | FontAttributes.Italic)) == (FontAttributes.Bold | FontAttributes.Italic))
				style = TypefaceStyle.BoldItalic;
			else if ((attrs & FontAttributes.Bold) != 0)
				style = TypefaceStyle.Bold;
			else if ((attrs & FontAttributes.Italic) != 0)
				style = TypefaceStyle.Italic;
			return style;
		}

		string FontNameToFontFile(string fontFamily)
		{
			fontFamily ??= string.Empty;

			int hashtagIndex = fontFamily.IndexOf('#');
			if (hashtagIndex >= 0)
				return fontFamily.Substring(0, hashtagIndex);

			throw new InvalidOperationException($"Can't parse the {nameof(fontFamily)} {fontFamily}");
		}
	}
}