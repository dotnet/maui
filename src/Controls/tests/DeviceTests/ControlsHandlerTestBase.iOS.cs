using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
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
					if (windowHandler is WindowHandlerStub windowHandlerStub)
					{
						if (windowHandlerStub.IsDisconnected)
							await windowHandlerStub.FinishedDisconnecting;
					}

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

		internal ControlsModalWrapper GetModalWrapper(Page modalPage)
		{
			var pageVC = (modalPage.Handler as IPlatformViewHandler).ViewController;
			return (ControlsModalWrapper)pageVC.ParentViewController;
		}

		protected bool IsBackButtonVisible(IElementHandler handler)
		{
			var toolbar = GetPlatformToolbar(handler);
			return toolbar?.HasBackButton() ?? false;
		}

		protected bool IsNavigationBarVisible(IElementHandler handler)
		{
			var platformToolbar = GetPlatformToolbar(handler);
			return platformToolbar?.Window is not null;
		}

		protected object GetTitleView(IElementHandler handler)
		{
			var activeVC = GetVisibleViewController(handler);
			if (activeVC?.NavigationItem?.TitleView is
				ShellPageRendererTracker.TitleViewContainer tvc)
			{
				return tvc.Subviews[0];
			}

			return null;
		}

		UIViewController[] GetActiveChildViewControllers(IElementHandler handler)
		{
			if (handler.VirtualView is IWindow window)
			{
				handler = window.Content.Handler;
			}

			if (handler.VirtualView is Page page)
				handler = page.GetCurrentPage()?.Handler;

			if (handler is null)
				return Array.Empty<UIViewController>();

			var navControllerResponder = (handler.PlatformView as UIView).FindResponder<UINavigationController>();

			if (navControllerResponder?.ChildViewControllers is not null)
				return navControllerResponder.ChildViewControllers;

			var contentResponder = (handler.PlatformView as UIView).FindResponder<ContainerViewController>();

			if (contentResponder?.NavigationController is not null)
				return contentResponder.NavigationController.ChildViewControllers;

			return Array.Empty<UIViewController>();
		}

		UIViewController GetVisibleViewController(IElementHandler handler)
		{
			var vcs = GetActiveChildViewControllers(handler);
			if (vcs.Length == 0)
				return null;

			return vcs[vcs.Length - 1];
		}

		protected UINavigationBar GetPlatformToolbar(IElementHandler handler)
		{
			var visibleController = GetVisibleViewController(handler);

			if (visibleController is null)
				return null;

			if (visibleController is UINavigationController nc)
				return nc.NavigationBar;

			var navController = visibleController.NavigationController;

			if (navController?.NavigationBar is UINavigationBar bar)
				return bar;

			return null;
		}

		protected Size GetTitleViewExpectedSize(IElementHandler handler)
		{
			var titleContainer = GetPlatformToolbar(handler).FindDescendantView<UIView>(result =>
			{
				return result.Class.Name?.Contains("UINavigationBarTitleControl", StringComparison.OrdinalIgnoreCase) == true;
			});

			if (!OperatingSystem.IsIOSVersionAtLeast(16))
			{
				titleContainer = titleContainer ?? GetPlatformToolbar(handler).FindDescendantView<UIView>(result =>
				{
					return result.Class.Name?.Contains("TitleViewContainer", StringComparison.OrdinalIgnoreCase) == true;
				});
			}

			_ = titleContainer ?? throw new Exception("Unable to Locate TitleView Container");

			return new Size(titleContainer.Frame.Width, titleContainer.Frame.Height);
		}

		protected string GetToolbarTitle(IElementHandler handler)
		{
			var toolbar = GetPlatformToolbar(handler);
			return AssertionExtensions.GetToolbarTitle(toolbar);
		}

		protected string GetBackButtonText(IElementHandler handler)
		{
			var toolbar = GetPlatformToolbar(handler);
			return AssertionExtensions.GetBackButtonText(toolbar);
		}
	}
}
