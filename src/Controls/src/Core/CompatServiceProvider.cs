#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Controls
{
	// TODO: Remove this and use the real service provider.
	static class CompatServiceProvider
	{
		static IServiceProvider? _serviceProvider;

		static IEmbeddedFontLoader? _embeddedFontLoader;
		static IFontRegistrar? _fontRegistrar;
		static IFontManager? _fontManager;
		static ILoggerFactory? _loggerFactory;

		public static IFontRegistrar FontRegistrar => _fontRegistrar ??= new FontRegistrar(_embeddedFontLoader);

		public static IFontManager FontManager => _fontManager ??= new FontManager(FontRegistrar);

		public static ILoggerFactory LoggerFactory => _loggerFactory ??= new FallbackLoggerFactory();

		public static void SetFontLoader(IEmbeddedFontLoader embeddedFontLoader)
		{
			_embeddedFontLoader = embeddedFontLoader;
			if (FontRegistrar is FontRegistrar fr)
				fr.SetFontLoader(embeddedFontLoader);
		}

		public static void SetServiceProvider(IServiceProvider services)
		{
			if (_serviceProvider != null)
				throw new InvalidOperationException("The service provider can only be set once.");

			_serviceProvider = services;

			Set(ref _embeddedFontLoader);
			Set(ref _fontRegistrar);
			Set(ref _fontManager);
			Set(ref _loggerFactory);
		}

		static void Set<T>(ref T? field)
		{
			if (_serviceProvider == null)
				throw new InvalidOperationException($"Service provider was not set.");

			if (field != null)
				throw new InvalidOperationException($"Service '{typeof(T).Name}' was already set.");

			field = _serviceProvider.GetService<T>();
		}
	}
}