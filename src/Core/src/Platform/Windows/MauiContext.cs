#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Windows.System.Profile;

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
					switch (AnalyticsInfo.VersionInfo.DeviceFamily)
					{
						case "Windows.Desktop":
							if (Windows.UI.ViewManagement.UIViewSettings.GetForCurrentView().UserInteractionMode ==
								Windows.UI.ViewManagement.UserInteractionMode.Touch)
								_targetIdiom = TargetIdiom.Tablet;
							else
								_targetIdiom = TargetIdiom.Desktop;
							break;
						case "Windows.Mobile":
							_targetIdiom = TargetIdiom.Phone;
							break;
						case "Windows.Xbox":
							_targetIdiom = TargetIdiom.TV;
							break;
						default:
							_targetIdiom = TargetIdiom.Unsupported;
							break;
					}
				}

				return _targetIdiom;
			}
		}
	}
}