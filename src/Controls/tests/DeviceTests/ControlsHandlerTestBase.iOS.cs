using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ControlsHandlerTestBase
	{
		protected bool GetIsAccessibilityElement(IViewHandler viewHandler)
		{
			var platformView = ((UIView)viewHandler.PlatformView);
			return platformView.IsAccessibilityElement;
		}

		protected bool GetExcludedWithChildren(IViewHandler viewHandler)
		{
			var platformView = ((UIView)viewHandler.PlatformView);
			return platformView.AccessibilityElementsHidden;
		}

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

			throw new NotImplementedException();
		}

		UIViewController GetVisibleViewController(IElementHandler handler)
		{
			var vcs = GetActiveChildViewControllers(handler);
			return vcs[vcs.Length - 1];
		}
	}
}
