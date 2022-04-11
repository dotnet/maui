using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
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
						window.Handler.DisconnectHandler();
					}
				}
			});
		}

		protected bool IsBackButtonVisible(IElementHandler handler)
		{
			if (handler is ShellRenderer renderer)
			{
				if (renderer.ChildViewControllers[0] is ShellItemRenderer sir)
				{
					if (sir.ChildViewControllers[0] is ShellSectionRenderer ssr)
					{
						// Nothing has been pushed to the stack
						if (ssr.ChildViewControllers.Length == 1)
							return false;

						var activeVC =
							ssr.ChildViewControllers[ssr.ChildViewControllers.Length - 1];

						return !activeVC.NavigationItem.HidesBackButton;
					}
				}
			}

			throw new NotImplementedException();
		}
	}
}
