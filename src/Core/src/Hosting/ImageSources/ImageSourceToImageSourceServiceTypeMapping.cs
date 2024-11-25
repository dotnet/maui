using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Maui.Hosting
{
	internal sealed class ImageSourceToImageSourceServiceTypeMapping
	{
		private static readonly ConcurrentDictionary<IImageSourceServiceCollection, ImageSourceToImageSourceServiceTypeMapping> s_instances = new();

		internal static ImageSourceToImageSourceServiceTypeMapping GetInstance(IImageSourceServiceCollection collection) =>
			s_instances.GetOrAdd(collection, static _ => new ImageSourceToImageSourceServiceTypeMapping());

		private readonly Dictionary<Type, Type> _concreteTypeMapping = new();
		private readonly Dictionary<Type, Type> _interfaceTypeMapping = new();

		public void Add<TImageSource, TImageSourceService>()
			where TImageSource : IImageSource
			where TImageSourceService : class, IImageSourceService<TImageSource>
		{
			if (typeof(TImageSource).IsInterface)
			{
				_interfaceTypeMapping[typeof(TImageSource)] = typeof(TImageSourceService);
			}
			else
			{
				_concreteTypeMapping[typeof(TImageSource)] = typeof(TImageSourceService);
			}
		}

		public Type FindImageSourceServiceType(Type type)
		{
			Debug.Assert(typeof(IImageSource).IsAssignableFrom(type));

			if (_concreteTypeMapping.TryGetValue(type, out var exactImageSourceService)
				|| _interfaceTypeMapping.TryGetValue(type, out exactImageSourceService))
			{
				return exactImageSourceService;
			}

			return FindImageSourceServiceMapping(type, _concreteTypeMapping)
				?? FindImageSourceServiceMapping(type, _interfaceTypeMapping)
				?? throw new InvalidOperationException($"Unable to find a {nameof(IImageSourceService)} corresponding to {type}. Please register a service for {type} using `ImageSourceServiceCollectionExtensions.AddService`");
		}

		private static Type? FindImageSourceServiceMapping(Type type, Dictionary<Type, Type> mapping)
		{
			Type? bestImageSource = null;
			Type? bestImageSourceService = null;

			foreach (var (imageSource, imageSourceService) in mapping)
			{
				if (imageSource.IsAssignableFrom(type))
				{
					if (bestImageSource is null || bestImageSource.IsAssignableFrom(imageSource))
					{
						bestImageSource = imageSource;
						bestImageSourceService = imageSourceService;
					}
					else if (!imageSource.IsAssignableFrom(bestImageSource))
					{
						// This exception can be thrown when the image source implements two interfaces that aren't derived from each other
						// which both have a registered image source service to them.
						// For example, consider this setup:
						//    class MyImageSource : IStreamImageSource, IUriImageSource { ... }
						//    
						//    services.AddService<IStreamImageSource, StreamImageSourceService>();
						//    services.AddService<IUriImageSource, UriImageSourceService>();
						//
						// 	  var imageSourceService = provider.GetImageSourceService(typeof(MyImageSource));
						//    ambiguous match: both StreamImageSourceService and UriImageSourceService are registered for MyImageSource

						throw new InvalidOperationException($"Unable to find a single {nameof(IImageSourceService)} corresponding to {type}. There is an ambiguous match between {bestImageSourceService} ({bestImageSource}) and {imageSourceService} ({imageSource}).");
					}
				}
			}

			return bestImageSourceService;
		}
	}

#if NETSTANDARD
	internal static class KeyValuePairExtensions
	{
		internal static void Deconstruct(this KeyValuePair<Type, Type> pair, out Type key, out Type value)
		{
			key = pair.Key;
			value = pair.Value;
		}
	}
#endif
}
