using System;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public class MauiContext : IMauiContext
	{
		readonly WeakReference<Context> _context;
		readonly IServiceProvider? _services;
		readonly IMauiHandlersServiceProvider? _mauiHandlersServiceProvider;

		public MauiContext(Context context)
		{
			_context = new WeakReference<Context>(context ?? throw new ArgumentNullException(nameof(context)));
		}

		public MauiContext(IServiceProvider services, Context context) : this(context)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_mauiHandlersServiceProvider = Services.GetRequiredService<IMauiHandlersServiceProvider>();
		}

		public Context? Context
		{
			get
			{
				Context? context;
				if (_context.TryGetTarget(out context))
				{
					return context;
				}

				return null;
			}
		}

		public IServiceProvider Services =>
			_services ?? throw new InvalidOperationException($"No service provider was specified during construction.");

		public IMauiHandlersServiceProvider Handlers =>
			_mauiHandlersServiceProvider ?? throw new InvalidOperationException($"No service provider was specified during construction.");
	}
}