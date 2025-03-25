#nullable enable
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	class MauiFactory : IMauiFactory
	{
		readonly IMauiServiceCollection _collection;

		protected IMauiServiceCollection InternalCollection => _collection;

		// TODO: do this properly and support scopes
		readonly ConcurrentDictionary<ServiceDescriptor, object?> _singletons;

		public MauiFactory(IMauiServiceCollection collection)
		{
			_collection = collection ?? throw new ArgumentNullException(nameof(collection));
			_singletons = new ConcurrentDictionary<ServiceDescriptor, object?>();

			// to make things easier, just add the provider
			collection.AddSingleton<IServiceProvider>(this);
		}

		public object? GetService(Type serviceType)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			if (!_collection.TryGetService(serviceType, out ServiceDescriptor? descriptor) || descriptor == null)
				return null;

			if (descriptor.Lifetime == ServiceLifetime.Singleton)
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

		object? CreateInstance(ServiceDescriptor item)
		{
			if (item.ImplementationType != null)
			{
				return Activator.CreateInstance(item.ImplementationType);
			}

			if (item.ImplementationInstance != null)
				return item.ImplementationInstance;

			if (item.ImplementationFactory != null)
				return item.ImplementationFactory(this);

			throw new InvalidOperationException($"You need to provide an {nameof(item.ImplementationType)}, an {nameof(item.ImplementationFactory)} or an {nameof(item.ImplementationInstance)}.");
		}
	}
}