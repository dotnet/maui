#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Controls
{
	// TODO: Remove this and use the real service provider.
	static class CompatServiceProvider
	{
		static IServiceProvider? _serviceProvider;

		public static IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider has not been initialized");

		public static IFontManager FontManager => ServiceProvider.GetRequiredService<IFontManager>();

		static List<(string filename, string? alias, Assembly assembly)> pendingFonts =
			new List<(string filename, string? alias, Assembly assembly)>();

		public static void SetServiceProvider(IServiceProvider services)
		{
			if (_serviceProvider != null && services != _serviceProvider)
				throw new InvalidOperationException("The service provider can only be set once.");

			_serviceProvider = services;

			lock (pendingFonts)
			{
				foreach (var pendingFont in pendingFonts)
				{
					RegisterFont(pendingFont.filename, pendingFont.alias, pendingFont.assembly);
				}

				pendingFonts.Clear();
			}
		}

		public static void RegisterFont(string filename, string? alias, Assembly assembly)
		{
			if (_serviceProvider == null)
			{
				lock (pendingFonts)
				{
					pendingFonts.Add((filename, alias, assembly));
				}

				return;
			}

			IFontRegistrar _fontRegistrar = ServiceProvider.GetRequiredService<IFontRegistrar>();
			_fontRegistrar.Register(filename, alias, assembly);
		}

	}
}