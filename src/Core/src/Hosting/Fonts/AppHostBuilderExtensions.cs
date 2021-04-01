using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureFonts(this IAppHostBuilder builder)
		{
			builder.ConfigureServices<FontCollectionBuilder>((a, b) => { });
			return builder;
		}

		public static IAppHostBuilder ConfigureFonts(this IAppHostBuilder builder, Action<IFontCollection> configureDelegate)
		{
			builder.ConfigureServices<FontCollectionBuilder>((_, fonts) => configureDelegate(fonts));
			return builder;
		}

		public static IAppHostBuilder ConfigureFonts(this IAppHostBuilder builder, Action<HostBuilderContext, IFontCollection> configureDelegate)
		{
			builder.ConfigureServices<FontCollectionBuilder>(configureDelegate);
			return builder;
		}

		class FontCollectionBuilder : FontCollection, IMauiServiceBuilder
		{
			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
				services.AddSingleton<IEmbeddedFontLoader, EmbeddedFontLoader>();
				services.AddSingleton<IFontRegistrar>(provider => new FontRegistrar(provider.GetRequiredService<IEmbeddedFontLoader>()));
				services.AddSingleton<IFontManager>(provider => new FontManager(provider.GetRequiredService<IFontRegistrar>()));
			}

			public void Configure(HostBuilderContext context, IServiceProvider services)
			{
				var fontRegistrar = services.GetService<IFontRegistrar>();
				if (fontRegistrar == null)
					return;

				foreach (var font in this)
				{
					if (font.Assembly == null)
						fontRegistrar.Register(font.Filename, font.Alias);
					else
						fontRegistrar.Register(font.Filename, font.Alias, font.Assembly);
				}
			}
		}
	}
}