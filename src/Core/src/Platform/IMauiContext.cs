using System;
using Microsoft.Maui.Animations;

namespace Microsoft.Maui
{
	public interface IMauiContext
	{
		IServiceProvider Services { get; }

		IMauiHandlersServiceProvider Handlers { get; }
		IAnimationManager AnimationManager { get; }

#if __ANDROID__
		global::Android.Content.Context? Context { get; }
#elif __IOS__
		UIKit.UIWindow? Window { get; }
#elif WINDOWS
		UI.Xaml.Window? Window { get; }
#endif
	}
}
