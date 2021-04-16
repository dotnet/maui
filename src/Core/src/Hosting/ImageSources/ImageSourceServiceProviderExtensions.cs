using System;

namespace Microsoft.Maui
{
	public static class ImageSourceServiceProviderExtensions
	{
		public static IImageSourceService GetRequiredImageSourceService(this IImageSourceServiceProvider provider, IImageSource imageSource) =>
			provider.GetImageSourceService(imageSource)
				?? throw new InvalidOperationException($"Unable to find a image source service for {provider.GetImageSourceType(imageSource)}.");
	}
}