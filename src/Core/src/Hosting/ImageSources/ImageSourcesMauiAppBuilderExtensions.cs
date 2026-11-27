using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static class ImageSourcesMauiAppBuilderExtensions
	{
		public static MauiAppBuilder ConfigureImageSources(this MauiAppBuilder builder)
		{
			builder.ConfigureImageSources(services =>
			{
				services.AddService<IFileImageSource>(svcs => new FileImageSourceService(svcs.CreateLogger<FileImageSourceService>()));
				services.AddService<IFontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
				services.AddService<IStreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
				services.AddService<IUriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>(), GetUriImageLoader(svcs)));
			});
			return builder;
		}

		public static MauiAppBuilder ConfigureImageSources(this MauiAppBuilder builder, Action<IImageSourceServiceCollection>? configureDelegate)
		{
			if (configureDelegate != null)
			{
				builder.Services.AddSingleton<ImageSourceRegistration>(new ImageSourceRegistration(configureDelegate));
			}

			builder.Services.TryAddSingleton<IImageSourceServiceProvider>(svcs => new ImageSourceServiceProvider(svcs.GetRequiredService<IImageSourceServiceCollection>(), svcs));
			builder.Services.TryAddSingleton<IImageSourceServiceCollection>(svcs => new ImageSourceServiceBuilder(svcs.GetServices<ImageSourceRegistration>()));
			AddImageLoading(builder.Services);

			return builder;
		}

		/// <summary>
		/// Configures how remote images (for example a <c>UriImageSource</c>) are downloaded and cached.
		/// </summary>
		/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
		/// <param name="configureDelegate">A delegate that configures the <see cref="ImageLoadingOptions"/>. Can be called multiple times; the delegates run in order.</param>
		/// <returns>The <paramref name="builder"/>, for chaining.</returns>
		/// <remarks>Use the <see cref="IImageCache"/> service to clear downloaded images.</remarks>
		public static MauiAppBuilder ConfigureImageLoading(this MauiAppBuilder builder, Action<ImageLoadingOptions> configureDelegate)
		{
			if (configureDelegate is null)
				throw new ArgumentNullException(nameof(configureDelegate));

			builder.Services.AddSingleton(new ImageLoadingRegistration(configureDelegate));
			AddImageLoading(builder.Services);

			return builder;
		}

		static void AddImageLoading(IServiceCollection services)
		{
			services.TryAddSingleton<UriImageLoader>(svcs =>
			{
				var options = new ImageLoadingOptions();
				foreach (var registration in svcs.GetServices<ImageLoadingRegistration>())
					registration.Configure(options);

				return new UriImageLoader(options, svcs.GetService<ILoggerFactory>()?.CreateLogger<UriImageLoader>());
			});
			services.TryAddSingleton<IImageCache>(svcs => svcs.GetRequiredService<UriImageLoader>());
		}

		internal static UriImageLoader? GetUriImageLoader(IServiceProvider services) =>
			(services as IImageSourceServiceProvider)?.HostServiceProvider.GetService<UriImageLoader>()
				?? services.GetService<UriImageLoader>();

		sealed class ImageLoadingRegistration
		{
			public ImageLoadingRegistration(Action<ImageLoadingOptions> configure) =>
				Configure = configure;

			public Action<ImageLoadingOptions> Configure { get; }
		}

		class ImageSourceRegistration
		{
			private readonly Action<IImageSourceServiceCollection> _registerAction;

			public ImageSourceRegistration(Action<IImageSourceServiceCollection> registerAction)
			{
				_registerAction = registerAction;
			}

			internal void AddRegistration(IImageSourceServiceCollection builder)
			{
				_registerAction(builder);
			}
		}

		class ImageSourceServiceBuilder : MauiServiceCollection, IImageSourceServiceCollection
		{
			public ImageSourceServiceBuilder(IEnumerable<ImageSourceRegistration> registrationActions)
			{
				if (registrationActions != null)
				{
					foreach (var effectRegistration in registrationActions)
					{
						effectRegistration.AddRegistration(this);
					}
				}
			}
		}
	}
}
