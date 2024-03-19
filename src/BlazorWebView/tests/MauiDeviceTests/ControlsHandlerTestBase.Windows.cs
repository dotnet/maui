using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Collections;
using Xunit;
using NativeAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using WAppBarButton = Microsoft.UI.Xaml.Controls.AppBarButton;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using WWindow = Microsoft.UI.Xaml.Window;

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
				//var applicationContext = mauiContext.MakeApplicationScope(UI.Xaml.Application.Current);

				var appStub = new MauiAppNewWindowStub(window);
				//UI.Xaml.Application.Current.SetApplicationHandler(appStub, applicationContext);
				WWindow newWindow = null;
				try
				{
					ApplicationExtensions.CreatePlatformWindow(UI.Xaml.Application.Current, appStub, new Handlers.OpenWindowRequest());
					newWindow = window.Handler.PlatformView as WWindow;
					await runTests.Invoke();
				}
				finally
				{
					window.Handler?.DisconnectHandler();
					await Task.Delay(10);
					newWindow?.Close();
					appStub.Handler?.DisconnectHandler();
				}
			});
		}
	}
}
