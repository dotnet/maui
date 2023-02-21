using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ContextStub : IMauiContext, IServiceProvider
	{
		IServiceProvider _services;
		IAnimationManager _manager;
#if WINDOWS || ANDROID
		NavigationRootManager _windowManager;
#endif
#if ANDROID
		Android.Content.Context _androidContext;
#endif

		public ContextStub(IServiceProvider services)
		{
			_services = services;
		}

		public IServiceProvider Services => this;

		public object GetService(Type serviceType)
		{
			// Don't add serviceType == typeof(IApplication) here
			// The headless runner doesn't set Application.Current
			// so you'll get confusing behavior if you do.

			if (serviceType == typeof(IAnimationManager))
				return _manager ??= _services.GetRequiredService<IAnimationManager>();
#if ANDROID
			if (serviceType == typeof(Android.Content.Context))
				return MauiProgramDefaults.DefaultContext;

			if (serviceType == typeof(NavigationRootManager))
				return _windowManager ??= new NavigationRootManager(this);
#elif IOS || MACCATALYST
			if (serviceType == typeof(UIKit.UIWindow))
				return UIKit.UIApplication.SharedApplication.GetKeyWindow();
#elif WINDOWS
			if (serviceType == typeof(NavigationRootManager))
				return _windowManager ??= new NavigationRootManager(MauiProgramDefaults.DefaultWindow);

			if (serviceType == typeof(UI.Xaml.Window))
				return _services.GetService(serviceType) ?? MauiProgramDefaults.DefaultWindow;
#endif
			if (serviceType == typeof(IDispatcher))
				return _services.GetService(serviceType) ?? TestDispatcher.Current;

			return _services.GetService(serviceType);
		}

		public IMauiHandlersFactory Handlers =>
			Services.GetRequiredService<IMauiHandlersFactory>();

#if __ANDROID__
		public Android.Content.Context Context
		{
			get => Services.GetRequiredService<Android.Content.Context>();
			set => _androidContext = value;
		}
#endif
	}
}