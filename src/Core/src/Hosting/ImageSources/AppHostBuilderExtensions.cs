using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureImageSourceServices(this IAppHostBuilder builder)
		{
			builder.ConfigureImageSourceServices(services =>
			{
				services.AddService<IFileImageSource>(svcs => new FileImageSourceService(svcs.GetService<ILogger<FileImageSourceService>>()));
				services.AddService<IFontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.GetService<ILogger<FontImageSourceService>>()));
				services.AddService<IStreamImageSource>(svcs => new StreamImageSourceService(svcs.GetService<ILogger<StreamImageSourceService>>()));
				services.AddService<IUriImageSource>(svcs => new UriImageSourceService(svcs.GetService<ILogger<UriImageSourceService>>()));
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