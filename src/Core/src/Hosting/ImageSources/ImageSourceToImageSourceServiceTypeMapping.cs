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

		private readonly object _lock = new();
		private readonly List<(Type ImageSource, Type ImageSourceService)> _typeMapping = new(8); // MAUI registers 8 image source services at startup

		public void Add<TImageSource, TImageSourceService>()
			where TImageSource : IImageSource
			where TImageSourceService : class, IImageSourceService<TImageSource>
		{
			lock (_lock)
			{
				_typeMapping.Add((typeof(TImageSource), typeof(TImageSourceService)));
			}
		}

		public Type FindImageSourceServiceType(Type type)
			=> TryFindImageSourceServiceType(type) ?? throw new InvalidOperationException($"Unable to find any configured {nameof(IImageSourceService)} corresponding to {type}.");

		public Type? TryFindImageSourceServiceType(Type type)
		{
			Debug.Assert(typeof(IImageSource).IsAssignableFrom(type));

			Type? bestImageSource = null;
			Type? bestImageSourceService = null;

			foreach (var (imageSource, imageSourceService) in _typeMapping)
			{
				if (imageSource == type)
				{
					return imageSourceService;
				}

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
				}
			}

			return bestImageSourceService;
		}
	}
}
