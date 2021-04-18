using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	public static class ImageSourceServiceCollectionExtensions
	{
		public static IImageSourceServiceCollection AddService<TImageSource, TImageSourceService>(this IImageSourceServiceCollection services)
			where TImageSource : IImageSource
			where TImageSourceService : class, IImageSourceService<TImageSource>
		{
			services.AddSingleton<IImageSourceService<TImageSource>, TImageSourceService>();

			return services;
		}

		public static IImageSourceServiceCollection AddService<TImageSource>(this IImageSourceServiceCollection services, Func<IServiceProvider, IImageSourceService<TImageSource>> implementationFactory)
			where TImageSource : IImageSource
		{
			services.AddSingleton(provider => implementationFactory(((IImageSourceServiceProvider)provider).HostServiceProvider));

			return services;
		}
	}
}