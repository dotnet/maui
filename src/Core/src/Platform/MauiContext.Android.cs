using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;


namespace Microsoft.Maui
{
	public partial class MauiContext
	{
		readonly WeakReference<Context>? _context;
		public MauiContext(IServiceProvider services, Context context) : this(services)
		{
			_context = new WeakReference<Context>(context ?? throw new ArgumentNullException(nameof(context)));
		}

		public MauiContext(Context context) : this()
		{
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
