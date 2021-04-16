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
	}
}