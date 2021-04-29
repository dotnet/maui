using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureImageSources(this IAppHostBuilder builder)
		{
			builder.ConfigureImageSources(services =>
			{
				services.AddService<IFileImageSource>(svcs => new FileImageSourceService(svcs.GetService<IImageSourceServiceConfiguration>(), svcs.CreateLogger<FileImageSourceService>()));
				services.AddService<IFontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
				services.AddService<IStreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
				services.AddService<IUriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
			});
			return builder;
		}

		public static IAppHostBuilder ConfigureImageSources(this IAppHostBuilder builder, Action<IImageSourceServiceCollection> configureDelegate)
		{
			builder.ConfigureServices<ImageSourceServiceBuilder>((_, services) => configureDelegate(services));
			return builder;
		}

		public static IAppHostBuilder ConfigureImageSources(this IAppHostBuilder builder, Action<HostBuilderContext, IImageSourceServiceCollection> configureDelegate)
		{
			builder.ConfigureServices<ImageSourceServiceBuilder>(configureDelegate);
			return builder;
		}

		class ImageSourceServiceBuilder : MauiServiceCollection, IImageSourceServiceCollection, IMauiServiceBuilder
		{
			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
				services.AddSingleton<IImageSourceServiceConfiguration, ImageSourceServiceConfiguration>();
				services.AddSingleton<IImageSourceServiceProvider>(svcs => new ImageSourceServiceProvider(this, svcs));
			}

			public void Configure(HostBuilderContext context, IServiceProvider services)
			{
			}
		}
	}
}