using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests
	{
#if IOS
		[Theory]
		[InlineData(typeof(ContentPage))]
		[InlineData(typeof(NavigationPage))]
		[InlineData(typeof(TabbedPage))]
		[InlineData(typeof(FlyoutPage))]
		[InlineData(typeof(Shell))]
		public async Task StatusBarThemeFlowsThroughRootController(Type rootPageType)
		{
			SetupBuilder();

			var testCase = new WindowPageSwapTestCase(rootPageType);
			var rootPage = testCase.GetNextPageType();
			var window = new Window(rootPage);

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(testCase.Page);

				var rootController = ((IPlatformViewHandler)rootPage.Handler).ViewController;

				Assert.Same(window, rootPage.Window);
				Assert.Equal(UIStatusBarStyle.Default, GetStatusBarStyleProvider(rootController).PreferredStatusBarStyle());

				window.StatusBarTheme = StatusBarTheme.Dark;
				var styleProvider = GetStatusBarStyleProvider(rootController);
				if (rootPageType == typeof(Shell))
					Assert.IsType<Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer>(styleProvider);
				Assert.Equal(UIStatusBarStyle.LightContent, styleProvider.PreferredStatusBarStyle());

				window.StatusBarTheme = StatusBarTheme.Light;
				Assert.Equal(UIStatusBarStyle.DarkContent, GetStatusBarStyleProvider(rootController).PreferredStatusBarStyle());

				window.StatusBarTheme = StatusBarTheme.Default;
				Assert.Equal(UIStatusBarStyle.Default, GetStatusBarStyleProvider(rootController).PreferredStatusBarStyle());
			});
		}

		static UIViewController GetStatusBarStyleProvider(UIViewController controller)
		{
			var visited = new HashSet<UIViewController>();

			while (visited.Add(controller))
			{
				var child = controller.ChildViewControllerForStatusBarStyle();
				if (child is null)
					return controller;

				controller = child;
			}

			throw new InvalidOperationException("The status bar style controller hierarchy contains a cycle.");
		}
#endif
	}
}
