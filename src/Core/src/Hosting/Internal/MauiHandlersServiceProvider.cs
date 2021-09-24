#nullable enable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Hosting.Internal
{
	class MauiHandlersServiceProvider : MauiServiceProvider, IMauiHandlersServiceProvider
	{
		public MauiHandlersServiceProvider(IEnumerable<HandlerMauiAppBuilderExtensions.HandlerRegistration> registrationActions) :
			base(CreateHandlerCollection(registrationActions), constructorInjection: false)
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
			return collection;
		}

		public IElementHandler? GetHandler(Type type)
			=> GetService(type) as IElementHandler;

		public IElementHandler? GetHandler<T>() where T : IElement
			=> GetHandler(typeof(T));

		public Type? GetHandlerType(Type iview)
		{
			foreach (var descriptor in GetServiceDescriptors(iview))
			{
				return descriptor.ImplementationType;
			}
			return null;
		}

		public IMauiHandlersCollection GetCollection() => (IMauiHandlersCollection)InternalCollection;
	}
}