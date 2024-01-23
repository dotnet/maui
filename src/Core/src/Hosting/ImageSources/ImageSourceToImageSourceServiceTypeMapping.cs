using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Hosting
{
	internal sealed class ImageSourceToImageSourceServiceTypeMapping
	{
		private static readonly ConcurrentDictionary<IImageSourceServiceCollection, ImageSourceToImageSourceServiceTypeMapping> s_instances = new();

		internal static ImageSourceToImageSourceServiceTypeMapping GetInstance(IImageSourceServiceCollection collection) =>
			s_instances.GetOrAdd(collection, static _ => new ImageSourceToImageSourceServiceTypeMapping());

		private readonly object _lock = new();
		private Dictionary<Type, Type> _interfaceMapping = new();
		private Dictionary<Type, Type> _directServiceMapping = new();
		private Dictionary<Type, Type> _fallbackServiceMapping = new();

		public void Add<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TImageSource>() where TImageSource : IImageSource
		{
			var imageSourceInterfaceType = GetImageSourceInterface(typeof(TImageSource));
			var fallbackImageSourceServiceType = MakeGenericImageSourceServiceType(imageSourceInterfaceType);

			lock (_lock)
			{
				_directServiceMapping[typeof(TImageSource)] = typeof(IImageSourceService<TImageSource>);
				_interfaceMapping[typeof(TImageSource)] = imageSourceInterfaceType;
				_fallbackServiceMapping[imageSourceInterfaceType] = fallbackImageSourceServiceType;
			}

			[UnconditionalSuppressMessage("AotAnalysis", "IL3050:RequiresDynamicCode",
				Justification = "The type which is used as a generic parameter is not a value type.")]
			Type MakeGenericImageSourceServiceType(Type imageSourceInterfaceType)
			{
				if (imageSourceInterfaceType.IsValueType)
				{
					throw new InvalidOperationException($"Unable to register {typeof(TImageSource)} as an {typeof(IImageSource)} because it is a value type.");
				}

				return typeof(IImageSourceService<>).MakeGenericType(imageSourceInterfaceType);
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
