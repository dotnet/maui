#nullable enable

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	class ImageSourceServiceProvider : MauiFactory, IImageSourceServiceProvider
	{
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		static readonly Type ImageSourceInterfaceType = typeof(IImageSource);
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		static readonly Type ImageSourceServiceType = typeof(IImageSourceService<>);

		readonly ConcurrentDictionary<Type, Type> _imageSourceCache = new ConcurrentDictionary<Type, Type>();
		readonly ConcurrentDictionary<Type, Type> _serviceCache = new ConcurrentDictionary<Type, Type>();

		public ImageSourceServiceProvider(IImageSourceServiceCollection collection, IServiceProvider hostServiceProvider)
			: base(collection, false)
		{
			HostServiceProvider = hostServiceProvider;
		}

		public IServiceProvider HostServiceProvider { get; }

		public IImageSourceService? GetImageSourceService([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type imageSource) =>
			(IImageSourceService?)GetService(GetImageSourceServiceType(imageSource));

		[UnconditionalSuppressMessage("Trimming", "IL2073", Justification = TrimmerHelper.ConcurrentDictionary)]
		[UnconditionalSuppressMessage("Trimming", "IL2111", Justification = TrimmerHelper.MakeGenericType)]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		public Type GetImageSourceServiceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type imageSource) =>
			_serviceCache.GetOrAdd(imageSource, CreateImageSourceServiceType);

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		Type CreateImageSourceServiceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
		{
			var genericConcreteType = MakeGenericType(ImageSourceServiceType, type);

			if (genericConcreteType != null && GetServiceDescriptor(genericConcreteType) != null)
				return genericConcreteType;

			return MakeGenericType(ImageSourceServiceType, GetImageSourceType(type));
		}

		[UnconditionalSuppressMessage("Trimming", "IL2073", Justification = TrimmerHelper.ConcurrentDictionary)]
		[UnconditionalSuppressMessage("Trimming", "IL2111", Justification = TrimmerHelper.MakeGenericType)]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		public Type GetImageSourceType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type imageSource) =>
			_imageSourceCache.GetOrAdd(imageSource, CreateImageSourceTypeCacheEntry);

		[UnconditionalSuppressMessage("Trimming", "IL2065", Justification = "'directInterface.GetInterface(fullName)' can not be statically determined and may not meet 'DynamicallyAccessedMembersAttribute' requirements.")]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		Type CreateImageSourceTypeCacheEntry([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
		{
			var fullName = ImageSourceInterfaceType.FullName;
			if (!string.IsNullOrEmpty(fullName))
			{
				if (type.IsInterface)
				{
					if (type.GetInterface(fullName) != null)
						return type;
				}
				else
				{
					foreach (var directInterface in type.GetInterfaces())
					{
						if (directInterface.GetInterface(fullName) != null)
							return directInterface;
					}
				}
			}

			throw new InvalidOperationException($"Unable to find the image source type because none of the interfaces on {type.Name} were derived from {nameof(IImageSource)}.");
		}

		[UnconditionalSuppressMessage("Trimming", "IL2055", Justification = TrimmerHelper.MakeGenericType)]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		static Type MakeGenericType ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type a, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type b) => a.MakeGenericType(b);
	}
}