using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using TypePair = (System.Type ImageSource, System.Type ImageSourceService);

namespace Microsoft.Maui.Hosting
{
	internal sealed class ImageSourceToImageSourceServiceTypeMapping
	{
		private static readonly ConcurrentDictionary<IImageSourceServiceCollection, ImageSourceToImageSourceServiceTypeMapping> s_instances = new();

		internal static ImageSourceToImageSourceServiceTypeMapping GetInstance(IImageSourceServiceCollection collection) =>
			s_instances.GetOrAdd(collection, static _ => new ImageSourceToImageSourceServiceTypeMapping());

		private ConcurrentDictionary<Type, Type> _typeMappings { get; } = new();

		public void Add<TImageSource>() where TImageSource : IImageSource =>
			_typeMappings[typeof(TImageSource)] = typeof(IImageSourceService<TImageSource>);

		public Type FindImageSourceType(Type imageSourceType) =>
			FindImageSourceToImageSourceServiceTypeMapping(imageSourceType).ImageSource;

		public Type FindImageSourceServiceType(Type imageSourceType) =>
			FindImageSourceToImageSourceServiceTypeMapping(imageSourceType).ImageSourceService;

		private TypePair FindImageSourceToImageSourceServiceTypeMapping(Type type)
		{
			Debug.Assert(typeof(IImageSource).IsAssignableFrom(type));

			// If there's an exact match for the type, just return it.
			if (_typeMappings.TryGetValue(type, out var imageSourceService))
			{
				return (type, imageSourceService);
			}

			List<TypePair> matches = new();
			foreach (var mapping in _typeMappings)
			{
				var imageSource = mapping.Key;
				if (imageSource.IsAssignableFrom(type) || type.IsAssignableFrom(imageSource))
				{
					matches.Add((imageSource, mapping.Value));
				}
			}

			return SelectBestMatch(matches, type);
		}

		private static TypePair SelectBestMatch(List<TypePair> matches, Type type)
		{
			if (matches.Count == 0)
			{
				throw new InvalidOperationException($"Unable to find any configured {nameof(IImageSource)} corresponding to {type.Name}.");
			}

			var bestImageSourceMatch = matches[0].ImageSource;
			var bestImageSourceServiceMatch = matches[0].ImageSourceService;

			for (int i = 1; i < matches.Count; i++)
			{
				var (imageSource, imageSourceService) = matches[i];

				if (!bestImageSourceMatch.IsAssignableFrom(imageSource) && !imageSource.IsAssignableFrom(bestImageSourceMatch))
				{
					throw new InvalidOperationException($"Ambiguous image source services for {type} ({bestImageSourceMatch} and {imageSource}).");
				}

				if (bestImageSourceMatch.IsAssignableFrom(imageSource) || (bestImageSourceMatch.IsInterface && imageSource.IsClass))
				{
					bestImageSourceMatch = imageSource;
					bestImageSourceServiceMatch = imageSourceService;
				}
			}

			return (bestImageSourceMatch, bestImageSourceServiceMatch);
		}
	}
}
