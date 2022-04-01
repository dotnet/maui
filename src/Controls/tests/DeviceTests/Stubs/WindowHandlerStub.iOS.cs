using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public class WindowHandlerStub : ElementHandler<IWindow, UIWindow>, IWindowHandler
	{
		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		private static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			var view = window.Content.ToPlatform(handler.MauiContext);

			var vc =
				(window.Content.Handler as IPlatformViewHandler)
					.ViewController;

			handler.PlatformView.RootViewController.PresentViewController(vc, false, null);
		}

		protected override void DisconnectHandler(UIWindow platformView)
		{
			platformView.RootViewController.DismissViewController(false, null);
			base.DisconnectHandler(platformView);
		}

		public WindowHandlerStub()
			: base(WindowMapper)
		{
		}

		protected override UIWindow CreatePlatformElement()
		{
			return MauiContext.Services.GetService<UIWindow>();
		}
	}
}
