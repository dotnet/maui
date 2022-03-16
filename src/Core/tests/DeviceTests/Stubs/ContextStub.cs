using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ContextStub : IMauiContext, IServiceProvider
	{
		IServiceProvider _services;
		IAnimationManager _manager;
#if WINDOWS || ANDROID
		NavigationRootManager _windowManager;
#endif

		public ContextStub(IServiceProvider services)
		{
			_services = services;
		}

		public IServiceProvider Services => this;

		public object GetService(Type serviceType)
		{
			if (serviceType == typeof(IAnimationManager))
				return _manager ??= _services.GetRequiredService<IAnimationManager>();
#if ANDROID
			if (serviceType == typeof(Android.Content.Context))
				return MauiProgram.DefaultContext;

			if (serviceType == typeof(NavigationRootManager))
				return _windowManager ??= new NavigationRootManager(this);
#elif __IOS__
			if (serviceType == typeof(UIKit.UIWindow))
				return UIKit.UIApplication.SharedApplication.GetKeyWindow();
#elif WINDOWS
			if (serviceType == typeof(NavigationRootManager))
				return _windowManager ??= new NavigationRootManager(this);

			if (serviceType == typeof(UI.Xaml.Window))
				return MauiProgram.DefaultWindow;
#endif

			return _services.GetService(serviceType);
		}

		public IMauiHandlersFactory Handlers =>
			Services.GetRequiredService<IMauiHandlersFactory>();

#if __ANDROID__
		public Android.Content.Context Context =>
			Services.GetRequiredService<Android.Content.Context>();
#endif
	}
}