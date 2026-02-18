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

		[Obsolete("Use GetHandlerType instead.")]
		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		[Obsolete("Use GetHandlerType instead.")]
		public IElementHandler? GetHandler(Type type, IMauiContext context)
		{
			var handlerType = GetHandlerType(type);
			if (handlerType == null)
				return null;

			return (IElementHandler?)Activator.CreateInstance(handlerType);
		}

		[Obsolete("Use GetHandlerType instead.")]
		public IElementHandler? GetHandler<T>(IMauiContext context) where T : IElement
			=> GetHandler(typeof(T), context);

		private Type? TryGetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);
	}
}