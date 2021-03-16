using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public abstract class App : IApp
	{
		IServiceProvider? _serviceProvider;
		IMauiContext? _context;

		public IServiceProvider? Services => _serviceProvider;

		public IMauiContext? Context => _context;

		internal void SetServiceProvider(IServiceProvider provider)
		{
			_serviceProvider = provider;
			SetHandlerContext(provider.GetService<IMauiContext>());
		}

		internal void SetHandlerContext(IMauiContext? context)
		{
			_context = context;
		}
	}
}