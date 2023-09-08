#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting.Internal
{
	sealed class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
	{
		public MauiHandlersFactory(IEnumerable<HandlerMauiAppBuilderExtensions.HandlerRegistration> registrationActions) :
			base(CreateHandlerCollection(registrationActions))
		{
		}

		private static IMauiServiceCollection CreateHandlerCollection(IEnumerable<HandlerMauiAppBuilderExtensions.HandlerRegistration> registrationActions)
		{
			var collection = new MauiHandlersCollection();
			if (registrationActions != null)
			{
				foreach (var registrationAction in registrationActions)
				{
					registrationAction.AddRegistration(collection);
				}
			}
			HotReload.MauiHotReloadHelper.RegisterHandlers(collection);
			return collection;
		}

		public IElementHandler? GetHandler(Type type)
			=> GetService(type) as IElementHandler;

		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		public Type? GetHandlerType(Type iview)
		{
			if (!TryGetServiceDescriptors(ref iview, out var single, out var enumerable))
				return default;

			if (single != null)
				return GetImplementationType(single);

			if (enumerable != null)
			{
				foreach (var descriptor in enumerable)
				{
					return GetImplementationType(descriptor);
				}
			}

			return default;
		}

		// Based on: https://github.com/dotnet/runtime/blob/7d399f6deed60ce90292d2551c288d137e2278e6/src/libraries/Microsoft.Extensions.DependencyInjection.Abstractions/src/ServiceDescriptor.cs#L296-L311
		Type? GetImplementationType(ServiceDescriptor descriptor)
		{
			if (descriptor.ImplementationType != null)
			{
				return descriptor.ImplementationType;
			}
			else if (descriptor.ImplementationInstance != null)
			{
				return descriptor.ImplementationInstance.GetType();
			}
			else if (descriptor.ImplementationFactory != null)
			{
				Type[]? typeArguments = descriptor.ImplementationFactory.GetType().GenericTypeArguments;
				if (typeArguments.Length == 2)
					return typeArguments[1];
			}
			return null;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;
	}
}