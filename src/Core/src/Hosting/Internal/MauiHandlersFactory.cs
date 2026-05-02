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

		public IElementHandler? GetHandler(Type type)
			=> GetHandler(type, this);

		internal IElementHandler? GetHandler(Type type, IMauiContext mauiContext)
			=> GetHandler(type, new HandlerActivationServiceProvider(this, mauiContext));

		IElementHandler? GetHandler(Type type, IServiceProvider implementationFactoryServiceProvider)
		{
			if (TryGetVirtualViewHandlerServiceType(type) is Type serviceType
				&& InternalCollection.TryGetService(serviceType, out ServiceDescriptor? serviceDescriptor)
				&& serviceDescriptor is not null
				&& GetService(serviceType, implementationFactoryServiceProvider) is IElementHandler handler)
			{
				HotReload.MauiHotReloadHelper.RegisterHandlerType(serviceDescriptor, handler.GetType());
				return handler;
			}

			if (TryGetElementHandlerAttribute(type, out var elementHandlerAttribute))
			{
				return elementHandlerAttribute.CreateHandler();
			}

			throw new HandlerNotFoundException($"Unable to find a {nameof(IElementHandler)} corresponding to {type}. Please register a handler for {type} using `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.AddHandler` or `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.TryAddHandler`");
		}

		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetHandlerType(Type iview)
		{
			if (TryGetVirtualViewHandlerServiceType(iview) is Type serviceType
				&& InternalCollection.TryGetService(serviceType, out ServiceDescriptor? serviceDescriptor)
				&& serviceDescriptor is not null)
			{
				return serviceDescriptor.ImplementationType;
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
			elementHandlerAttribute = viewType.GetCustomAttribute<ElementHandlerAttribute>();
			return elementHandlerAttribute is not null;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		private Type? TryGetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);

		object? GetService(Type serviceType, HandlerActivationServiceProvider implementationFactoryServiceProvider)
			=> base.GetService(serviceType, implementationFactoryServiceProvider);

		sealed class HandlerActivationServiceProvider : IServiceProvider
		{
			readonly MauiHandlersFactory _handlerServices;
			readonly IMauiContext _mauiContext;

			public HandlerActivationServiceProvider(MauiHandlersFactory handlerServices, IMauiContext mauiContext)
			{
				_handlerServices = handlerServices;
				_mauiContext = mauiContext;
			}

			public object? GetService(Type serviceType)
			{
				if (serviceType == typeof(IServiceProvider))
					return this;

				if (serviceType == typeof(IMauiContext))
					return _mauiContext;

#if ANDROID
				if (serviceType == typeof(global::Android.Content.Context))
					return _mauiContext.Context;
#endif

				return _handlerServices.GetService(serviceType, this)
					?? _mauiContext.Services.GetService(serviceType);
			}
		}

	}

	static class MauiHandlersFactoryExtensions
	{
		internal static IElementHandler? GetHandler(this IMauiHandlersFactory handlers, Type type, IMauiContext mauiContext) =>
			handlers is MauiHandlersFactory handlersFactory
				? handlersFactory.GetHandler(type, mauiContext)
				: handlers.GetHandler(type);
	}
}