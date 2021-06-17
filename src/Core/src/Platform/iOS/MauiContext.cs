using System;
using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace Microsoft.Maui
{
	public class MauiContext : IMauiContext
	{
		readonly IServiceProvider? _services;
		readonly IMauiHandlersServiceProvider? _mauiHandlersServiceProvider;
		TargetIdiom _targetIdiom;

		public MauiContext()
		{

		}

		public MauiContext(IServiceProvider services)
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
			_mauiHandlersServiceProvider = Services.GetRequiredService<IMauiHandlersServiceProvider>();
			_targetIdiom = TargetIdiom.Unsupported;
		}

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
#if MACCATALYST
					_targetIdiom = TargetIdiom.Desktop;
#else
					_targetIdiom = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? TargetIdiom.Tablet : TargetIdiom.Phone;
#endif
				}

				return _targetIdiom;
			}
		}
	}
}