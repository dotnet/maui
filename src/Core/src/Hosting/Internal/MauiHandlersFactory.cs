using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	sealed class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
	{
		readonly ConcurrentDictionary<Type, Type?> _serviceCache = new();

		readonly RegisteredHandlerServiceTypeSet _registeredHandlerServiceTypeSet;

		public MauiHandlersFactory(IMauiHandlersCollection collection)
			: base(collection)
		{
			_registeredHandlerServiceTypeSet = RegisteredHandlerServiceTypeSet.GetInstance(collection);
		}

		public IElementHandler? GetHandler(Type type, IMauiContext context)
		{
			// Check if there is a handler registered for this EXACT type -- allows overriding the default handler
			if (GetService(type) is IElementHandler exactRegisteredHandler)
			{
				return exactRegisteredHandler;
			}

			if (TryGetElementHandlerAttribute(type, out var elementHandlerAttribute))
			{
				return elementHandlerAttribute.CreateHandler(context);
			}

			if (TryGetVirtualViewHandlerServiceType(type) is Type serviceType
				&& GetService(serviceType) is IElementHandler inheritedRegisteredHandler)
			{
				return inheritedRegisteredHandler;
			}

			throw new HandlerNotFoundException($"Unable to find a {nameof(IElementHandler)} corresponding to {type}. Please register a handler for {type} using `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.AddHandler` or `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.TryAddHandler`");
		}

		public IElementHandler? GetHandler<T>(IMauiContext context) where T : IElement
			=> GetHandler(typeof(T), context);

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetConstructibleHandlerType(Type iview)
		{
			// Check if there is a handler registered for this EXACT type -- allows overriding the default handler
			if (TryGetRegisteredHandlerType(iview, out Type? type))
			{
				return type;
			}

			if (TryGetElementHandlerAttribute(iview, out var elementHandlerAttribute))
			{
				throw new InvalidOperationException($"The handler type {elementHandlerAttribute.HandlerType} for {iview} cannot be constructed by the factory. " +
					$"Handlers created via {nameof(ElementHandlerAttribute)} must be created using the attribute's {nameof(ElementHandlerAttribute.CreateHandler)} method.");
			}

			if (TryGetVirtualViewHandlerServiceType(iview) is Type serviceType
				&& TryGetRegisteredHandlerType(serviceType, out type))
			{
				return type;
			}

			return null;
		}

		public Type? GetHandlerType(Type iview)
		{
			// Check if there is a handler registered for this EXACT type -- allows overriding the default handler
			if (TryGetRegisteredHandlerType(iview, out Type? type))
			{
				return type;
			}

			if (TryGetElementHandlerAttribute(iview, out var elementHandlerAttribute))
			{
				return elementHandlerAttribute.HandlerType;
			}

			if (TryGetVirtualViewHandlerServiceType(iview) is Type serviceType
				&& TryGetRegisteredHandlerType(serviceType, out type))
			{
				return type;
			}

			return null;
		}

		private bool TryGetRegisteredHandlerType(Type serviceType, [NotNullWhen(returnValue: true)] out Type? handlerType)
		{
			if (InternalCollection.TryGetService(serviceType, out ServiceDescriptor? serviceDescriptor)
				&& serviceDescriptor?.ImplementationType is Type type)
			{
				handlerType = type;
				return true;
			}

			handlerType = null;
			return handlerType is not null;
		}

		private static bool TryGetElementHandlerAttribute(Type viewType, [NotNullWhen(returnValue: true)] out ElementHandlerAttribute? elementHandlerAttribute)
		{
			elementHandlerAttribute = null;
			Type? type = viewType;

			while (type is not null)
			{
				elementHandlerAttribute = type.GetCustomAttribute<ElementHandlerAttribute>();
				if (elementHandlerAttribute is not null)
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		private Type? TryGetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);
	}
}