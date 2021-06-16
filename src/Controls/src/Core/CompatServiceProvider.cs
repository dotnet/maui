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

		// TODO MAUI This create seems wrong
		public static IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider has not been initialized");

		public static IFontRegistrar FontRegistrar => ServiceProvider.GetRequiredService<IFontRegistrar>();

		public static IFontManager FontManager => ServiceProvider.GetRequiredService<IFontManager>();

		public static ILoggerFactory LoggerFactory => ServiceProvider.GetRequiredService<ILoggerFactory>();

		public static void SetFontLoader(Type loaderType)
		{
			if (_embeddedFontLoader != null)
				return;

			_embeddedFontLoader = (IEmbeddedFontLoader)Activator.CreateInstance(loaderType)!;
			if (FontRegistrar is FontRegistrar fr)
				fr.SetFontLoader(_embeddedFontLoader);
		}

		public static void SetServiceProvider(IServiceProvider services)
		{
			if (_serviceProvider != null && services != _serviceProvider)
				throw new InvalidOperationException("The service provider can only be set once.");

			_serviceProvider = services;
			_embeddedFontLoader = _serviceProvider.GetService<IEmbeddedFontLoader>();
		}
	}
}