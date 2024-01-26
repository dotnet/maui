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

		private readonly Dictionary<Type, Type> _typeMapping = new(8); // MAUI registers 8 image source services at startup

		public void Add<TImageSource, TImageSourceService>()
			where TImageSource : IImageSource
			where TImageSourceService : class, IImageSourceService<TImageSource>
		{
			_typeMapping[typeof(TImageSource)] = typeof(TImageSourceService);
		}

		public Type FindImageSourceServiceType(Type type)
		{
			Debug.Assert(typeof(IImageSource).IsAssignableFrom(type));

			if (_typeMapping.TryGetValue(type, out var exactImageSourceService))
			{
				return exactImageSourceService;
			}

			Type? bestImageSource = null;
			Type? bestImageSourceService = null;

			foreach (var (imageSource, imageSourceService) in _typeMapping)
			{
				if (imageSource.IsAssignableFrom(type))
				{
					if (bestImageSource is null)
					{
						bestImageSource = imageSource;
						bestImageSourceService = imageSourceService;
					}
					else if (bestImageSource.IsAssignableFrom(imageSource))
					{
						bestImageSource = imageSource;
						bestImageSourceService = imageSourceService;
					}
					else if (!imageSource.IsAssignableFrom(bestImageSource))
					{
						throw new InvalidOperationException($"Unable to find a single {nameof(IImageSourceService)} corresponding to {type}. There is an ambiguous match between {bestImageSourceService} ({bestImageSource}) and {imageSourceService} ({imageSource}).");
					}
				}
			}

			if (bestImageSourceService is null)
			{
				throw new InvalidOperationException($"Unable to find any configured {nameof(IImageSourceService)} corresponding to {type}.");
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
