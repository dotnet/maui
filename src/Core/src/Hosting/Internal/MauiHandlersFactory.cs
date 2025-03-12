using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	sealed class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
	{
		readonly ConcurrentDictionary<Type, Type> _serviceCache = new ConcurrentDictionary<Type, Type>();

		readonly RegisteredHandlerServiceTypeSet _registeredHandlerServiceTypeSet;

		public MauiHandlersFactory(IMauiHandlersCollection collection)
			: base(collection)
		{
			_registeredHandlerServiceTypeSet = RegisteredHandlerServiceTypeSet.GetInstance(collection);
		}

		public IElementHandler? GetHandler(Type type)
		{
			if (GetService(GetVirtualViewHandlerServiceType(type)) is IElementHandler handler)
			{
				return handler;
			}

			if (TryGetElementHandlerAttribute(type, out var elementHandlerAttribute))
			{
				return elementHandlerAttribute.CreateHandler();
			}

			return null;
		}

		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetHandlerType(Type iview)
		{
			
			if (InternalCollection.TryGetService(GetVirtualViewHandlerServiceType(iview), out ServiceDescriptor? serviceDescriptor)
				&& serviceDescriptor?.ImplementationType is Type type)
			{
				return type;
			}

			if (TryGetElementHandlerAttribute(iview, out var elementHandlerAttribute))
			{
				return GetHandlerType(elementHandlerAttribute);
			}

			return null;

			[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2073",
				Justification = "There is no need to create instances of the handlers for types with this attribute using reflection."
					+ "We intentionally avoid annotating these handler types with DAM.")]
			[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
			static Type GetHandlerType(ElementHandlerAttribute elementHandlerAttribute)
				=> elementHandlerAttribute.HandlerType;
		}

		private static bool TryGetElementHandlerAttribute(Type viewType, [NotNullWhen(returnValue: true)] out ElementHandlerAttribute? elementHandlerAttribute)
		{
			elementHandlerAttribute = viewType.GetCustomAttribute(typeof(ElementHandlerAttribute), inherit: false) as ElementHandlerAttribute;
			return elementHandlerAttribute != null;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		private Type GetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);
	}
}