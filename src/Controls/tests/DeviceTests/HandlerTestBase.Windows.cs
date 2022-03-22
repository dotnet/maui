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
using WPoint = Windows.Foundation.Point;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase
	{
		protected bool GetIsAccessibilityElement(IViewHandler viewHandler) =>
			((AccessibilityView)((DependencyObject)viewHandler.PlatformView).GetValue(NativeAutomationProperties.AccessibilityViewProperty))
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

					MauiContext
						.Services
						.GetRequiredService<WWindow>()
						.SetWindowHandler(window, mauiContext);

					newWindowHandler = window.Handler;
					var content = window.Content.Handler.ToPlatform();
					await content.OnLoadedAsync();
					await Task.Delay(10);

					if (typeof(THandler).IsAssignableFrom(newWindowHandler.GetType()))
						await action((THandler)newWindowHandler);
					else if (typeof(THandler).IsAssignableFrom(window.Content.Handler.GetType()))
						await action((THandler)window.Content.Handler);
					else if (window.Content is ContentPage cp && typeof(THandler).IsAssignableFrom(cp.Content.Handler.GetType()))
						await action((THandler)cp.Content.Handler);
					else
						throw new Exception($"I can't work with {typeof(THandler)}");

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
						await testingRootPanel.OnLoadedAsync();
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

		protected double DistanceYFromTheBottomOfTheAppTitleBar(IElement element)
		{
			var handler = element.Handler;
			var rootManager = handler.MauiContext.GetNavigationRootManager();
			var position = element.GetLocationRelativeTo(rootManager.AppTitleBar);
			var distance = rootManager.AppTitleBar.Height - position.Value.Y;
			return distance;
		}

		MauiNavigationView GetMauiNavigationView(NavigationRootManager navigationRootManager)
		{
			return (navigationRootManager.RootView as WindowRootView).NavigationViewControl;
		}

		protected MauiNavigationView GetMauiNavigationView(IMauiContext mauiContext)
		{
			return GetMauiNavigationView(mauiContext.GetNavigationRootManager());
		}
	}
}
