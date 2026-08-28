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
		readonly ConcurrentDictionary<Type, Lazy<Type?>> _serviceCache = new();
		readonly ConcurrentDictionary<Type, Lazy<ElementHandlerAttributeResolution>> _elementHandlerAttributeCache = new();

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

			// 2. A handler declared directly on the requested type preserves the historical
			// precedence of an exact built-in registration over assignable registrations.
			if (TryGetDirectElementHandlerAttribute(type, out var directElementHandlerAttribute))
			{
				return CreateAttributeHandler(type, directElementHandlerAttribute);
			}

			// 3. Assignable DI registration. AddHandler<Button, CustomButtonHandler>() still
			// applies to FancyButton when FancyButton does not declare its own handler.
			if (TryGetVirtualViewHandlerServiceType(type) is Type serviceType
				&& GetService(serviceType) is IElementHandler assignedHandler)
			{
				return assignedHandler;
			}

			// 4. An inherited ElementHandler attribute is the trimmable fallback for derived
			// types that do not declare or register their own handler.
			if (TryGetInheritedElementHandlerAttribute(type, out var inheritedElementHandlerAttribute))
			{
				return CreateAttributeHandler(type, inheritedElementHandlerAttribute);
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
			_elementHandlerAttributeCache.GetOrAdd(
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
			=> _serviceCache.GetOrAdd(
				type,
				static (viewType, serviceTypes) => new Lazy<Type?>(
					() => serviceTypes.ResolveVirtualViewToRegisteredHandlerServiceType(viewType)),
				_registeredHandlerServiceTypeSet).Value;

		private static IElementHandler? CreateAttributeHandler(Type viewType, ElementHandlerAttribute elementHandlerAttribute)
		{
			var handlerType = GetValidatedHandlerType(viewType, elementHandlerAttribute);

			try
			{
				return (IElementHandler)Activator.CreateInstance(handlerType)!;
			}
			catch (MissingMethodException ex)
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
	}
}
