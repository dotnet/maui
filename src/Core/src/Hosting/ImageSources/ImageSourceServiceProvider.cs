using System;
using System.Collections.Concurrent;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	class ImageSourceServiceProvider : MauiServiceProvider, IImageSourceServiceProvider
	{
		static readonly string ImageSourceInterface = typeof(IImageSource).FullName;
		static readonly Type ImageSourceServiceType = typeof(IImageSourceService<>);

		readonly ConcurrentDictionary<Type, Type> _imageSourceCache = new ConcurrentDictionary<Type, Type>();
		readonly ConcurrentDictionary<Type, Type> _serviceCache = new ConcurrentDictionary<Type, Type>();

		public ImageSourceServiceProvider(IMauiServiceCollection collection)
			: base(collection, false)
		{
		}

		public IImageSourceService? GetImageSourceService(IImageSource imageSource) =>
			(IImageSourceService?)GetService(GetImageSourceServiceType(imageSource));

		public Type GetImageSourceType(IImageSource imageSource) =>
			GetImageSourceType(imageSource.GetType());

		Type GetImageSourceServiceType(IImageSource imageSource) =>
			_serviceCache.GetOrAdd(imageSource.GetType(), type => ImageSourceServiceType.MakeGenericType(GetImageSourceType(type)));

		Type GetImageSourceType(Type imageSourceType) =>
			_imageSourceCache.GetOrAdd(imageSourceType, CreateImageSourceTypeCacheEntry);

		Type CreateImageSourceTypeCacheEntry(Type type)
		{
			foreach (var directInterface in type.GetInterfaces())
			{
				if (directInterface.GetInterface(ImageSourceInterface) != null)
					return directInterface;
			}

			throw new InvalidOperationException($"Unable to find the image source type because none of the interfaces on {type.Name} were derived from {nameof(IImageSource)}.");
		}
	}
}