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
		static readonly Type EnumerableType = typeof(IEnumerable<>);
		static readonly Type ListType = typeof(List<>);

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
			if (!TryGetServiceDescriptors(ref serviceType, out var single, out var enumerable))
				return default;

			return GetService(serviceType, single, enumerable);
		}

		protected ServiceDescriptor? GetServiceDescriptor(Type serviceType)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var types = GetServiceBaseTypes(serviceType);

			foreach (var type in types)
			{
				if (_collection.TryGetService(type, out var descriptor) && descriptor != null)
					return descriptor;
			}

			return null;
		}

		protected IEnumerable<ServiceDescriptor> GetServiceDescriptors(Type serviceType)
		{
			if (serviceType == null)
				throw new ArgumentNullException(nameof(serviceType));

			var types = GetServiceBaseTypes(serviceType);

			foreach (var type in types)
			{
				foreach (var descriptor in _collection)
				{
					if (descriptor.ServiceType == serviceType)
						yield return descriptor;
				}
			}
		}

		protected bool TryGetServiceDescriptors(ref Type serviceType, out ServiceDescriptor? single, out IEnumerable<ServiceDescriptor>? enumerable)
		{
			// fast path for exact match
			{
				var descriptor = GetServiceDescriptor(serviceType);
				if (descriptor != null)
				{
					single = descriptor;
					enumerable = null;
					return true;
				}
			}

			// try match IEnumerable<TServiceType>
			if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == EnumerableType)
			{
				serviceType = serviceType.GenericTypeArguments[0];
				var descriptors = GetServiceDescriptors(serviceType);

				single = null;
				enumerable = descriptors;
				return true;
			}

			single = null;
			enumerable = null;
			return false;
		}

		static List<Type> GetServiceBaseTypes(Type serviceType)
		{
			var types = new List<Type> { serviceType };

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

			return types;
		}

		object? GetService(ServiceDescriptor descriptor)
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

		object? GetService(Type serviceType, ServiceDescriptor? single, IEnumerable<ServiceDescriptor>? enumerable)
		{
			if (single != null)
				return GetService(single);

			if (enumerable != null)
			{
				var values = (IList)Activator.CreateInstance(ListType.MakeGenericType(serviceType))!;

				foreach (var descriptor in enumerable)
				{
					values.Add(GetService(descriptor));
				}

				if (values.Count > 0)
					return values;
			}
			return default;
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