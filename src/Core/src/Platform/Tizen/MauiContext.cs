using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public class MauiContext : IMauiContext
	{
		readonly CoreUIAppContext _context;
		readonly IServiceProvider? _services;
		readonly IMauiHandlersServiceProvider? _mauiHandlersServiceProvider;

		public MauiContext(CoreUIAppContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public MauiContext(IServiceProvider services, CoreUIAppContext context)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_mauiHandlersServiceProvider = Services.GetRequiredService<IMauiHandlersServiceProvider>();
		}

		public CoreUIAppContext Context => _context;

		public IServiceProvider Services =>
			_services ?? throw new InvalidOperationException($"No service provider was specified during construction.");

		public IMauiHandlersServiceProvider Handlers =>
			_mauiHandlersServiceProvider ?? throw new InvalidOperationException($"No service provider was specified during construction.");
	}
}