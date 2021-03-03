using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	class HandlersContextStub : IMauiContext
	{
		readonly IServiceProvider _services;
		readonly IMauiHandlersServiceProvider _handlersServiceProvider;

		public HandlersContextStub(IServiceProvider services)
		{
			_services = services;
			_handlersServiceProvider = Services.GetRequiredService<IMauiHandlersServiceProvider>();
		}

		public IServiceProvider Services => _services;

		public IMauiHandlersServiceProvider Handlers => _handlersServiceProvider;
	}
}
