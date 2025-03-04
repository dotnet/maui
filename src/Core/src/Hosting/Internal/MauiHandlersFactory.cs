using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	sealed class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
	{
		readonly ConcurrentDictionary<Type, Type?> _serviceCache = new ();

		readonly RegisteredHandlerServiceTypeSet _registeredHandlerServiceTypeSet;

		public MauiHandlersFactory(IMauiHandlersCollection collection)
			: base(collection)
		{
			_registeredHandlerServiceTypeSet = RegisteredHandlerServiceTypeSet.GetInstance(collection);
		}

		public IElementHandler? GetHandler(IElement view, IMauiContext context)
		{
			// Try to get the handler from the service collection first.
			// This allows app developers to override the default handler for a view.
			if (TryGetVirtualViewHandlerServiceType(view.GetType()) is Type serviceType
				&& GetService(serviceType) is IElementHandler handler)
			{
				return handler;
			}

			// TODO should we throw if the handler is not found?
			return view.GetElementHandler(context);
		}

		public Type? GetHandlerType(IElement view)
		{
			// Try to get the handler type from the service collection first.
			// This allows app developers to override the default handler for a view.
			if (TryGetVirtualViewHandlerServiceType(view.GetType()) is Type serviceType
				&& InternalCollection.TryGetService(serviceType, out ServiceDescriptor? serviceDescriptor))
			{
				return serviceDescriptor?.ImplementationType;
			}

			// TODO should we throw if the handler is not found?
			return view.GetElementHandlerType();
		}

		[Obsolete("Use GetHandler(IElement, IMauiContext) instead.")]
		public IElementHandler? GetHandler(Type type)
			=> GetService(GetVirtualViewHandlerServiceType(type)) as IElementHandler;

		[Obsolete("Use GetHandler(IElement, IMauiContext) instead.")]
		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		[Obsolete("Use GetHandlerType(IElement) instead.")]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetHandlerType(Type iview)
		{
			InternalCollection.TryGetService(GetVirtualViewHandlerServiceType(iview), out ServiceDescriptor? serviceDescriptor);

			return serviceDescriptor?.ImplementationType;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		private Type? TryGetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);

		private Type GetVirtualViewHandlerServiceType(Type type)
			=> TryGetVirtualViewHandlerServiceType(type) ?? throw new HandlerNotFoundException($"Unable to find a {nameof(IElementHandler)} corresponding to {type}. Please register a handler for {type} using `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.AddHandler` or `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.TryAddHandler`");
	}
}