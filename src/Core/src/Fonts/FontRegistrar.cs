#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public class FontRegistrar : IFontRegistrar
	{
		readonly Dictionary<string, (string Filename, string? Alias, Assembly Assembly)> _embeddedFonts = new();
		readonly Dictionary<string, (string Filename, string? Alias)> _nativeFonts = new();
		readonly Dictionary<string, (bool Success, string? Path)> _fontLookupCache = new();
		readonly ILogger<FontRegistrar>? _logger;

		IEmbeddedFontLoader? _fontLoader;

		public FontRegistrar(IEmbeddedFontLoader? fontLoader = null, ILogger<FontRegistrar>? logger = null)
		{
			_fontLoader = fontLoader;
			_logger = logger;
		}

		public void SetFontLoader(IEmbeddedFontLoader? fontLoader)
		{
			_fontLoader = fontLoader;
		}

		public void Register(string filename, string? alias, Assembly assembly)
		{
			_embeddedFonts[filename] = (filename, alias, assembly);

			if (!string.IsNullOrWhiteSpace(alias))
				_embeddedFonts[alias!] = (filename, alias, assembly);
		}

		public void Register(string filename, string? alias)
		{
			_nativeFonts[filename] = (filename, alias);

			if (!string.IsNullOrWhiteSpace(alias))
				_nativeFonts[alias!] = (filename, alias);
		}

		public (bool hasFont, string? fontPath) HasFont(string font)
		{
			if (_fontLookupCache.TryGetValue(font, out var foundResult))
				return foundResult;

			try
			{
				if (_embeddedFonts.TryGetValue(font, out var foundFont))
				{
					using var stream = GetEmbeddedResourceStream(foundFont);

					return TryLoadFont(font, foundFont.Filename, foundFont.Alias, stream);
				}
				else if (_nativeFonts.TryGetValue(font, out var foundNativeFont))
				{
					if (CanUsePackagedNativeFonts)
					{
						var match = (true, GetNativeFontUri(foundNativeFont));
						_fontLookupCache[font] = match;
						return match;
					}
					else
					{
						using var stream = GetNativeFontStream(foundNativeFont);

						return TryLoadFont(font, foundNativeFont.Filename, foundNativeFont.Alias, stream);
					}
				}
			}
			catch (Exception ex)
			{
				_logger?.LogWarning(ex, "Unable to load font '{Font}'.", font);
			}

			return _fontLookupCache[font] = (false, null);
		}

		(bool hasFont, string? fontPath) TryLoadFont(string cacheKey, string filename, string? alias, Stream stream)
		{
			var font = new EmbeddedFont { FontName = filename, ResourceStream = stream };

			if (_fontLoader == null)
				throw new InvalidOperationException("Font loader was not set on the font registrar.");

			var result = _fontLoader.LoadFont(font);

			return _fontLookupCache[cacheKey] = result;
		}

		Stream GetEmbeddedResourceStream((string Filename, string? Alias, Assembly Assembly) embeddedFont)
		{
			var resourceNames = embeddedFont.Assembly.GetManifestResourceNames();

			var resourcePaths = resourceNames
				.Where(x => x.EndsWith(embeddedFont.Filename, StringComparison.CurrentCultureIgnoreCase))
				.ToArray();

			if (resourcePaths.Length == 0)
				throw new FileNotFoundException($"Resource ending with {embeddedFont.Filename} not found.");

			if (resourcePaths.Length > 1)
				resourcePaths = resourcePaths.Where(x => IsFile(x, embeddedFont.Filename)).ToArray();

			return embeddedFont.Assembly.GetManifestResourceStream(resourcePaths[0])!;
		}

		bool IsFile(string path, string file)
		{
			if (!path.EndsWith(file, StringComparison.Ordinal))
				return false;

			return path.Replace(file, "").EndsWith(".", StringComparison.Ordinal);
		}

		bool CanUsePackagedNativeFonts =>
#if WINDOWS
			true;
#else
			false;
#endif

		string GetNativeFontUri((string Filename, string? Alias) nativeFont)
		{
#if WINDOWS
			var alias = nativeFont.Alias;
			if (!string.IsNullOrEmpty(alias))
				alias = "#" + alias;

			var root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;

			var packagePath = Path.Combine(root, "Assets", nativeFont.Filename);
			if (File.Exists(packagePath))
				return $"ms-appx:///Assets/{nativeFont.Filename}{alias}";

			packagePath = Path.Combine(root, "Fonts", nativeFont.Filename);
			if (File.Exists(packagePath))
				return $"ms-appx:///Fonts/{nativeFont.Filename}{alias}";

			packagePath = Path.Combine(root, "Assets", "Fonts", nativeFont.Filename);
			if (File.Exists(packagePath))
				return $"ms-appx:///Assets/Fonts/{nativeFont.Filename}{alias}";

			// TODO: check other folders as well
#endif

			throw new FileNotFoundException($"Native font with the name {nativeFont.Filename} was not found.");
		}

		Stream GetNativeFontStream((string Filename, string? Alias) nativeFont)
		{
#if __IOS__ || IOS
			var mainBundlePath = Foundation.NSBundle.MainBundle.BundlePath;

			var fontBundlePath = Path.Combine(mainBundlePath, nativeFont.Filename);
			if (File.Exists(fontBundlePath))
				return File.OpenRead(fontBundlePath);

			fontBundlePath = Path.Combine(mainBundlePath, "Resources", nativeFont.Filename);
			if (File.Exists(fontBundlePath))
				return File.OpenRead(fontBundlePath);

			fontBundlePath = Path.Combine(mainBundlePath, "Fonts", nativeFont.Filename);
			if (File.Exists(fontBundlePath))
				return File.OpenRead(fontBundlePath);

			fontBundlePath = Path.Combine(mainBundlePath, "Resources", "Fonts", nativeFont.Filename);
			if (File.Exists(fontBundlePath))
				return File.OpenRead(fontBundlePath);
#elif __ANDROID__ || ANDROID
			var assets = Android.App.Application.Context.Assets;

			if (assets != null && assets.Open(nativeFont.Filename) is Stream assetStream)
				return assetStream;

			// TODO: check other folders as well
#endif

			throw new FileNotFoundException($"Native font with the name {nativeFont.Filename} was not found.");
		}
	}
}