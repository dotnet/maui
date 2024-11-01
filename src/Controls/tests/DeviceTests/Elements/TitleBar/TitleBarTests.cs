#if MACCATALYST
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using AppKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Window)]
	public partial class TitleBarTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Window, WindowHandler>();
					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Image, ImageHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		[Fact("TitleBar Does Not Leak")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();
			WeakReference platformViewReference = null;
			WeakReference handlerReference = null;

			NSToolbar originalToolbar = null;

			// Get the original Toolbar on the window
			await InvokeOnMainThreadAsync(() =>
			{
				var root = UIApplication.SharedApplication.GetKeyWindow().RootViewController;

				if (root?.View?.Window is UIWindow window)
				{
					var platformTitleBar = window?.WindowScene?.Titlebar;

					if (platformTitleBar is not null)
					{
						originalToolbar = platformTitleBar.Toolbar;
					}
					else
					{
						Assert.Fail("Could not find platform Titlebar");
					}
				}
				else
				{
					Assert.Fail("Could not find original window");
				}
			});

			// Create a new WindowViewController, attach it to a viewController,
			// and see if it disposes properly
			await InvokeOnMainThreadAsync(() =>
			{
				var window = new Window
				{
					Page = new ContentPage(),
					// Page = contentPage,
					TitleBar = new TitleBar()
					{
						BackgroundColor = Colors.Red,
						Icon = new FileImageSource { File = "dotnet_bot.png" },
						Content = new Label () { Text = "Hello, TitleBar!" },
					},
				};

				var contentPage = new ContentPage();

				var pageHandler = CreateHandler<PageHandler>(contentPage);
				handlerReference = new WeakReference(pageHandler);
				platformViewReference = new WeakReference(pageHandler.PlatformView);

				if ((pageHandler as IPlatformViewHandler)?.ViewController is UIViewController vc)
				{
					var windowHandler = CreateHandler<WindowHandler>(window);

					var titleBarViewController = new UIViewController();
					var windowVC = new WindowViewController(titleBarViewController, window, window.MauiContext);

					vc.AddChildViewController(windowVC);
					vc.View.AddSubview(windowVC.View);
				}
				else
				{
					Assert.Fail("Could not get the pageHandler ViewController");
				}
			});

			await AssertionExtensions.WaitForGC(handlerReference, platformViewReference);

			// Restore the original Toolbar on the window
			await InvokeOnMainThreadAsync(() =>
			{
				var root = UIApplication.SharedApplication.GetKeyWindow().RootViewController;

				if (root?.View?.Window is UIWindow window)
				{
					var platformTitleBar = window?.WindowScene?.Titlebar;

					if (platformTitleBar is not null)
					{
						platformTitleBar.Toolbar = originalToolbar;
						platformTitleBar.TitleVisibility = UITitlebarTitleVisibility.Visible;
					}
					else
					{
						Assert.Fail("Could not find the platform Titlebar");
					}
				}
				else
				{
					Assert.Fail("Could not find window to restore");
				}
			});
		}
#endif
	}
}
