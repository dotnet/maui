using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	public class FontManager : IFontManager
	{
		const string SystemFontFamily = "ContentControlThemeFontFamily";
		const string SystemFontSize = "ControlContentThemeFontSize";

		const string TypicalFontAssetsPath = "Assets/Fonts/";
		static readonly string[] TypicalFontFileExtensions = new[]
		{
			".ttf",
			".otf",
		};

		readonly ConcurrentDictionary<string, FontFamily> _fonts =
			new ConcurrentDictionary<string, FontFamily>();

		readonly IFontRegistrar _fontRegistrar;

		public FontManager(IFontRegistrar fontRegistrar)
		{
			_fontRegistrar = fontRegistrar;
		}

		public FontFamily DefaultFontFamily =>
			(FontFamily)MauiWinUIApplication.Current.Resources[SystemFontFamily];

		public double DefaultFontSize =>
			(double)MauiWinUIApplication.Current.Resources[SystemFontSize];

		public FontFamily GetFontFamily(Font font)
		{
			if (font.IsDefault || string.IsNullOrWhiteSpace(font.FontFamily))
				return DefaultFontFamily;

			return _fonts.GetOrAdd(font.FontFamily, CreateFontFamily);
		}

		public double GetFontSize(Font font)
		{
			if (font.UseNamedSize)
				return GetFontSize(font.NamedSize);

			return font.FontSize;
		}

		public double GetFontSize(NamedSize namedSize)
		{
			// TODO: Hmm, maybe we need to revisit this, since we no longer support Windows Phone OR WinRT.
			// These are values pulled from the mapped sizes on Windows Phone, WinRT has no equivalent sizes, only intents.

			return namedSize switch
			{
				NamedSize.Default => DefaultFontSize,
				NamedSize.Micro => 15.667,
				NamedSize.Small => 18.667,
				NamedSize.Medium => 22.667,
				NamedSize.Large => 32,
				NamedSize.Body => 14,
				NamedSize.Caption => 12,
				NamedSize.Header => 46,
				NamedSize.Subtitle => 20,
				NamedSize.Title => 24,
				_ => throw new ArgumentOutOfRangeException(nameof(namedSize)),
			};
		}

		FontFamily CreateFontFamily(string fontFamily)
		{
			var formatted = string.Join(", ", GetAllFontPossibilities(fontFamily));

			var font = new FontFamily(formatted);

			return font;
		}

		IEnumerable<string> GetAllFontPossibilities(string fontFamily)
		{
			// First check Alias
			if (_fontRegistrar.TryGetFont(fontFamily, out var fontPostScriptName))
			{
				if (fontPostScriptName!.Contains("://") && fontPostScriptName.Contains("#"))
				{
					// The registrar has given us a perfect path, so use it exactly
					yield return fontPostScriptName;
				}
				else
				{
					var familyName = FindFontFamilyName(fontPostScriptName);
					var file = FontFile.FromString(Path.GetFileName(fontPostScriptName));
					var formatted = $"{fontPostScriptName}#{familyName ?? file.GetPostScriptNameWithSpaces()}";

					yield return formatted;
				}
				yield break;
			}

			var fontFile = FontFile.FromString(fontFamily);

			// If the extension is provided, they know what they want!
			var hasExtension = !string.IsNullOrWhiteSpace(fontFile.Extension);
			if (hasExtension)
			{
				var (hasFont, filePath) = _fontRegistrar.HasFont(fontFile.FileNameWithExtension());
				if (hasFont)
				{
					var familyName = FindFontFamilyName(filePath);
					var formatted = $"{filePath}#{familyName ?? fontFile.GetPostScriptNameWithSpaces()}";

					yield return formatted;
					yield break;
				}
				else
				{
					yield return $"{TypicalFontAssetsPath}{fontFile.FileNameWithExtension()}";
				}
			}

			// There was no extension so let's just try a few things
			foreach (var ext in TypicalFontFileExtensions)
			{
				var (hasFont, filePath) = _fontRegistrar.HasFont(fontFile.FileNameWithExtension(ext));
				if (hasFont)
				{
					var familyName = FindFontFamilyName(filePath);
					var formatted = $"{filePath}#{familyName ?? fontFile.GetPostScriptNameWithSpaces()}";

					yield return formatted;
					yield break;
				}
			}

			// Always send the base back
			yield return fontFamily;

			// And then just wing it with each extension
			foreach (var ext in TypicalFontFileExtensions)
			{
				var fileName = $"{TypicalFontAssetsPath}{fontFile.FileNameWithExtension(ext)}";
				var familyName = FindFontFamilyName(fileName);
				var formatted = $"{fileName}#{familyName ?? fontFile.GetPostScriptNameWithSpaces()}";

				yield return formatted;
			}
		}

		string? FindFontFamilyName(string? fontFile)
		{
			if (fontFile == null)
				return null;

			try
			{
				var fontUri = new Uri(fontFile, UriKind.RelativeOrAbsolute);

				// CanvasFontSet only supports ms-appx:// and ms-appdata:// font URIs
				if (fontUri.IsAbsoluteUri && (fontUri.Scheme == "ms-appx" || fontUri.Scheme == "ms-appdata"))
				{
					using (var fontSet = new CanvasFontSet(fontUri))
					{
						if (fontSet.Fonts.Count != 0)
							return fontSet.GetPropertyValues(CanvasFontPropertyIdentifier.FamilyName).FirstOrDefault().Value;
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				// the CanvasFontSet constructor can throw an exception in case something's wrong with the font. It should not crash the app
				Debug.WriteLine("Font", $"Error loading font {fontFile}: {ex.Message}");

				return null;
			}
		}
	}
}