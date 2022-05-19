using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase
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

		Task RunWindowTest<THandler>(IWindow window, Func<THandler, Task> action)
			where THandler : class, IElementHandler
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				try
				{
					_ = window.ToHandler(MauiContext);
					IView content = window.Content;

					if (content is IPageContainer<Page> pc)
						content = pc.CurrentPage;

					await OnLoadedAsync(content as VisualElement);

					if (typeof(THandler).IsAssignableFrom(window.Handler.GetType()))
						await action((THandler)window.Handler);
					else if (typeof(THandler).IsAssignableFrom(window.Content.Handler.GetType()))
						await action((THandler)window.Content.Handler);
					else if (window.Content is ContentPage cp && typeof(THandler).IsAssignableFrom(cp.Content.Handler.GetType()))
						await action((THandler)cp.Content.Handler);
					else
						throw new Exception($"I can't work with {typeof(THandler)}");
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
			if ( activeVC.NavigationItem.TitleView is
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
