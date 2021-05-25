#nullable enable

using System;

namespace Microsoft.Maui
{
	public static class ImageSourceServiceProviderExtensions
	{
		public static IImageSourceService? GetImageSourceService(this IImageSourceServiceProvider provider, IImageSource imageSource) =>
			provider.GetImageSourceService(imageSource.GetType());

		public static IImageSourceService? GetImageSourceService<T>(this IImageSourceServiceProvider provider)
			where T : IImageSource =>
			provider.GetImageSourceService(typeof(T));

		public static IImageSourceService GetRequiredImageSourceService(this IImageSourceServiceProvider provider, IImageSource imageSource) =>
			provider.GetRequiredImageSourceService(imageSource.GetType());

		public static IImageSourceService GetRequiredImageSourceService<T>(this IImageSourceServiceProvider provider)
			where T : IImageSource =>
			provider.GetRequiredImageSourceService(typeof(T));

		public static IImageSourceService GetRequiredImageSourceService(this IImageSourceServiceProvider provider, Type imageSourceType)
		{
			var service = provider.GetImageSourceService(imageSourceType);
			if (service != null)
				return service;

			throw new InvalidOperationException($"Unable to find a image source service for {provider.GetImageSourceType(imageSourceType)}.");
		}
	}
}