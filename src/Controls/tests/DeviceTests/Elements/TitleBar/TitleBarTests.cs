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

			// Create a new WindowViewController, attach it to a viewController, and see if it disposes properly
			await InvokeOnMainThreadAsync(() =>
			{
				var window = new Window
				{
					Page = new ContentPage(),
					TitleBar = new TitleBar()
					{
						BackgroundColor = Colors.Red,
						Icon = new FileImageSource { File = "blue.png" },
						Content = new Label () { Text = "Hello, TitleBar!" },
					},
				};

				var contentPage = new ContentPage();

				var pageHandler = CreateHandler<PageHandler>(contentPage);
				handlerReference = new WeakReference(pageHandler);
				platformViewReference = new WeakReference(pageHandler.PlatformView);

				if ((pageHandler as IPlatformViewHandler)?.ViewController is UIViewController vc)
				{
					var titleBarViewController = new UIViewController();
					var windowVC = new WindowViewController(titleBarViewController, window, pageHandler.MauiContext);

					vc.AddChildViewController(windowVC);
					vc.View.AddSubview(windowVC.View);
				}
				else
				{
					Assert.Fail("Could not get the pageHandler ViewController");
				}
			});

			await AssertionExtensions.WaitForGC(handlerReference, platformViewReference);
		}
	}
}
#endif
