using System;

namespace Microsoft.Maui
{
	public partial class MauiContext
	{
		readonly WeakReference<CoreUIAppContext>? _context;

		public MauiContext(CoreUIAppContext context) : this()
		{
			_context = new WeakReference<CoreUIAppContext>(context ?? throw new ArgumentNullException(nameof(context)));
		}

		public MauiContext(IServiceProvider services, CoreUIAppContext context) : this(services)
		{
			_context = new WeakReference<CoreUIAppContext>(context ?? throw new ArgumentNullException(nameof(context)));
		}

		public CoreUIAppContext? Context
		{
			get
			{
				if (_context == null)
					return null;

				CoreUIAppContext? context;
				if (_context.TryGetTarget(out context))
				{
					return context;
				}

				return null;
			}
		}

	}
}