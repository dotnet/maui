using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ContextStub : IMauiContext
	{
		public ContextStub(IServiceProvider services)
		{
			Services = services;
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersServiceProvider Handlers =>
			Services.GetRequiredService<IMauiHandlersServiceProvider>();

		public IAnimationManager AnimationManager =>
			Services.GetRequiredService<IAnimationManager>();

#if __ANDROID__
		public Android.Content.Context Context => Platform.DefaultContext;
#elif __IOS__
		public UIKit.UIWindow Window => UIKit.UIApplication.SharedApplication.GetKeyWindow();
#elif WINDOWS
		public UI.Xaml.Window Window => throw new NotImplementedException();
#endif
	}
}