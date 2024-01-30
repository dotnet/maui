#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
			=> GetService(GetVirtualViewHandlerServiceType(type)) as IElementHandler;

		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetHandlerType(Type iview)
		{
			var serviceDescriptor = GetServiceDescriptor(GetVirtualViewHandlerServiceType(iview));
			if (serviceDescriptor == null)
				return default;

			return serviceDescriptor.ImplementationType;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		private Type GetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);
	}
}