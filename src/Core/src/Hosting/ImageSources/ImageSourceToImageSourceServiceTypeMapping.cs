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

		private ConcurrentDictionary<Type, Type> _interfaceMapping { get; } = new();
		private ConcurrentDictionary<Type, Type> _directServiceMapping { get; } = new();
		private ConcurrentDictionary<Type, Type> _fallbackServiceMapping { get; } = new();

		public void Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TImageSource>() where TImageSource : IImageSource
		{
			// override any previous mapping (possibly a default mapping)
			_directServiceMapping[typeof(TImageSource)] = typeof(IImageSourceService<TImageSource>);

			var imageSourceInterfaceType = GetImageSourceInterface(typeof(TImageSource));
			_interfaceMapping[typeof(TImageSource)] = imageSourceInterfaceType;

			// add a fallback value for the interface to mimic the behavior of the old implementation
			AddFallbackMapping();

			[UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
				Justification = "The type which is used as a generic parameter is not a value type.")]
			void AddFallbackMapping()
			{
				if (imageSourceInterfaceType.IsValueType)
				{
					throw new InvalidOperationException($"Unable to register {typeof(TImageSource)} as an {typeof(IImageSource)} because it is a value type.");
				}

				var imageSourceServiceType = typeof(IImageSourceService<>).MakeGenericType(imageSourceInterfaceType);
				_fallbackServiceMapping[imageSourceInterfaceType] = imageSourceServiceType;
			}
		}

		public Type FindImageSourceType(Type imageSourceType)
		{
			Debug.Assert(typeof(IImageSource).IsAssignableFrom(imageSourceType));

			return TryFindImageSourceInterface(imageSourceType)
				?? throw new InvalidOperationException($"Unable to find any configured {nameof(IImageSource)} corresponding to {imageSourceType.Name}.");
		}

		public Type FindImageSourceServiceType(Type type)
		{
			Debug.Assert(typeof(IImageSource).IsAssignableFrom(type));

			if (_directServiceMapping.TryGetValue(type, out var imageSourceService))
			{
				return imageSourceService;
			}

			if (TryFindImageSourceInterface(type) is Type imageSourceInterfaceType
				&& _fallbackServiceMapping.TryGetValue(imageSourceInterfaceType, out var imageSourceServiceType)
				&& imageSourceServiceType is not null)
			{
				return imageSourceServiceType;
			}

			throw new InvalidOperationException($"Unable to find any configured {typeof(IImageSourceService)} corresponding to {type.Name}.");
		}

		private Type? TryFindImageSourceInterface(Type imageSourceType)
		{
			if (_interfaceMapping.TryGetValue(imageSourceType, out var imageSourceInterfaceType))
			{
				return imageSourceInterfaceType;
			}

			foreach (var iface in _interfaceMapping.Values)
			{
				if (iface.IsAssignableFrom(imageSourceType))
				{
					return iface;
				}
			}

			return null;
		}

		private static Type GetImageSourceInterface([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
		{
			if (type.IsInterface)
			{
				if (typeof(IImageSource).IsAssignableFrom(type))
					return type;
			}
			else
			{
				foreach (var directInterface in type.GetInterfaces())
				{
					if (directInterface != typeof(IImageSource) && typeof(IImageSource).IsAssignableFrom(directInterface))
						return directInterface;
				}
			}

			throw new InvalidOperationException($"Unable to find the image source type because none of the interfaces on {type.Name} were derived from {nameof(IImageSource)}.");
		}
	}
}
