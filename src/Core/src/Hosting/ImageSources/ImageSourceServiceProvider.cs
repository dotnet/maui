#nullable enable

using System;
using System.Collections.Concurrent;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	sealed class ImageSourceServiceProvider : MauiFactory, IImageSourceServiceProvider
	{
		static readonly string ImageSourceInterface = typeof(IImageSource).FullName!;
		static readonly Type ImageSourceServiceType = typeof(IImageSourceService<>);

		readonly ConcurrentDictionary<Type, Type> _imageSourceCache = new ConcurrentDictionary<Type, Type>();
		readonly ConcurrentDictionary<Type, Type> _serviceCache = new ConcurrentDictionary<Type, Type>();

		readonly ImageSourceToImageSourceServiceTypeMapping _imageSourceMapping;

		public ImageSourceServiceProvider(IImageSourceServiceCollection collection, IServiceProvider hostServiceProvider)
			: base(collection)
		{
			_imageSourceMapping = ImageSourceToImageSourceServiceTypeMapping.GetInstance(collection);
			HostServiceProvider = hostServiceProvider;
		}

		public IServiceProvider HostServiceProvider { get; }

		public IImageSourceService? GetImageSourceService(Type imageSource)
		{
			var imageSourceService = _serviceCache.GetOrAdd(imageSource, _imageSourceMapping.FindImageSourceServiceType);
			return (IImageSourceService?)GetService(imageSourceService);
		}

		public Type GetImageSourceServiceType(Type imageSource) =>
			_serviceCache.GetOrAdd(imageSource, type =>
			{
				var genericConcreteType = ImageSourceServiceType.MakeGenericType(type);

				if (genericConcreteType != null && GetServiceDescriptor(genericConcreteType) != null)
					return genericConcreteType;

				return ImageSourceServiceType.MakeGenericType(GetImageSourceType(type));
			});

		public Type GetImageSourceType(Type imageSource) =>
			_imageSourceCache.GetOrAdd(imageSource, CreateImageSourceTypeCacheEntry);

		Type CreateImageSourceTypeCacheEntry(Type type)
		{
			if (type.IsInterface)
			{
				if (type.GetInterface(ImageSourceInterface) != null)
					return type;
			}
			else
			{
				foreach (var directInterface in type.GetInterfaces())
				{
					if (directInterface.GetInterface(ImageSourceInterface) != null)
						return directInterface;
				}
			}

			throw new InvalidOperationException($"Unable to find the image source type because none of the interfaces on {type.Name} were derived from {nameof(IImageSource)}.");
		}
	}
}