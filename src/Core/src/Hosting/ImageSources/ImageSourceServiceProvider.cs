#nullable enable

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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

#if !NETSTANDARD
		[RequiresDynamicCode("The GetImageSourceServiceType method is not AOT compatible. Use GetImageSourceService instead.")]
#endif
		[RequiresUnreferencedCode("The GetImageSourceServiceType method is not trimming compatible. Use GetImageSourceService instead.")]
		public Type GetImageSourceServiceType(Type imageSource)
		{
			return _serviceCache.GetOrAdd(imageSource, CreateImageSourceServiceTypeCacheEntry);

			Type CreateImageSourceServiceTypeCacheEntry(Type type)
			{
				var genericConcreteType = ImageSourceServiceType.MakeGenericType(type);

				if (genericConcreteType != null && InternalCollection.TryGetService(genericConcreteType, out _))
				{
					return genericConcreteType;
				}

				return ImageSourceServiceType.MakeGenericType(GetImageSourceType(type));
			}
		}

		[RequiresUnreferencedCode("The GetImageSourceType method is not trimming compatible. Use GetImageSourceService instead.")]
		public Type GetImageSourceType(Type imageSource)
		{
			return _imageSourceCache.GetOrAdd(imageSource, CreateImageSourceTypeCacheEntry);

			Type CreateImageSourceTypeCacheEntry(Type type)
			{
				if (type.IsInterface)
				{
					if (type.GetInterface(ImageSourceInterface) != null)
					{
						return type;
					}
				}
				else
				{
					foreach (var directInterface in type.GetInterfaces())
					{
						if (directInterface.GetInterface(ImageSourceInterface) != null)
						{
							return directInterface;
						}
					}
				}

				throw new InvalidOperationException($"Unable to find the image source type because none of the interfaces on {type.Name} were derived from {nameof(IImageSource)}.");
			}
		}
	}
}