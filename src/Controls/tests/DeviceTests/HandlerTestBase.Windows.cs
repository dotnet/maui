using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using WPanel = Microsoft.UI.Xaml.Controls.Panel;
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

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase
	{
		protected bool GetIsAccessibilityElement(IViewHandler viewHandler) =>
			((AccessibilityView)((DependencyObject)viewHandler.NativeView).GetValue(NativeAutomationProperties.AccessibilityViewProperty)) 
			== AccessibilityView.Content;


		Task RunWindowTest(IWindow window, Func<WindowHandler, Task> action)
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

					// This will swap out the root windows panel with the panel we are going to be testing with
					newWindowHandler = window.ToHandler(mauiContext);
					var content = (WPanel)MauiProgram.CurrentWindow.Content;
					navigationRootManager = mauiContext.GetNavigationRootManager();
					await content.LoadedAsync();
					await Task.Delay(10);
					await action((WindowHandler)newWindowHandler);
				}
				finally
				{
					if (navigationRootManager != null)
						navigationRootManager.Disconnect();

					if (newWindowHandler != null)
						newWindowHandler.DisconnectHandler();

					// Set the root window panel back to the testing panel
					if (testingRootPanel != null)
					{
						MauiProgram.CurrentWindow.Content = testingRootPanel;
						await testingRootPanel.LoadedAsync();
						await Task.Delay(10);
					}
				}
			});
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
				if (view is IWindow window)
				{
					await RunWindowTest(window, (handler) => action(handler as THandler));
					return;
				}

				WFrameworkElement frameworkElement = null;
				var content = (WPanel)MauiContext.Services.GetService<WWindow>().Content;
				try
				{
					var mauiContext = MauiContext.MakeScoped(true);
					var handler = CreateHandler<THandler>(view, mauiContext);
					frameworkElement = (WFrameworkElement)handler.NativeView;
					content.Children.Add(frameworkElement);
					await frameworkElement.LoadedAsync();
					await Task.Delay(10);
					await action(handler);
				}
				finally
				{
					if (frameworkElement != null)
					{
						content.Children.Remove(frameworkElement);
						await frameworkElement.UnloadedAsync();
						await Task.Delay(10);
					}
				}
			});
		}
	}
}
