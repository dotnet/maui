using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Microsoft.UI.Xaml;
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

		readonly ConcurrentDictionary<string, FontFamily> _fonts = new();
		readonly IFontRegistrar _fontRegistrar;
		readonly IServiceProvider? _serviceProvider;

		/// <remarks>Value is cached to avoid the performance hit of accessing <see cref="ResourceDictionary"/> many times.</remarks>
		FontFamily? _defaultFontFamily;

		/// <remarks>Value is cached to avoid the performance hit of accessing <see cref="ResourceDictionary"/> many times.</remarks>
		double? _defaultFontSize;

		/// <summary>
		/// Creates a new <see cref="EmbeddedFontLoader"/> instance.
		/// </summary>
		/// <param name="fontRegistrar">An <see cref="IFontRegistrar"/> instance for retrieving details about the registered fonts.</param>
		/// <param name="serviceProvider">The applications <see cref="IServiceProvider"/>.
		/// Typically this is provided through dependency injection.</param>
		public FontManager(IFontRegistrar fontRegistrar, IServiceProvider? serviceProvider = null)
		{
			_fontRegistrar = fontRegistrar;
			_serviceProvider = serviceProvider;
		}

		/// <inheritdoc/>
		public FontFamily DefaultFontFamily
		{
			get
			{
				_defaultFontFamily ??= (FontFamily)Application.Current.Resources[SystemFontFamily];
				return _defaultFontFamily;
			}
		}

		/// <inheritdoc/>
		public double DefaultFontSize
		{
			get
			{
				_defaultFontSize ??= (double)Application.Current.Resources[SystemFontSize];
				return _defaultFontSize.Value;
			}
		}

		/// <inheritdoc/>
		public FontFamily GetFontFamily(Font font)
		{
			if (font.IsDefault || string.IsNullOrWhiteSpace(font.Family))
				return DefaultFontFamily;

			return _fonts.GetOrAdd(font.Family, CreateFontFamily);
		}

		/// <inheritdoc/>
		public double GetFontSize(Font font, double defaultFontSize = 0) =>
			font.Size <= 0 || double.IsNaN(font.Size)
				? (defaultFontSize > 0 ? defaultFontSize : DefaultFontSize)
				: font.Size;

		FontFamily CreateFontFamily(string fontFamily)
		{
			var formatted = string.Join(", ", GetAllFontPossibilities(fontFamily));

			var font = new FontFamily(formatted);

			return font;
		}

		IEnumerable<string> GetAllFontPossibilities(string fontFamily)
		{
			// First check Alias
			if (_fontRegistrar.GetFont(fontFamily) is string fontPostScriptName)
			{
				if (fontPostScriptName.Contains("://", StringComparison.Ordinal) && fontPostScriptName.Contains('#', StringComparison.Ordinal))
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
				if (_fontRegistrar.GetFont(fontFile.FileNameWithExtension()) is string filePath)
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
				if (_fontRegistrar.GetFont(fontFile.FileNameWithExtension(ext)) is string filePath)
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

			// Under Native AOT, observed crashes when invoking Win2D CanvasFontSet -> GetPropertyValues
			// This lookup is an optimization; returning null should just cause callers to use the
			// PostScript name already embedded in the file path.
#if USE_NATIVE_AOT
			return null;
#else
			try
			{
				var fontUri = new Uri(fontFile, UriKind.RelativeOrAbsolute);

				// Win2D in unpackaged apps can't load files using packaged schemes, such as `ms-appx://`
				// so we have to first convert it to a `file://` scheme will the full file path.
				// At this part of the load operation, the font URI does NOT yet have the font family name
				// fragment component, so we don't have to remove it.
				if (!AppInfoUtils.IsPackagedApp)
				{
					var path = fontUri.LocalPath.TrimStart('/');
					if (FileSystemUtils.TryGetAppPackageFileUri(path, out var uri))
						fontUri = new Uri(uri, UriKind.RelativeOrAbsolute);
				}

				using (var fontSet = new CanvasFontSet(fontUri))
				{
					if (fontSet.Fonts.Count != 0)
					{
						var props = fontSet.GetPropertyValues(CanvasFontPropertyIdentifier.FamilyName);
						return props.Length == 0 ? null : props[0].Value;
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				// the CanvasFontSet constructor can throw an exception in case something's wrong with the font. It should not crash the app

				_serviceProvider?.CreateLogger<FontManager>()?.LogError(ex, "Error loading font '{Font}'.", fontFile);

				return null;
			}
#endif
		}
	}
}