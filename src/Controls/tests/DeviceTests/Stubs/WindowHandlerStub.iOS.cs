using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowHandlerStub : ElementHandler<IWindow, UIWindow>, IWindowHandler
	{
		TaskCompletionSource<bool> _finishedDisconnecting = new TaskCompletionSource<bool>();
		public Task FinishedDisconnecting => _finishedDisconnecting.Task;

		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		private static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			var view = window.Content.ToPlatform(handler.MauiContext);

			if (window.Content is Shell)
			{
				var vc =
					(window.Content.Handler as IPlatformViewHandler)
						.ViewController;

				handler.PlatformView.RootViewController.PresentViewController(vc, false, null);
			}
			else
			{
				handler.PlatformView.RootViewController.View.AddSubview(view);
			}
		}

		protected override void DisconnectHandler(UIWindow platformView)
		{
			var vc = (VirtualView.Content.Handler as IPlatformViewHandler)
							.ViewController;

			if (VirtualView.Content is Shell)
			{
				platformView.RootViewController
					.PresentedViewController.
					DismissViewController(false,
					() =>
					{
						_finishedDisconnecting.SetResult(true);
					});
			}
			else
			{
				VirtualView
					.Content
					.ToPlatform()
					.RemoveFromSuperview();

				_finishedDisconnecting.SetResult(true);
			}

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
