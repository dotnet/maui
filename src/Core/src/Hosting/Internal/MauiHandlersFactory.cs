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
		readonly ConcurrentDictionary<Type, ElementHandlerAttribute?> _elementHandlerAttributeCache = new();

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

			// 2. Assignable DI registration. This preserves base/interface override behavior, e.g.
			// AddHandler<Button, CustomButtonHandler>() must also win for FancyButton : Button
			// instead of falling through to Button's inherited ElementHandler attribute.
			if (TryGetVirtualViewHandlerServiceType(type) is Type serviceType
				&& GetService(serviceType) is IElementHandler assignedHandler)
			{
				return assignedHandler;
			}

			// 3. ElementHandler attribute. Built-in controls use this as their trimmable default
			// when no user DI registration overrides the view type.
			if (TryGetElementHandlerAttribute(type, out var elementHandlerAttribute))
			{
				return CreateAttributeHandler(type, elementHandlerAttribute);
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
			if (InternalCollection.TryGetService(iview, out _))
			{
				return TryGetRegisteredHandlerType(iview, out Type? type) ? type : null;
			}

			if (TryGetVirtualViewHandlerServiceType(iview) is Type serviceType
				&& InternalCollection.TryGetService(serviceType, out _))
			{
				return TryGetRegisteredHandlerType(serviceType, out Type? type) ? type : null;
			}

			// Keep GetHandlerType in the same order as GetHandler so injection fallback paths see
			// the user-registered override type instead of the inherited ElementHandler default.
			if (TryGetElementHandlerAttribute(iview, out var elementHandlerAttribute))
			{
				return elementHandlerAttribute.GetHandlerType();
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

		private bool TryGetElementHandlerAttribute(Type viewType, [NotNullWhen(returnValue: true)] out ElementHandlerAttribute? elementHandlerAttribute)
		{
			elementHandlerAttribute = _elementHandlerAttributeCache.GetOrAdd(viewType, static type => FindElementHandlerAttribute(type));
			return elementHandlerAttribute is not null;
		}

		private static ElementHandlerAttribute? FindElementHandlerAttribute(Type viewType)
		{
			Type? type = viewType;

			while (type is not null)
			{
				var elementHandlerAttribute = type.GetCustomAttribute<ElementHandlerAttribute>();
				if (elementHandlerAttribute is not null)
				{
					return elementHandlerAttribute;
				}

				type = type.BaseType;
			}

			return null;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;

		private Type? TryGetVirtualViewHandlerServiceType(Type type)
			=> _serviceCache.GetOrAdd(type, _registeredHandlerServiceTypeSet.ResolveVirtualViewToRegisteredHandlerServiceType);

		private static IElementHandler? CreateAttributeHandler(Type viewType, ElementHandlerAttribute elementHandlerAttribute)
		{
			var handlerType = elementHandlerAttribute.GetHandlerType();

			try
			{
				return (IElementHandler?)Activator.CreateInstance(handlerType);
			}
			catch (MissingMethodException ex)
			{
				throw new HandlerNotFoundException(
					$"Unable to create the {nameof(IElementHandler)} {handlerType} declared by {nameof(ElementHandlerAttribute)} for {viewType}. " +
					$"Handlers declared with {nameof(ElementHandlerAttribute)} must have a public parameterless constructor. " +
					$"Use `Microsoft.Maui.Hosting.MauiHandlersCollectionExtensions.AddHandler` to register handlers that require constructor arguments.",
					ex);
			}
		}
	}
}
