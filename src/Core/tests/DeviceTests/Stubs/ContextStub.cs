using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ContextStub : IMauiContext, IScopedMauiContext
	{
		IAnimationManager _manager;
		public ContextStub(IServiceProvider services)
		{
			Services = services;
		}

		public IServiceProvider Services { get; }

		public IMauiHandlersServiceProvider Handlers =>
			Services.GetRequiredService<IMauiHandlersServiceProvider>();

		public IAnimationManager AnimationManager =>
			_manager ??= Services.GetRequiredService<IAnimationManager>();

#if __ANDROID__
		public Android.Content.Context Context => Platform.DefaultContext;

		public Android.Views.LayoutInflater LayoutInflater => null;

		public AndroidX.Fragment.App.FragmentManager FragmentManager => null;
#elif __IOS__
		public UIKit.UIWindow Window => UIKit.UIApplication.SharedApplication.GetKeyWindow();
#elif WINDOWS
		public UI.Xaml.Window Window => throw new NotImplementedException();
#endif
	}
}