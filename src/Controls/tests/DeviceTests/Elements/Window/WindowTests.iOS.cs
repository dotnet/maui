using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
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

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WillTerminateDestroysMainPageWindowOnce(bool sceneDisconnectedFirst)
		{
			SetupBuilder();

			var page = new ContentPage();
			var window = new Window(page);
			var app = (ApplicationStub)ApplicationServices.GetRequiredService<IApplication>();
#pragma warning disable CS0618 // Type or member is obsolete
			await InvokeOnMainThreadAsync(() => app.MainPage = page);
#pragma warning restore CS0618 // Type or member is obsolete

			var destroyingCount = 0;
			window.Destroying += (_, _) => destroyingCount++;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, _ =>
			{
				var originalPlatformApplication = IPlatformApplication.Current;
				var shouldDisconnectKeyboardLifecycle = KeyboardAutoManagerScroll.ShouldDisconnectLifecycle;
				IPlatformApplication.Current = new PlatformApplicationStub(ApplicationServices, app);

				try
				{
					// A force-closed scene app gets sceneDidDisconnect: before applicationWillTerminate:
					if (sceneDisconnectedFirst)
						((IWindow)window).Destroying();

					// The MainPage window stays in Application.Windows after it is destroyed
					Assert.Same(window, app.Windows.Single());

					var exception = Record.Exception(() =>
						ApplicationServices.InvokeLifecycleEvents<iOSLifecycle.WillTerminate>(del => del(UIApplication.SharedApplication)));

					Assert.Null(exception);
					Assert.Equal(1, destroyingCount);
				}
				finally
				{
					IPlatformApplication.Current = originalPlatformApplication;

					// WillTerminate disconnects the keyboard observers; restore them for the tests that follow
					KeyboardAutoManagerScroll.ShouldDisconnectLifecycle = shouldDisconnectKeyboardLifecycle;
					if (!shouldDisconnectKeyboardLifecycle)
						KeyboardAutoManagerScroll.Connect();
				}

				return Task.CompletedTask;
			});
		}

		class PlatformApplicationStub : IPlatformApplication
		{
			public PlatformApplicationStub(IServiceProvider services, IApplication application)
			{
				Services = services;
				Application = application;
			}

			public IServiceProvider Services { get; }

			public IApplication Application { get; }
		}
	}
}
