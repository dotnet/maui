using System;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public class MauiContext : IMauiContext
	{
		readonly Context _context;
		readonly IServiceProvider _services;
		readonly IMauiHandlersServiceProvider _mauiHandlersServiceProvider;
		public MauiContext(IServiceProvider services, Context context)
		{
			_services = services;
			_context = context;
			_mauiHandlersServiceProvider = Services.GetRequiredService<IMauiHandlersServiceProvider>() ??
				throw new InvalidOperationException($"The Handlers provider of type {nameof(IMauiHandlersServiceProvider)} was not found");
		}
		public Context Context => _context;

		public IServiceProvider Services => _services;

		public IMauiHandlersServiceProvider Handlers => _mauiHandlersServiceProvider;
	}
}
