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
#if WINDOWS || IOS
			[nameof(IMenuBarElement.MenuBar)] = MapMenuBar,
#endif
#if WINDOWS
			[nameof(IWindow.FlowDirection)] = MapFlowDirection,
#endif
		};

		public static CommandMapper<IWindow, IWindowHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IWindow.RequestDisplayDensity)] = MapRequestDisplayDensity,
		};

		public WindowHandler()
			: base(Mapper, CommandMapper)
		{
		}

		public WindowHandler(IPropertyMapper? mapper = null)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public WindowHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

#if !NETSTANDARD
		protected override PlatformView CreatePlatformElement() =>
			MauiContext?.Services.GetService<PlatformView>() ?? throw new InvalidOperationException($"MauiContext did not have a valid window.");
#endif
	}
}