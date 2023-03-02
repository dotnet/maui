using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ControlsHandlerTestBase
	{

		Task SetupWindowForTests<THandler>(IWindow window, Func<Task> runTests, IMauiContext mauiContext = null)
		where THandler : class, IElementHandler
		{
			mauiContext ??= MauiContext;
			return InvokeOnMainThreadAsync(async () =>
			{
				IElementHandler windowHandler = null;
				try
				{
					windowHandler = window.ToHandler(mauiContext);
					await runTests.Invoke();
				}
				finally
				{
					if (windowHandler is not null)
					{
						if (window is Window controlsWindow && controlsWindow.Navigation.ModalStack.Count > 0)
						{
							for (int i = 0; i < controlsWindow.Navigation.ModalStack.Count; i++)
							{
								var page = controlsWindow.Navigation.ModalStack[i];
								if (page.Handler is IPlatformViewHandler pvh &&
									pvh.ViewController?.ParentViewController is ModalWrapper modal &&
									modal.PresentingViewController is not null)
								{
									await modal.PresentingViewController.DismissViewControllerAsync(false);
								}
							}
						}
					}

					if (windowHandler is WindowHandlerStub whs)
					{
						if (!whs.IsDisconnected)
							window.Handler.DisconnectHandler();

						await whs.FinishedDisconnecting;
					}
					else
						window.Handler?.DisconnectHandler();

					var vc =
						(window.Content?.Handler as IPlatformViewHandler)?
							.ViewController;

					vc?.RemoveFromParentViewController();
					vc?.View?.RemoveFromSuperview();

					var rootView = UIApplication.SharedApplication
									.GetKeyWindow()
									.RootViewController;

					bool dangling = false;

					while (rootView?.PresentedViewController is not null)
					{
						dangling = true;
						await rootView.DismissViewControllerAsync(false);
					}

					Assert.False(dangling, "Test failed to cleanup modals");
				}
			});
		}

		internal ModalWrapper GetModalWrapper(Page modalPage)
		{
			var pageVC = (modalPage.Handler as IPlatformViewHandler).ViewController;
			return (ModalWrapper)pageVC.ParentViewController;
		}

		protected bool IsBackButtonVisible(IElementHandler handler)
		{
			var vcs = GetActiveChildViewControllers(handler);

			if (vcs.Length <= 1)
				return false;

			return !vcs[vcs.Length - 1].NavigationItem.HidesBackButton;
		}

		protected object GetTitleView(IElementHandler handler)
		{
			var activeVC = GetVisibleViewController(handler);
			if (activeVC.NavigationItem.TitleView is
				ShellPageRendererTracker.TitleViewContainer tvc)
			{
				return tvc.View.Handler.PlatformView;
			}

			return null;
		}

		UIViewController[] GetActiveChildViewControllers(IElementHandler handler)
		{
			if (handler is ShellRenderer renderer)
			{
				if (renderer.ChildViewControllers[0] is ShellItemRenderer sir)
				{
					if (sir.ChildViewControllers[0] is ShellSectionRenderer ssr)
					{
						return ssr.ChildViewControllers;
					}
				}
			}

			var containerVC = (handler as IPlatformViewHandler).ViewController;
			var view = handler.VirtualView.Parent;

			while (containerVC is null && view is not null)
			{
				containerVC = (view?.Handler as IPlatformViewHandler).ViewController;
				view = view?.Parent;
			}

			if (containerVC is null)
				return new UIViewController[0];

			return new[] { containerVC };
		}

		UIViewController GetVisibleViewController(IElementHandler handler)
		{
			var vcs = GetActiveChildViewControllers(handler);
			return vcs[vcs.Length - 1];
		}
	}
}
