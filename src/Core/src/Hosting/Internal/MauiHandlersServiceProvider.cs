#nullable enable
using System;

namespace Microsoft.Maui.Hosting.Internal
{
	class MauiHandlersServiceProvider : MauiServiceProvider, IMauiHandlersServiceProvider
	{
		public MauiHandlersServiceProvider(IMauiServiceCollection collection)
			: base(collection, false)
		{
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
	}
}