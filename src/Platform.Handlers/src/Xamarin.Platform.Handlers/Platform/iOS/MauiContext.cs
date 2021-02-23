using System;
using Microsoft.Extensions.DependencyInjection;

namespace Xamarin.Platform
{ 
	public class MauiContext : IMauiContext
	{
		readonly IServiceProvider _services;
		readonly IMauiHandlersServiceProvider _mauiHandlersServiceProvider;
		public MauiContext(IServiceProvider services)
		{
			_services = services;
			_mauiHandlersServiceProvider = Services.GetRequiredService<IMauiHandlersServiceProvider>() ??
				throw new InvalidOperationException($"The Handlers provider of type {nameof(IMauiHandlersServiceProvider)} was not found");
		}
		public IServiceProvider Services => _services;

		public IMauiHandlersServiceProvider Handlers => _mauiHandlersServiceProvider;
	}
}
