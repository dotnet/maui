using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Hosting.Internal
{
	sealed class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
	{
		readonly object _cacheLock = new();
		HandlerCache? _cache;

		public MauiHandlersFactory(IMauiHandlersCollection collection)
			: base(collection)
		{
		}

		HandlerCache GetCache()
		{
			var version = (InternalCollection as MauiServiceCollection)?.Version;
			var generation = HotReload.MauiHotReloadHelper.HandlerCacheGeneration;
			var current = Volatile.Read(ref _cache);
			if (version is not null && current is not null
				&& current.CollectionVersion == version && current.HotReloadGeneration == generation)
				return current;

			lock (_cacheLock)
			{
				version = (InternalCollection as MauiServiceCollection)?.Version;
				generation = HotReload.MauiHotReloadHelper.HandlerCacheGeneration;
				if (version is not null && _cache is not null
					&& _cache.CollectionVersion == version && _cache.HotReloadGeneration == generation)
					return _cache;

				var cache = new HandlerCache(version, generation);
				foreach (var descriptor in InternalCollection)
				{
					if (typeof(IElement).IsAssignableFrom(descriptor.ServiceType))
						cache.RegisteredServiceTypes.Add(descriptor.ServiceType);
				}

				// Custom collections do not expose a mutation version, so their resolution
				// results cannot safely be retained between requests.
				Volatile.Write(ref _cache, version is not null ? cache : null);
				return cache;
			}
		}

		public IElementHandler? GetHandler(Type type) =>
			GetHandler(type, this);

		internal IElementHandler? GetHandler(Type type, IMauiContext mauiContext) =>
			GetHandler(type, new HandlerActivationServiceProvider(this, mauiContext));

		internal static IElementHandler? GetHandlerWithInjection(IMauiHandlersFactory handlers, Type viewType, IMauiContext mauiContext)
		{
			var activationServices = handlers is MauiHandlersFactory factory
				? new HandlerActivationServiceProvider(factory, mauiContext, useConstructorInjection: true)
				: null;
			try
			{
				if (activationServices is not null && handlers is MauiHandlersFactory handlersFactory)
					return handlersFactory.GetHandler(viewType, activationServices);

				return handlers.GetHandler(viewType, mauiContext);
			}
			catch (MissingMethodException) when (activationServices?.ConstructorInjectionAttempted != true)
			{
				return CreateTypeWithInjection(handlers, viewType, mauiContext);
			}
			catch (HandlerNotFoundException ex) when (ex.InnerException is MissingMethodException
				&& activationServices?.ConstructorInjectionAttempted != true)
			{
				return CreateTypeWithInjection(handlers, viewType, mauiContext);
			}
		}

		protected override object? CreateInstance(ServiceDescriptor item, IServiceProvider implementationFactoryServiceProvider)
		{
			if (item.ImplementationType is Type handlerType
				&& typeof(IElementHandler).IsAssignableFrom(handlerType)
				&& implementationFactoryServiceProvider is HandlerActivationServiceProvider { UseConstructorInjection: true } provider)
				return CreateHandlerInstance(handlerType, provider);

			return base.CreateInstance(item, implementationFactoryServiceProvider);
		}

		static IElementHandler CreateHandlerInstance(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType,
			HandlerActivationServiceProvider? activationServices)
		{
			if (activationServices is null)
				return (IElementHandler)Activator.CreateInstance(handlerType)!;

			if (handlerType.IsValueType || handlerType.GetConstructor(Type.EmptyTypes) is not null)
				return (IElementHandler)Activator.CreateInstance(handlerType)!;

			// A dependency failure must propagate without retrying the entire injection.
			activationServices.ConstructorInjectionAttempted = true;
			return CreateTypeWithInjection(handlerType, activationServices.MauiContext);
		}

		static IElementHandler? CreateTypeWithInjection(IMauiHandlersFactory handlers, Type viewType, IMauiContext mauiContext)
		{
			var handlerType = handlers.GetHandlerType(viewType);

			if (handlerType == null)
				return null;

			return CreateTypeWithInjection(handlerType, mauiContext);
		}

		static IElementHandler CreateTypeWithInjection(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType,
			IMauiContext mauiContext)
		{
#if ANDROID
			if (mauiContext.Context != null)
			{
				return (IElementHandler)ActivatorUtilities.CreateInstance(mauiContext.Services, handlerType, mauiContext.Context);
			}
#endif

			return (IElementHandler)ActivatorUtilities.CreateInstance(mauiContext.Services, handlerType);
		}

		IElementHandler? GetHandler(Type type, IServiceProvider implementationFactoryServiceProvider)
		{
			// 1. Exact DI registration (allows overriding attribute-based defaults)
			if (InternalCollection.TryGetService(type, out ServiceDescriptor? exactDescriptor)
				&& exactDescriptor is not null
				&& GetService(type, implementationFactoryServiceProvider) is IElementHandler exactHandler)
			{
				HotReload.MauiHotReloadHelper.RegisterHandlerType(exactDescriptor, exactHandler.GetType());
				return exactHandler;
			}

			// 2. A handler declared directly on the requested type preserves the historical
			// precedence of an exact built-in registration over assignable registrations.
			if (TryGetDirectElementHandlerAttribute(type, out var directElementHandlerAttribute))
			{
				return CreateAttributeHandler(type, directElementHandlerAttribute, implementationFactoryServiceProvider);
			}

			// 3. Assignable DI registration. AddHandler<Button, CustomButtonHandler>() still
			// applies to FancyButton when FancyButton does not declare its own handler.
			if (TryGetVirtualViewHandlerServiceType(type) is Type serviceType
				&& InternalCollection.TryGetService(serviceType, out ServiceDescriptor? assignedDescriptor)
				&& assignedDescriptor is not null
				&& GetService(serviceType, implementationFactoryServiceProvider) is IElementHandler assignedHandler)
			{
				HotReload.MauiHotReloadHelper.RegisterHandlerType(assignedDescriptor, assignedHandler.GetType());
				return assignedHandler;
			}

			// 4. An inherited ElementHandler attribute is the trimmable fallback for derived
			// types that do not declare or register their own handler.
			if (TryGetInheritedElementHandlerAttribute(type, out var inheritedElementHandlerAttribute))
			{
				return CreateAttributeHandler(type, inheritedElementHandlerAttribute, implementationFactoryServiceProvider);
			}

			// 5. ContentView fallback
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
			// An exact registration always wins, even when a factory registration has no
			// implementation type for this API to report.
			if (InternalCollection.TryGetService(iview, out ServiceDescriptor? exactDescriptor))
			{
				return exactDescriptor?.ImplementationType;
			}

			if (TryGetDirectElementHandlerAttribute(iview, out var directElementHandlerAttribute))
			{
				return GetValidatedHandlerType(iview, directElementHandlerAttribute);
			}

			if (TryGetVirtualViewHandlerServiceType(iview) is Type serviceType)
			{
				return TryGetRegisteredHandlerType(serviceType, out Type? type) ? type : null;
			}

			if (TryGetInheritedElementHandlerAttribute(iview, out var inheritedElementHandlerAttribute))
			{
				return GetValidatedHandlerType(iview, inheritedElementHandlerAttribute);
			}

			// ContentViewHandler is the default/fallback handler for any IContentView
			if (typeof(IContentView).IsAssignableFrom(iview))
			{
				return typeof(ContentViewHandler);
			}

			return null;
		}

		private bool TryGetRegisteredHandlerType(Type serviceType, [NotNullWhen(returnValue: true), DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] out Type? handlerType)
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

		private bool TryGetDirectElementHandlerAttribute(Type viewType, [NotNullWhen(returnValue: true)] out ElementHandlerAttribute? elementHandlerAttribute)
		{
			elementHandlerAttribute = GetElementHandlerAttributeResolution(viewType).Direct;
			return elementHandlerAttribute is not null;
		}

		private bool TryGetInheritedElementHandlerAttribute(Type viewType, [NotNullWhen(returnValue: true)] out ElementHandlerAttribute? elementHandlerAttribute)
		{
			elementHandlerAttribute = GetElementHandlerAttributeResolution(viewType).Inherited;
			return elementHandlerAttribute is not null;
		}

		private ElementHandlerAttributeResolution GetElementHandlerAttributeResolution(Type viewType) =>
			GetCache().ElementHandlerAttributes.GetOrAdd(
				viewType,
				static type => new Lazy<ElementHandlerAttributeResolution>(
					() => FindElementHandlerAttributes(type))).Value;

		private static ElementHandlerAttributeResolution FindElementHandlerAttributes(Type viewType)
		{
			var direct = viewType.GetCustomAttribute<ElementHandlerAttribute>(inherit: false);
			Type? type = viewType.BaseType;

			while (type is not null)
			{
				var elementHandlerAttribute = type.GetCustomAttribute<ElementHandlerAttribute>(inherit: false);
				if (elementHandlerAttribute is not null)
				{
					return new(direct, elementHandlerAttribute);
				}

				type = type.BaseType;
			}

			return new(direct, null);
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		private Type? TryGetVirtualViewHandlerServiceType(Type type)
		{
			var cache = GetCache();
			return cache.ServiceTypes.GetOrAdd(type, cache.ServiceTypeFactory).Value;
		}

		new object? GetService(Type serviceType, IServiceProvider implementationFactoryServiceProvider) =>
			base.GetService(serviceType, implementationFactoryServiceProvider);

		private IElementHandler? CreateAttributeHandler(Type viewType, ElementHandlerAttribute elementHandlerAttribute, IServiceProvider implementationFactoryServiceProvider)
		{
			var handlerType = GetValidatedHandlerType(viewType, elementHandlerAttribute);

			try
			{
				var activationServices = implementationFactoryServiceProvider is HandlerActivationServiceProvider { UseConstructorInjection: true } provider
					? provider
					: null;
				return CreateHandlerInstance(handlerType, activationServices);
			}
			catch (MissingMethodException ex) when (implementationFactoryServiceProvider is not HandlerActivationServiceProvider { ConstructorInjectionAttempted: true })
			{
				throw new HandlerNotFoundException(
					$"Unable to create the {nameof(IElementHandler)} {handlerType} declared by {nameof(ElementHandlerAttribute)} for {viewType}. " +
					$"Handlers declared with {nameof(ElementHandlerAttribute)} must have a public parameterless constructor when resolved directly through {nameof(IMauiHandlersFactory.GetHandler)}. " +
					$"Use `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.AddHandler` to register handlers that require constructor arguments, or resolve through `ToHandler()` so constructor arguments can come from the DI container.",
					ex);
			}
		}

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		private static Type GetValidatedHandlerType(Type viewType, ElementHandlerAttribute elementHandlerAttribute)
		{
			var handlerType = elementHandlerAttribute.GetHandlerType();

			if (!typeof(IElementHandler).IsAssignableFrom(handlerType))
			{
				throw new HandlerNotFoundException(
					$"Unable to use the handler type {handlerType} declared by {nameof(ElementHandlerAttribute)} for {viewType}. " +
					$"The declared handler type must implement {nameof(IElementHandler)}.");
			}

			return handlerType;
		}

		private readonly record struct ElementHandlerAttributeResolution(
			ElementHandlerAttribute? Direct,
			ElementHandlerAttribute? Inherited);

		sealed class HandlerCache
		{
			public HandlerCache(long? collectionVersion, long hotReloadGeneration)
			{
				CollectionVersion = collectionVersion;
				HotReloadGeneration = hotReloadGeneration;
				ServiceTypeFactory = viewType => new Lazy<Type?>(
					() => RegisteredServiceTypes.ResolveVirtualViewToRegisteredHandlerServiceType(viewType));
			}

			public long? CollectionVersion { get; }
			public long HotReloadGeneration { get; }
			public RegisteredHandlerServiceTypeSet RegisteredServiceTypes { get; } = new();
			public Func<Type, Lazy<Type?>> ServiceTypeFactory { get; }
			public ConcurrentDictionary<Type, Lazy<Type?>> ServiceTypes { get; } = new();
			public ConcurrentDictionary<Type, Lazy<ElementHandlerAttributeResolution>> ElementHandlerAttributes { get; } = new();
		}

		sealed class HandlerActivationServiceProvider : IServiceProvider
		{
			readonly MauiHandlersFactory _handlerServices;
			readonly IMauiContext _mauiContext;

			public HandlerActivationServiceProvider(MauiHandlersFactory handlerServices, IMauiContext mauiContext, bool useConstructorInjection = false)
			{
				_handlerServices = handlerServices;
				_mauiContext = mauiContext;
				UseConstructorInjection = useConstructorInjection;
			}

			public IMauiContext MauiContext => _mauiContext;
			public bool UseConstructorInjection { get; }
			public bool ConstructorInjectionAttempted { get; set; }

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
		internal static IElementHandler? GetHandlerWithInjection(this IMauiHandlersFactory handlers, Type type, IMauiContext mauiContext) =>
			MauiHandlersFactory.GetHandlerWithInjection(handlers, type, mauiContext);

		internal static IElementHandler? GetHandler(this IMauiHandlersFactory handlers, Type type, IMauiContext mauiContext) =>
			handlers is MauiHandlersFactory handlersFactory
				? handlersFactory.GetHandler(type, mauiContext)
				: handlers.GetHandler(type);
	}
}
