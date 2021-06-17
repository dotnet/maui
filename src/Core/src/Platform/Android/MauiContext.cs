using System;
using System.Diagnostics;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui
{
	public class MauiContext : IMauiContext
	{
		const int TabletCrossover = 600;

		readonly WeakReference<Context> _context;
		readonly IServiceProvider? _services;
		readonly IMauiHandlersServiceProvider? _mauiHandlersServiceProvider;
		TargetIdiom _targetIdiom;

		public MauiContext(Context context)
		{
			_context = new WeakReference<Context>(context ?? throw new ArgumentNullException(nameof(context)));
		}

		public MauiContext(IServiceProvider services, Context context) : this(context)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_mauiHandlersServiceProvider = Services.GetRequiredService<IMauiHandlersServiceProvider>();
			_targetIdiom = TargetIdiom.Unsupported;
		}

		public Context? Context =>
			_context.TryGetTarget(out Context? context) ? context : null;

		public IServiceProvider Services =>
			_services ?? throw new InvalidOperationException($"No service provider was specified during construction.");

		public IMauiHandlersServiceProvider Handlers =>
			_mauiHandlersServiceProvider ?? throw new InvalidOperationException($"No service provider was specified during construction.");

		public TargetIdiom Idiom
		{
			get
			{
				if (_targetIdiom == TargetIdiom.Unsupported)
				{
					if (Context == null)
						return _targetIdiom;

					// First try UIModeManager
					using (var uiModeManager = UiModeManager.FromContext(Context))
					{
						try
						{
							var uiMode = uiModeManager?.CurrentModeType ?? UiMode.TypeUndefined;

							if (uiMode == UiMode.TypeNormal)
								_targetIdiom = TargetIdiom.Unsupported;
							else if (uiMode == UiMode.TypeTelevision)
								_targetIdiom = TargetIdiom.TV;
							else if (uiMode == UiMode.TypeDesk)
								_targetIdiom = TargetIdiom.Desktop;
							else if (uiMode == UiMode.TypeWatch)
								_targetIdiom = TargetIdiom.Watch;
						}
						catch (Exception ex)
						{
							Debug.WriteLine($"Unable to detect using UiModeManager: {ex.Message}");
						}
					}

					// Then try Configuration
					if (TargetIdiom.Unsupported == _targetIdiom)
					{
						var activity = Context.GetActivity();
						var configuration = activity?.Resources?.Configuration;

						if (configuration != null)
						{
							var minWidth = configuration.SmallestScreenWidthDp;
							var isWide = minWidth >= TabletCrossover;
							_targetIdiom = isWide ? TargetIdiom.Tablet : TargetIdiom.Phone;
						}
						else
						{
							// Start clutching at straws
							var metrics = activity?.Resources?.DisplayMetrics;

							if (metrics != null)
							{
								var minSize = Math.Min(metrics.WidthPixels, metrics.HeightPixels);
								var isWide = minSize * metrics.Density >= TabletCrossover;
								_targetIdiom = isWide ? TargetIdiom.Tablet : TargetIdiom.Phone;
							}
						}
					}
				}

				return _targetIdiom;
			}
		}
	}
}