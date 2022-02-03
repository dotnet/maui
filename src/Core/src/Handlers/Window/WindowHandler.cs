using System;
using Microsoft.Extensions.DependencyInjection;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIWindow;
#elif MONOANDROID
using PlatformView = Android.App.Activity;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler : IWindowHandler
	{
		public static IPropertyMapper<IWindow, IWindowHandler> Mapper = new PropertyMapper<IWindow, IWindowHandler>(ElementHandler.ElementMapper)
		{
			[nameof(IWindow.Title)] = MapTitle,
			[nameof(IWindow.Content)] = MapContent,
#if ANDROID || WINDOWS
			[nameof(IToolbarElement.Toolbar)] = MapToolbar,
#endif
		};

		public WindowHandler()
			: base(Mapper)
		{
		}

		public WindowHandler(IPropertyMapper? mapper = null)
			: base(mapper ?? Mapper)
		{
		}

#if !NETSTANDARD
		protected override PlatformView CreateNativeElement() =>
			MauiContext?.Services.GetService<PlatformView>() ?? throw new InvalidOperationException($"MauiContext did not have a valid window.");
#endif
	}
}