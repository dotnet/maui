using System;
using Microsoft.Maui.Animations;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIWindow;
#elif MONOANDROID
using NativeView = Android.App.Activity;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : IWindowHandler
	{
		public static PropertyMapper<IWindow, WindowHandler> WindowMapper = new(ElementHandler.ElementMapper)
		{
			[nameof(IWindow.Title)] = MapTitle,
			[nameof(IWindow.Content)] = MapContent,
		};

		IAnimationManager? _animationManager;

		public WindowHandler()
			: base(WindowMapper)
		{
		}

		public WindowHandler(PropertyMapper? mapper = null)
			: base(mapper ?? WindowMapper)
		{
		}

		public IAnimationManager AnimationManager =>
			_animationManager ??= this.GetRequiredService<IAnimationManager>();

#if !NETSTANDARD
		protected override NativeView CreateNativeElement() =>
#if __ANDROID__
			MauiContext?.Context as NativeView ?? throw new InvalidOperationException($"MauiContext did not have a valid window.");
#else
			MauiContext?.Window ?? throw new InvalidOperationException($"MauiContext did not have a valid window.");
#endif
#endif
	}
}