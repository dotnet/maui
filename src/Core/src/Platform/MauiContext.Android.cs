using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Android.Content;


namespace Microsoft.Maui
{
	public partial class MauiContext
	{
		readonly WeakReference<Context>? _context;
		public MauiContext(IServiceProvider services, Context context) : this(services)
		{
			_context = new WeakReference<Context>(context ?? throw new ArgumentNullException(nameof(context)));
		}

		public Context? Context
		{
			get
			{
				if (_context == null)
					return null;

				Context? context;
				if (_context.TryGetTarget(out context))
				{
					return context;
				}

				return null;
			}
		}
	}
}
