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
		TaskCompletionSource _finishedDisconnecting = new TaskCompletionSource();
		public Task FinishedDisconnecting => _finishedDisconnecting.Task;
		IView _currentView;
		public bool IsDisconnected { get; private set; }

		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		void UpdateContent(UIWindow platformView)
		{
			ReplaceCurrentView(_currentView, platformView, () =>
			{
				if (IsDisconnected)
					return;

				var virtualView = VirtualView;
				var view = virtualView.Content.ToPlatform(MauiContext);
				_currentView = virtualView.Content;

				bool fireEvents = !(virtualView as Window)?.IsActivated ?? true;
				if (virtualView.Content is IFlyoutView)
				{
					var vc =
						((IPlatformViewHandler)_currentView.Handler).ViewController;

					_ = vc ?? throw new Exception($"{_currentView} needs to have a ViewController");

					vc.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;

					if (fireEvents)
						FireWindowEvent(virtualView, (window) => !window.IsCreated, () => virtualView.Created());

					PlatformView.RootViewController.PresentViewController(vc, false,
						() =>
						{
							if (fireEvents)
								FireWindowEvent(virtualView, (window) => !window.IsActivated, () => virtualView.Activated());
						});
				}
				else
				{
					if (fireEvents)
						FireWindowEvent(virtualView, (window) => !window.IsCreated, () => virtualView.Created());

					var contentView = AssertionExtensions.FindContentView();
					contentView.AddSubview(view);

					if (fireEvents)
						FireWindowEvent(virtualView, (window) => !window.IsActivated, () => virtualView.Activated());
				}
			}, false);
		}

		// If the content on the window is updated as part of the test
		// this logic takes care of removing the old view and then adding the incoming
		// view to the testing surface
		async void ReplaceCurrentView(IView view, UIWindow platformView, Action finishedClosing, bool disconnecting)
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
				if (pvc is null)
				{
					finishedClosing?.Invoke();
					return;
				}

				if (disconnecting)
				{
					FireWindowEvent(virtualView, (window) => window.IsActivated, () => virtualView.Deactivated());
				}

				pvc.PresentingViewController.DismissViewController(false,
					() =>
					{
						finishedClosing.Invoke();

						if (disconnecting)
						{
							FireWindowEvent(virtualView, (window) => !window.IsDestroyed, () => virtualView.Destroying());
						}
					});
			}
			else
			{
				// With a real app the Modals will get dismissed automatically
				// When the presenting view controller (RootView) gets replaced with a new
				// Window.Page
				// So, we're just simulating that cleanup here ourselves.
				var presentedVC =
					platformView?.RootViewController?.PresentedViewController ??
					vc.PresentedViewController;

				if (presentedVC is ModalWrapper mw)
				{
					await mw.PresentingViewController.DismissViewControllerAsync(false);
				}

				vc.RemoveFromParentViewController();

				view
					.ToPlatform()
					.RemoveFromSuperview();

				finishedClosing.Invoke();

				if (disconnecting &&
					FireWindowEvent(virtualView, (window) => window.IsActivated, () => virtualView.Deactivated()))
				{
					virtualView.Destroying();
				}
			}

		}

		bool FireWindowEvent(IWindow platformView, Func<Window, bool> condition, Action action)
		{
			if ((platformView is Window window &&
				condition.Invoke(window)) ||
				platformView is not Window)
			{
				action.Invoke();
				return true;
			}

			return false;
		}

		public static void MapContent(WindowHandlerStub handler, IWindow window)
		{
			handler.UpdateContent(handler.PlatformView);
		}

		protected override void DisconnectHandler(UIWindow platformView)
		{
			ReplaceCurrentView(VirtualView.Content, platformView, () => _finishedDisconnecting.TrySetResult(), true);
			base.DisconnectHandler(platformView);
			IsDisconnected = true;
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
