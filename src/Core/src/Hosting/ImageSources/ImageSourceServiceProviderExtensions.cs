#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui
{
	public static class ImageSourceServiceProviderExtensions
	{
		public static IImageSourceService? GetImageSourceService(this IImageSourceServiceProvider provider, IImageSource imageSource) =>
			provider.GetImageSourceService(TrimmerHelper.GetType(imageSource));

		public static IImageSourceService? GetImageSourceService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(this IImageSourceServiceProvider provider)
			where T : IImageSource =>
			provider.GetImageSourceService(typeof(T));

		public static IImageSourceService GetRequiredImageSourceService(this IImageSourceServiceProvider provider, IImageSource imageSource) =>
			provider.GetRequiredImageSourceService(TrimmerHelper.GetType(imageSource));

		public static IImageSourceService GetRequiredImageSourceService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] T>(this IImageSourceServiceProvider provider)
			where T : IImageSource =>
			provider.GetRequiredImageSourceService(typeof(T));

		public static IImageSourceService GetRequiredImageSourceService(this IImageSourceServiceProvider provider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type imageSourceType)
		{
			var service = provider.GetImageSourceService(imageSourceType);
			if (service != null)
				return service;

			throw new InvalidOperationException($"Unable to find a image source service for {provider.GetImageSourceType(imageSourceType)}.");
		}
	}
}