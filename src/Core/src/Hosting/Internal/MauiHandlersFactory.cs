using System;
using System.Collections.Concurrent;
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

		public IElementHandler? GetHandler(IElement view, IMauiContext context)
		{
			// Try to get the handler from the service collection first.
			// This allows app developers to override the default handler for a view.
			if (GetService(GetVirtualViewHandlerServiceType(view.GetType())) is IElementHandler handler)
			{
				return handler;
			}

			return view.GetElementHandler(context);
		}

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetHandlerType(IElement view)
		{
			// Try to get the handler type from the service collection first.
			// This allows app developers to override the default handler for a view.
			if (InternalCollection.TryGetService(GetVirtualViewHandlerServiceType(view.GetType()), out ServiceDescriptor? serviceDescriptor))
			{
				return serviceDescriptor?.ImplementationType;
			}

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

		private Type GetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);
	}
}