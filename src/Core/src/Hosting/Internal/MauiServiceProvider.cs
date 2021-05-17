#nullable enable
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	class MauiServiceProvider : IMauiServiceProvider
	{
		static readonly Type ServiceProviderType = typeof(IServiceProvider);
		static readonly Type EnumerableType = typeof(IEnumerable<>);
		static readonly Type ListType = typeof(List<>);

		readonly IMauiServiceCollection _collection;
		readonly bool _constructorInjection;

		// TODO: do this properly and support scopes
		readonly IDictionary<ServiceDescriptor, object?> _singletons;

		public MauiServiceProvider(IMauiServiceCollection collection, bool constructorInjection)
		{
			_collection = collection ?? throw new ArgumentNullException(nameof(collection));
			_constructorInjection = constructorInjection;
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

		bool TryGetServiceDescriptors(ref Type serviceType, out ServiceDescriptor? single, out IEnumerable<ServiceDescriptor>? enumerable)
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
			// get constructors ordered by parameter count
			var constructors = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var matches = new (ConstructorInfo? Constructor, ParameterInfo[]? Parameters)[constructors.Length];
			var matchesCounts = new int[constructors.Length];
			var found = false;
			for (var i = 0; i < constructors.Length; i++)
			{
				var ctor = constructors[i];
				if (!ctor.IsFamily && !ctor.IsPrivate)
				{
					var parameters = ctor.GetParameters();
					matchesCounts[i] = parameters.Length;
					matches[i] = (ctor, parameters);
					found = true;
				}
			}
			Array.Sort(matchesCounts, matches);

			if (!found)
				throw new InvalidOperationException($"The type '{implementationType.Name}' did not have any public or internal constructors.");

			// go through in reverse order
			for (var m = matches.Length - 1; m >= 0; m--)
			{
				var (constructor, parameters) = matches[m];
				if (constructor != null && parameters != null)
				{
					var paramCount = parameters.Length;

					// we are at a ctor that has no params, so just use that
					if (paramCount == 0)
						return constructor.Invoke(null);

					// try find a ctor that matches what we have in the service collection
					var validConstructor = true;
					var paramDescriptors = new (Type ServiceType, ServiceDescriptor? Single, IEnumerable<ServiceDescriptor>? Enumerable, object? Value)[paramCount];
					for (var i = 0; i < paramCount; i++)
					{
						var param = parameters[i];
						var paramType = param.ParameterType;

						var isValid = TryGetServiceDescriptors(ref paramType, out var single, out var enumerable);
						if (isValid)
							paramDescriptors[i] = (paramType, single, enumerable, null);
						else if (param.HasDefaultValue)
							paramDescriptors[i] = (paramType, null, null, param.DefaultValue);
						else
						{
							validConstructor = false;
							break;
						}
					}

					// we found something, so now inflate
					if (validConstructor)
					{
						var paramValues = new object?[paramCount];
						for (var i = 0; i < paramCount; i++)
						{
							var descriptor = paramDescriptors[i];
							if (descriptor.Single != null || descriptor.Enumerable != null)
								paramValues[i] = GetService(descriptor.ServiceType, descriptor.Single, descriptor.Enumerable);
							else
								paramValues[i] = descriptor.Value;
						}
						return constructor.Invoke(paramValues);
					}
				}
			}

			throw new InvalidOperationException($"Could not match any constructors for '{implementationType.Name}'.");
		}
	}
}