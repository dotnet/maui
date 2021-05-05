#nullable enable
using System;
using Microsoft.Maui.HotReload;

namespace Microsoft.Maui.Hosting.Internal
{
	class MauiHandlersServiceProvider : MauiServiceProvider, IMauiHandlersServiceProvider, IHotReloadableHandlersServiceProvider
	{
		readonly IMauiHandlersCollection _collection;

		public MauiHandlersServiceProvider(IMauiHandlersCollection collection)
			: base(collection, false)
		{
			_collection = collection;
		}

		public IViewHandler? GetHandler(Type type)
			=> GetService(type) as IViewHandler;

		public IViewHandler? GetHandler<T>() where T : IView
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