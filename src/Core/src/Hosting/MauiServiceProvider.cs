using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	class MauiServiceProvider : IMauiServiceProvider
	{
		static readonly Type ServiceProviderType = typeof(IServiceProvider);

		readonly IMauiServiceCollection _collection;
		readonly bool _constructorInjection;

		// TODO: do this properly and support scopes
		readonly IDictionary<ServiceDescriptor, object?> _singletons;

		public MauiServiceProvider(IMauiServiceCollection collection, bool constructorInjection)
		{
			_collection = collection ?? throw new ArgumentNullException(nameof(collection));
			_constructorInjection = constructorInjection;
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
			{
				if (_constructorInjection)
					return CreateInstance(item.ImplementationType);
				else
					return Activator.CreateInstance(item.ImplementationType);
			}

			if (item.ImplementationInstance != null)
				return item.ImplementationInstance;

			if (item.ImplementationFactory != null)
				return item.ImplementationFactory(this);

			throw new InvalidOperationException($"You need to provide an {nameof(item.ImplementationType)}, an {nameof(item.ImplementationFactory)} or an {nameof(item.ImplementationInstance)}.");
		}

		object? CreateInstance(Type implementationType)
		{
			(ConstructorInfo Constructor, ParameterInfo[] Parameters) match = default;
			var constructors = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (var i = 0; i < constructors.Length; i++)
			{
				var ctor = constructors[i];
				if (!ctor.IsFamily && !ctor.IsPrivate)
				{
					var parameters = ctor.GetParameters();
					if (match.Parameters == null || parameters.Length > match.Parameters.Length)
						match = (ctor, parameters);
				}
			}

			if (match.Constructor == null)
				throw new InvalidOperationException($"The type '{implementationType.Name}' did not have any public or internal constructors.");

			var paramCount = match.Parameters!.Length;

			if (paramCount == 0)
				return match.Constructor.Invoke(null);

			var paramValues = new object?[paramCount];

			for (var i = 0; i < paramCount; i++)
			{
				var param = match.Parameters[i];
				var value = GetServiceCore(param.ParameterType);
				if (value == null)
				{
					if (!param.HasDefaultValue)
						throw new InvalidOperationException($"No service for type '{param.ParameterType}' has been registered.");
					else
						value = param.DefaultValue;
				}
				paramValues[i] = value;
			}

			return match.Constructor.Invoke(paramValues);
		}

		object? GetServiceCore(Type type)
		{
			if (ServiceProviderType.IsAssignableFrom(type))
				return this;

			return GetService(type);
		}
	}
}