using System;
using Android.Content;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui
{
	public partial class MauiContext : IScopedMauiContext
	{
		readonly WeakReference<Context>? _context;

		public MauiContext(IServiceProvider services, Context context)
			: this(services)
		{
			_context = new WeakReference<Context>(context ?? throw new ArgumentNullException(nameof(context)));
		}

		internal MauiContext(Context context)
			: this()
		{
			_context = new WeakReference<Context>(context ?? throw new ArgumentNullException(nameof(context)));
		}

		public Context? Context
		{
			get
			{
				if (_context == null)
					return null;

				return _context.TryGetTarget(out Context? context) ? context : null;
			}
		}

		LayoutInflater? IScopedMauiContext.LayoutInflater => null;

		FragmentManager? IScopedMauiContext.FragmentManager => null;

		NavigationManager? IScopedMauiContext.NavigationManager
			=> throw new NotImplementedException();
	}
}
