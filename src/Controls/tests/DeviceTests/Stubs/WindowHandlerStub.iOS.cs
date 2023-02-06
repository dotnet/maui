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
		IView _currentView;

		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		void UpdateContent(UIWindow platformView)
		{
			ReplaceCurrentView(_currentView, platformView, () =>
			{
				var view = VirtualView.Content.ToPlatform(MauiContext);
				_currentView = VirtualView.Content;

				var vc =
					(_currentView.Handler as IPlatformViewHandler)
						.ViewController;


				bool fireEvents = !(VirtualView as Window).IsActivated;
				if (VirtualView.Content is IFlyoutView)
				{
					vc.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;

					if (fireEvents)
						VirtualView.Created();

					PlatformView.RootViewController.PresentViewController(vc, false,
						() =>
						{
							if (fireEvents)
								VirtualView.Activated();
						});
				}
				else
				{
					if (fireEvents)
						VirtualView.Created();

					//AssertionExtensions.FindContentViewController().AddChildViewController(vc);

					var contentView = AssertionExtensions.FindContentView();
					contentView.AddSubview(view);

					if (fireEvents)
						VirtualView.Activated();
				}
			}, false);
		}

		// If the content on the window is updated as part of the test
		// this logic takes care of removing the old view and then adding the incoming
		// view to the testing surface
		void ReplaceCurrentView(IView view, UIWindow platformView, Action finishedClosing, bool disconnecting)
		{
			if (view == null)
			{
				finishedClosing?.Invoke();
				return;
			}

			var vc = (view.Handler as IPlatformViewHandler).ViewController;
			var virtualView = VirtualView;

			if (view is IFlyoutView)
			{
				var pvc = platformView?.RootViewController?.PresentedViewController;
				// This means shell never got to the point of being preesented
				if (pvc == null)
				{
					finishedClosing?.Invoke();
					return;
				}

				if (disconnecting)
					virtualView.Deactivated();

				pvc.DismissViewController(false,
					() =>
					{
						finishedClosing.Invoke();

						if (disconnecting)
							virtualView.Destroying();
					});
			}
			else
			{
				vc.RemoveFromParentViewController();

				view
					.ToPlatform()
					.RemoveFromSuperview();

				finishedClosing.Invoke();

				if (disconnecting)
				{
					virtualView.Deactivated();
					virtualView.Destroying();
				}
			}

		}

		public static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			handler.UpdateContent(handler.PlatformView);
		}

		protected override void DisconnectHandler(UIWindow platformView)
		{
			ReplaceCurrentView(VirtualView.Content, platformView, () => _finishedDisconnecting.SetResult(true), true);
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
