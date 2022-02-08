using System;
using Microsoft.Extensions.DependencyInjection;
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
#if ANDROID || WINDOWS
			[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
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
			MauiContext?.Services.GetService<NativeView>() ?? throw new InvalidOperationException($"MauiContext did not have a valid window.");
#endif
	}
}