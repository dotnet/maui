using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ContextStub : IMauiContext, IServiceProvider
	{
		IServiceProvider _services;
		IAnimationManager _manager;
#if WINDOWS
		WindowManager _windowManager;
#endif

		public ContextStub(IServiceProvider services)
		{
			_services = services;
		}

		public IServiceProvider Services => this;

		public object GetService(Type serviceType)
		{
			if (serviceType == typeof(IAnimationManager))
				return _manager ??= Services.GetRequiredService<IAnimationManager>();
#if __ANDROID__
			if (serviceType == typeof(Android.Content.Context))
				return Platform.DefaultContext;
#elif __IOS__
			if (serviceType == typeof(UIKit.UIWindow))
				return UIKit.UIApplication.SharedApplication.GetKeyWindow();
#elif WINDOWS
			if (serviceType == typeof(WindowManager))
				return _windowManager ??= new WindowManager(this);
#endif

			return _services.GetService(serviceType);
		}

		public IMauiHandlersServiceProvider Handlers =>
			Services.GetRequiredService<IMauiHandlersServiceProvider>();

#if __ANDROID__
		public Android.Content.Context Context =>
			Services.GetRequiredService<Android.Content.Context>();
#endif
	}
}