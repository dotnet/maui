#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	/// <inheritdoc cref="IFontRegistrar"/>
	public partial class FontRegistrar : IFontRegistrar
	{
		readonly Dictionary<string, (string Filename, string? Alias, Assembly Assembly)> _embeddedFonts = new();
		readonly Dictionary<string, (string Filename, string? Alias)> _nativeFonts = new();
		readonly Dictionary<string, string?> _fontLookupCache = new();
		readonly IServiceProvider? _serviceProvider;

		IEmbeddedFontLoader _fontLoader;

		/// <summary>
		/// Creates a new instance of <see cref="FontRegistrar"/>.
		/// </summary>
		/// <param name="fontLoader">An instance of <see cref="IEmbeddedFontLoader"/> that is responsible for actually loading fonts.</param>
		/// <param name="serviceProvider">A reference to the app <see cref="IServiceProvider"/>.
		/// Typically this should be provided through dependency injection for logging purposes, otherwise can be ignored.</param>
		public FontRegistrar(IEmbeddedFontLoader fontLoader, IServiceProvider? serviceProvider = null)
		{
			_fontLoader = fontLoader;
			_serviceProvider = serviceProvider;
		}

		/// <inheritdoc/>
		public void Register(string filename, string? alias, Assembly assembly)
		{
			_embeddedFonts[filename] = (filename, alias, assembly);

			if (!string.IsNullOrWhiteSpace(alias))
				_embeddedFonts[alias!] = (filename, alias, assembly);
		}

		/// <inheritdoc/>
		public void Register(string filename, string? alias)
		{
			_nativeFonts[filename] = (filename, alias);

			if (!string.IsNullOrWhiteSpace(alias))
				_nativeFonts[alias!] = (filename, alias);
		}

		/// <inheritdoc/>
		public string? GetFont(string font)
		{
			if (_fontLookupCache.TryGetValue(font, out var foundResult))
				return foundResult;

			try
			{
				if (_embeddedFonts.TryGetValue(font, out var foundFont))
				{
					using var stream = GetEmbeddedResourceStream(foundFont);

					return LoadEmbeddedFont(font, foundFont.Filename, foundFont.Alias, stream);
				}
				else if (_nativeFonts.TryGetValue(font, out var foundNativeFont))
				{
					return LoadNativeAppFont(font, foundNativeFont.Filename, foundNativeFont.Alias);
				}
			}
			catch (Exception ex)
			{
				_serviceProvider?.CreateLogger<FontRegistrar>()?.LogWarning(ex, "Unable to load font '{Font}'.", font);
			}

			return _fontLookupCache[font] = null;
		}

		string? LoadFileSystemFont(string cacheKey, string filename, string? alias)
		{
			var font = new EmbeddedFont { FontName = filename };

			if (_fontLoader == null)
				throw new InvalidOperationException("Font loader was not set on the font registrar.");

			var result = _fontLoader.LoadFont(font);

			return _fontLookupCache[cacheKey] = result;
		}

		string? LoadEmbeddedFont(string cacheKey, string filename, string? alias, Stream stream)
		{
			var font = new EmbeddedFont { FontName = filename, ResourceStream = stream };

			if (_fontLoader == null)
				throw new InvalidOperationException("Font loader was not set on the font registrar.");

			var result = _fontLoader.LoadFont(font);

			return _fontLookupCache[cacheKey] = result;
		}

		static Stream GetEmbeddedResourceStream((string Filename, string? Alias, Assembly Assembly) embeddedFont)
		{
			var resourceNames = embeddedFont.Assembly.GetManifestResourceNames();
			var searchName = "." + embeddedFont.Filename;

			foreach (var name in resourceNames)
			{
				if (name.EndsWith(searchName, StringComparison.CurrentCultureIgnoreCase))
					return embeddedFont.Assembly.GetManifestResourceStream(name)!;
			}

			throw new FileNotFoundException($"Resource ending with {embeddedFont.Filename} not found.");
		}
	}
}