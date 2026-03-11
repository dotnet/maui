using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;

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

		public IElementHandler? GetHandler(Type type)
		{
			// 1. Exact DI registration (allows overriding attribute-based defaults)
			if (InternalCollection.TryGetService(type, out _)
				&& GetService(type) is IElementHandler exactHandler)
			{
				return exactHandler;
			}

			// 2. ElementHandler attribute (no DI, just Activator.CreateInstance)
			if (TryGetElementHandlerAttribute(type, out var elementHandlerAttribute))
			{
				return (IElementHandler?)Activator.CreateInstance(elementHandlerAttribute.HandlerType);
			}

			// 3. Interface-based DI registration (e.g., handler registered for IScrollView)
			if (TryGetVirtualViewHandlerServiceType(type) is Type serviceType
				&& GetService(serviceType) is IElementHandler interfaceHandler)
			{
				return interfaceHandler;
			}

			// 4. ContentView fallback
			if (typeof(IContentView).IsAssignableFrom(type))
			{
				return new ContentViewHandler();
			}

			throw new HandlerNotFoundException($"Unable to find a {nameof(IElementHandler)} corresponding to {type}. Please register a handler for {type} using `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.AddHandler` or `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.TryAddHandler`");
		}

		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
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

			// ContentViewHandler is the default/fallback handler for any IContentView
			if (typeof(IContentView).IsAssignableFrom(iview))
			{
				return typeof(ContentViewHandler);
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
			return false;
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

		[Obsolete("The handlers collection no longer contains all registered handlers. Use GetHandlerType instead.")]
		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		private Type? TryGetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);
	}
}