#nullable enable
using System;

namespace Microsoft.Maui.Hosting.Internal
{
	class MauiHandlersServiceProvider : MauiServiceProvider, IMauiHandlersServiceProvider
	{
		readonly IMauiHandlersCollection _collection;

		public MauiHandlersServiceProvider(IMauiHandlersCollection collection)
			: base(collection, false)
		{
			_collection = collection;
		}

		public IFrameworkElementHandler? GetHandler(Type type)
			=> GetService(type) as IFrameworkElementHandler;

		public IFrameworkElementHandler? GetHandler<T>() where T : IView
			=> GetHandler(typeof(T));

		public Type? GetHandlerType(Type iview)
		{
			foreach (var descriptor in GetServiceDescriptors(iview))
			{
				return descriptor.ImplementationType;
			}
			return null;
		}

		public IMauiHandlersCollection GetCollection() => _collection;
	}
}