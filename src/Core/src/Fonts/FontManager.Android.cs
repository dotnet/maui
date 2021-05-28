using System;
using System.Collections.Concurrent;
using Android.Graphics;
using Microsoft.Extensions.Logging;
using AApplication = Android.App.Application;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		static readonly string[] FontFolders = new[]
		{
			"",
			"Fonts/",
			"fonts/",
		};

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
			font.FontSize <= 0
				? (defaultFontSize > 0 ? defaultFontSize : 14f)
				: (float)font.FontSize;


		Typeface? GetFromAssets(string fontName)
		{
			// First check Alias
			if (_fontRegistrar.GetFont(fontName) is string fontPostScriptName)
				return Typeface.CreateFromFile(fontPostScriptName);

			var isAssetFont = IsAssetFontFamily(fontName);
			if (isAssetFont)
				return LoadTypefaceFromAsset(fontName);

			// copied text
			var fontFile = FontFile.FromString(fontName);

			if (!string.IsNullOrWhiteSpace(fontFile.Extension))
			{
				if (_fontRegistrar.GetFont(fontFile.FileNameWithExtension()) is string fontPath)
					return Typeface.CreateFromFile(fontPath);
			}
			else
			{
				foreach (var ext in FontFile.Extensions)
				{
					var formatted = fontFile.FileNameWithExtension(ext);
					if (_fontRegistrar.GetFont(formatted) is string fontPath)
						return Typeface.CreateFromFile(fontPath);

					foreach (var folder in FontFolders)
					{
						formatted = $"{folder}{fontFile.FileNameWithExtension()}#{fontFile.PostScriptName}";
						var result = LoadTypefaceFromAsset(formatted, false);
						if (result != null)
							return result;
					}
				}
			}

			return null;
		}

		Typeface? LoadTypefaceFromAsset(string fontfamily, bool warning = true)
		{
			try
			{
				return Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(fontfamily));
			}
			catch (Exception ex)
			{
				if (warning)
					_logger?.LogWarning(ex, "Unable to load font '{Font}' from assets.", fontfamily);
			}

			return null;
		}

		bool IsAssetFontFamily(string name)
		{
			return name != null && (name.Contains(".ttf#") || name.Contains(".otf#"));
		}

		Typeface? CreateTypeface((string fontFamilyName, FontWeight weight, bool italic) fontData)
		{
			var (fontFamily, weight, italic) = fontData;
			fontFamily ??= string.Empty;
			var style = ToTypefaceStyle(weight, italic);

			var result = Typeface.Default;

			if (!string.IsNullOrWhiteSpace(fontFamily))
			{
				if (IsAssetFontFamily(fontFamily))
				{
					result = Typeface.CreateFromAsset(AApplication.Context.Assets, FontNameToFontFile(fontFamily));
				}
				else
				{
					if (GetFromAssets(fontFamily) is Typeface typeface)
						result = typeface;
					else
						result = Typeface.Create(fontFamily, style);
				}
			}

			if (NativeVersion.IsAtLeast(28))
				result = Typeface.Create(result, (int)weight, italic);
			else
				result = Typeface.Create(result, style);

			return result;
		}

		TypefaceStyle ToTypefaceStyle(FontWeight weight, bool italic)
		{
			var style = TypefaceStyle.Normal;
			var bold = weight >= FontWeight.Bold;
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