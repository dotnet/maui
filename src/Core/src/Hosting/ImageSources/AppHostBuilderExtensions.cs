using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureImageSourceServices(this IAppHostBuilder builder)
		{
			builder.ConfigureImageSourceServices(services =>
			{
				services.AddService<IFileImageSource, FileImageSourceService>();
				services.AddService<IFontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>()));
				services.AddService<IStreamImageSource, StreamImageSourceService>();
				services.AddService<IUriImageSource, UriImageSourceService>();
			});
			return builder;
		}

		public static IAppHostBuilder ConfigureImageSourceServices(this IAppHostBuilder builder, Action<IImageSourceServiceCollection> configureDelegate)
		{
			builder.ConfigureServices<ImageSourceServiceBuilder>((_, services) => configureDelegate(services));
			return builder;
		}

		public static IAppHostBuilder ConfigureImageSourceServices(this IAppHostBuilder builder, Action<HostBuilderContext, IImageSourceServiceCollection> configureDelegate)
		{
			builder.ConfigureServices<ImageSourceServiceBuilder>(configureDelegate);
			return builder;
		}

		class ImageSourceServiceBuilder : MauiServiceCollection, IImageSourceServiceCollection, IMauiServiceBuilder
		{
			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
				services.AddSingleton<IImageSourceServiceProvider>(svcs => new ImageSourceServiceProvider(this, svcs));
			}

			public void Configure(HostBuilderContext context, IServiceProvider services)
			{
			}
		}
	}
}