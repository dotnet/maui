using System;
using System.Collections.Concurrent;
using Android.Graphics;
using Microsoft.Extensions.Logging;
using AApplication = Android.App.Application;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		readonly ConcurrentDictionary<(string fontFamilyName, FontWeight weight, bool italic), Typeface?> _typefaces = new();
		readonly IFontRegistrar _fontRegistrar;
		readonly ILogger<FontManager>? _logger;

		Typeface? _defaultTypeface;

		public FontManager(IFontRegistrar fontRegistrar, ILogger<FontManager>? logger = null)
		{
			_fontRegistrar = fontRegistrar;
			_logger = logger;
		}

		public Typeface DefaultTypeface => _defaultTypeface ??= Typeface.Default!;

		public Typeface? GetTypeface(Font font)
		{
			if (font == Font.Default || (font.Weight == FontWeight.Regular && string.IsNullOrEmpty(font.FontFamily) && font.FontSlant == FontSlant.Default))
				return DefaultTypeface;

			return _typefaces.GetOrAdd((font.FontFamily, font.Weight, font.FontSlant != FontSlant.Default), CreateTypeface);
		}

		public float GetFontSize(Font font, float defaultFontSize = 0) =>
			font.FontSize > 0 ? (float)font.FontSize : (defaultFontSize > 0 ? defaultFontSize : 14f);


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
				_logger?.LogWarning(ex, "Unable to load font '{Font}' from assets.", fontfamily);
				return (false, null);
			}
		}

		bool IsAssetFontFamily(string name)
		{
			return name != null && (name.Contains(".ttf#") || name.Contains(".otf#"));
		}

		Typeface? CreateTypeface((string fontFamilyName, FontWeight weight, bool italic) fontData)
		{
			var (fontFamily, weight, italic) = fontData;
			fontFamily ??= string.Empty;

			Typeface? result;

			if (string.IsNullOrWhiteSpace(fontFamily))
			{
				if (NativeVersion.IsAtLeast(28))
				{
					result = Typeface.Create(Typeface.Default, (int)weight, italic);
				}
				else
				{
					var style = ToTypefaceStyle(weight, italic);
					result = Typeface.Create(Typeface.Default, style);
				}
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
					if (NativeVersion.IsAtLeast(28))
					{
						return Typeface.Create(Typeface.Default, (int)weight, italic);
					}
					else
					{
						var style = ToTypefaceStyle(weight, italic);
						return Typeface.Create(Typeface.Default, style);
					}
				}
			}

			return result;
		}

		TypefaceStyle ToTypefaceStyle(FontWeight weight, bool italic)
		{
			var style = TypefaceStyle.Normal;
			var bold = weight > FontWeight.Bold;
			if (bold && italic)
				style = TypefaceStyle.BoldItalic;
			else if (bold)
				style = TypefaceStyle.Bold;
			else if (italic)
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