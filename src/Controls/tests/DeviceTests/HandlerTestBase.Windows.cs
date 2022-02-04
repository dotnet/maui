using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;
using WNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WWindow = Microsoft.UI.Xaml.Window;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase
	{
		protected bool GetIsAccessibilityElement(IViewHandler viewHandler) =>
			((AccessibilityView)((DependencyObject)viewHandler.NativeView).GetValue(NativeAutomationProperties.AccessibilityViewProperty))
			== AccessibilityView.Content;

		Task RunWindowTest<THandler>(IWindow window, Func<THandler, Task> action)
			where THandler : class, IElementHandler
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				var testingRootPanel = (WPanel)MauiProgram.CurrentWindow.Content;
				IElementHandler newWindowHandler = null;
				NavigationRootManager navigationRootManager = null;
				try
				{
					var scopedContext = new MauiContext(MauiContext.Services);
					scopedContext.AddWeakSpecific(MauiProgram.CurrentWindow);
					var mauiContext = scopedContext.MakeScoped(true);
					navigationRootManager = mauiContext.GetNavigationRootManager();
					navigationRootManager.UseCustomAppTitleBar = false;

					newWindowHandler = window.ToHandler(mauiContext);
					var content = window.Content.Handler.ToPlatform();
					await content.LoadedAsync();
					await Task.Delay(10);

					if (typeof(THandler).IsAssignableFrom(newWindowHandler.GetType()))
						await action((THandler)newWindowHandler);
					else if (typeof(THandler).IsAssignableFrom(window.Content.Handler.GetType()))
						await action((THandler)window.Content.Handler);

				}
				finally
				{
					if (navigationRootManager != null)
						navigationRootManager.Disconnect();

					if (newWindowHandler != null)
						newWindowHandler.DisconnectHandler();

					// Set the root window panel back to the testing panel
					if (testingRootPanel != null && MauiProgram.CurrentWindow.Content != testingRootPanel)
					{
						MauiProgram.CurrentWindow.Content = testingRootPanel;
						await testingRootPanel.LoadedAsync();
						await Task.Delay(10);
					}
				}
			});
		}

		protected IEnumerable<WNavigationViewItem> GetNavigationViewItems(MauiNavigationView navigationView)
		{
			if (navigationView.MenuItems?.Count > 0)
			{
				foreach (var menuItem in navigationView.MenuItems)
				{
					if (menuItem is WNavigationViewItem item)
						yield return item;
				}
			}
			else if (navigationView.MenuItemsSource != null && navigationView.TopNavMenuItemsHost != null)
			{
				var itemCount = navigationView.TopNavMenuItemsHost.ItemsSourceView.Count;
				for (int i = 0; i < itemCount; i++)
				{
					UI.Xaml.UIElement uIElement = navigationView.TopNavMenuItemsHost.TryGetElement(i);

					if (uIElement is WNavigationViewItem item)
						yield return item;
				}
			}
		}


		MauiNavigationView GetMauiNavigationView(NavigationRootManager navigationRootManager)
		{
			return (navigationRootManager.RootView as WindowRootView).NavigationViewControl;
		}

		protected MauiNavigationView GetMauiNavigationView(IMauiContext mauiContext)
		{
			return GetMauiNavigationView(mauiContext.GetNavigationRootManager());
		}

		protected Task CreateHandlerAndAddToWindow<THandler>(IElement view, Func<THandler, Task> action)
			where THandler : class, IElementHandler
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				IWindow window = null;

				if (view is IWindow w)
				{
					window = w;
				}
				else if (view is Page page)
				{
					window = new Controls.Window(page);
				}
				else
				{
					window = new Controls.Window(new ContentPage() { Content = (View)view });
				}

				await RunWindowTest<THandler>(window, (handler) => action(handler as THandler));
			});
		}
	}
}
