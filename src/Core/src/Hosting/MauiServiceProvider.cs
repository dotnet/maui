using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	class MauiServiceProvider : IMauiServiceProvider
	{
		IMauiServiceCollection _collection;

		// TODO: do this properly and support scopes
		IDictionary<ServiceDescriptor, object?> _singletons;

		public MauiServiceProvider(IMauiServiceCollection collection)
		{
			_collection = collection ?? throw new ArgumentNullException(nameof(collection));
			_singletons = new ConcurrentDictionary<ServiceDescriptor, object?>();
		}

		public object? GetService(Type serviceType)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			List<Type> types = new List<Type> { serviceType };

			Type? baseType = serviceType.BaseType;

			while (baseType != null)
			{
				types.Add(baseType);
				baseType = baseType.BaseType;
			}

			foreach (var interfac in serviceType.GetInterfaces())
			{
				if (typeof(IView).IsAssignableFrom(interfac))
					types.Add(interfac);
			}

			foreach (var type in types)
			{
				if (_collection.TryGetService(type, out var descriptor))
				{
					if (descriptor!.Lifetime == ServiceLifetime.Singleton)
					{
						if (_singletons.TryGetValue(descriptor, out var singletonInstance))
							return singletonInstance;
					}

					var typeInstance = CreateInstance(descriptor);

					if (descriptor.Lifetime == ServiceLifetime.Singleton)
					{
						_singletons[descriptor] = typeInstance;
					}

					return typeInstance;
				}
			}

			return default!;
		}

		object? CreateInstance(ServiceDescriptor item)
		{
			if (item.ImplementationType != null)
				return Activator.CreateInstance(item.ImplementationType);

			if (item.ImplementationInstance != null)
				return item.ImplementationInstance;

			if (item.ImplementationFactory != null)
				return item.ImplementationFactory(this);

			throw new InvalidOperationException($"You need to provide an {nameof(item.ImplementationType)}, an {nameof(item.ImplementationFactory)} or an {nameof(item.ImplementationInstance)}.");
		}
	}
}