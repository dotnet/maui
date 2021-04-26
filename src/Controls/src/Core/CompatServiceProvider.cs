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

		public static IServiceProvider ServiceProvider => _serviceProvider ??= CreateCompatServiceProvider();

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
			if (_serviceProvider != null)
				throw new InvalidOperationException("The service provider can only be set once.");

			_serviceProvider = services;
			_embeddedFontLoader = _serviceProvider.GetService<IEmbeddedFontLoader>();
		}

		static IServiceProvider CreateCompatServiceProvider()
		{
			var collection = new MauiServiceCollection();

			collection.AddSingleton<ILoggerFactory, FallbackLoggerFactory>();
			collection.AddSingleton<IFontRegistrar>(svc => new FontRegistrar(_embeddedFontLoader, svc.CreateLogger<FontRegistrar>()));
			collection.AddSingleton<IFontManager>(svc => new FontManager(svc.GetRequiredService<IFontRegistrar>(), svc.CreateLogger<FontManager>()));

			var provider = new MauiServiceProvider(collection, false);

			return provider;
		}
	}
}