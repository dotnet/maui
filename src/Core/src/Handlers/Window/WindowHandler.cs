using System;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIWindow;
#elif MONOANDROID
using NativeView = Android.App.Activity;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler
	{
		public static IPropertyMapper<IWindow, WindowHandler> WindowMapper = new PropertyMapper<IWindow, WindowHandler>(ElementHandler.ElementMapper)
		{
			[nameof(IWindow.Title)] = MapTitle,
			[nameof(IWindow.Content)] = MapContent,
		};

		public WindowHandler()
			: base(WindowMapper)
		{
		}

		public WindowHandler(IPropertyMapper? mapper = null)
			: base(mapper ?? WindowMapper)
		{
		}

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