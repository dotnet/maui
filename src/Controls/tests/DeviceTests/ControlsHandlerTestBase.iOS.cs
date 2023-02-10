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
				try
				{
					_ = window.ToHandler(mauiContext);
					await runTests.Invoke();
				}
				finally
				{
					if (window.Handler != null)
					{
						if (window is Controls.Window controlsWindow && controlsWindow.Navigation.ModalStack.Count > 0)
						{
							var modalCount = controlsWindow.Navigation.ModalStack.Count;

							for (int i = 0; i < modalCount; i++)
								await controlsWindow.Navigation.PopModalAsync();
						}

						if (window.Handler is WindowHandlerStub whs)
						{
							window.Handler.DisconnectHandler();
							await whs.FinishedDisconnecting;
						}
						else
							window.Handler.DisconnectHandler();

					}
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

		protected bool IsNavigationBarVisible(IElementHandler handler)
		{
			var platformToolbar = GetPlatformToolbar(handler);
			return platformToolbar?.Window != null;
		}

		protected object GetTitleView(IElementHandler handler)
		{
			var activeVC = GetVisibleViewController(handler);
			if (activeVC.NavigationItem.TitleView is
				ShellPageRendererTracker.TitleViewContainer tvc)
			{
				return tvc.Subviews[0];
			}

			return null;
		}

		UIViewController[] GetActiveChildViewControllers(IElementHandler handler)
		{
			if (handler is IWindowHandler wh)
			{
				handler = wh.VirtualView.Content.Handler;
			}

			if (handler is ShellRenderer renderer)
			{
				if (renderer.ChildViewControllers[0] is ShellItemRenderer sir)
				{
					return sir.SelectedViewController.ChildViewControllers;
				}
			}

			var containerVC = (handler as IPlatformViewHandler).ViewController;
			var view = handler.VirtualView.Parent;

			while (containerVC == null && view != null)
			{
				containerVC = (view?.Handler as IPlatformViewHandler).ViewController;
				view = view?.Parent;
			}

			if (containerVC == null)
				return new UIViewController[0];

			return new[] { containerVC };
		}

		UIViewController GetVisibleViewController(IElementHandler handler)
		{
			var vcs = GetActiveChildViewControllers(handler);
			return vcs[vcs.Length - 1];
		}

		protected UINavigationBar GetPlatformToolbar(IElementHandler handler)
		{
			var visibleController = GetVisibleViewController(handler);
			if (visibleController is UINavigationController nc)
				return nc.NavigationBar;

			var navController = visibleController.NavigationController;
			return navController?.NavigationBar;
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
