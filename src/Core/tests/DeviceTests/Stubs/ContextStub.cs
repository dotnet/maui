using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ContextStub : IMauiContext
	{
		IAnimationManager _animationManager;
		public ContextStub(IServiceProvider services)
		{
			Services = services;
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersServiceProvider Handlers =>
			Services.GetRequiredService<IMauiHandlersServiceProvider>();

		public IAnimationManager AnimationManager => _animationManager ??= new AnimationManager { Ticker = new NativeTicker(this) };
#if __ANDROID__
		public Android.Content.Context Context => Platform.DefaultContext;
#elif __IOS__
		public UIKit.UIWindow Window => throw new NotImplementedException();
#endif
	}
}