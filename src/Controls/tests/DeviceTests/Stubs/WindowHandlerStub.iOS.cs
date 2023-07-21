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
		WorkspaceViewController _workSpace;
		public bool IsDisconnected { get; private set; }

		public static IPropertyMapper<IWindow, WindowHandlerStub> WindowMapper = new PropertyMapper<IWindow, WindowHandlerStub>(WindowHandler.Mapper)
		{
			[nameof(IWindow.Content)] = MapContent
		};

		class WorkspaceViewController : UIViewController
		{
			public WorkspaceViewController()
			{
				ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
			}

			public override void ViewDidLayoutSubviews()
			{
				base.ViewDidLayoutSubviews();
				if (View.Subviews.Length > 0)
					View.Subviews[0].Frame = View.Bounds;
			}
		}

		void UpdateContent()
		{
			bool needsToPush = false;
			if (_workSpace is null)
			{
				needsToPush = true;
				_workSpace = new WorkspaceViewController();
			}

			for(int i = _workSpace.ChildViewControllers.Length - 1; i >= 0; i--)
			{
				_workSpace.ChildViewControllers[i].View.RemoveFromSuperview();
				_workSpace.ChildViewControllers[i].RemoveFromParentViewController();
			}

			_currentView = VirtualView.Content;
			var view = _currentView.ToPlatform(MauiContext);
			if (needsToPush)
			{
				bool fireEvents = !(VirtualView as Window)?.IsActivated ?? true;
				var vc =
					((IPlatformViewHandler)_currentView.Handler).ViewController;

				_ = vc ?? throw new Exception($"{_currentView} needs to have a ViewController");

				PlatformView.RootViewController.PresentViewController(_workSpace, false,
					() =>
					{
						_workSpace.AddChildViewController(vc);
						_workSpace.View.AddSubview(vc.View);
						if (fireEvents)
							FireWindowEvent(VirtualView, (window) => !window.IsActivated, () => VirtualView.Activated());
					});
			}
			else
			{
				var vc =
					((IPlatformViewHandler)_currentView.Handler).ViewController;

				_ = vc ?? throw new Exception($"{_currentView} needs to have a ViewController");
				_workSpace.AddChildViewController(vc);
				_workSpace.View.AddSubview(vc.View);
			}
		}

		// If the content on the window is updated as part of the test
		// this logic takes care of removing the old view and then adding the incoming
		// view to the testing surface
		void Disconnect(IView view, UIWindow platformView, Action finishedClosing)
		{
			if (view == null)
			{
				finishedClosing?.Invoke();
				return;
			}

			if (_workSpace is not null)
			{
				for (int i = _workSpace.ChildViewControllers.Length - 1; i >= 0; i--)
				{
					_workSpace.ChildViewControllers[i].View.RemoveFromSuperview();
					_workSpace.ChildViewControllers[i].RemoveFromParentViewController();
				}
			}

			var vc = (view.Handler as IPlatformViewHandler).ViewController;
			var virtualView = VirtualView;
			var pvc = platformView?.RootViewController?.PresentedViewController;

			// This means shell never got to the point of being preesented
			if (pvc is null || _workSpace is null)
			{
				finishedClosing?.Invoke();
				return;
			}

			FireWindowEvent(virtualView, (window) => window.IsActivated, () => virtualView.Deactivated());

			pvc.PresentingViewController.DismissViewController(false,
				() =>
				{
					finishedClosing.Invoke();
					FireWindowEvent(virtualView, (window) => !window.IsDestroyed, () => virtualView.Destroying());
				});

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
			handler.UpdateContent();
		}

		protected override void DisconnectHandler(UIWindow platformView)
		{
			Disconnect(VirtualView.Content, platformView, () => _finishedDisconnecting.TrySetResult());
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
