using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
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
				});
			});
		}

#if MACCATALYST
		// NOTE: this test is slightly different than MemoryTests.HandlerDoesNotLeak
		// It adds a child to the Border, which is a worthwhile test case.
		[Fact("TitleBar Does Not Leak")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();
			WeakReference platformViewReference = null;
			WeakReference handlerReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
                var handler = new WindowHandler();


                var contentPage = new ContentPage();

                // Create the handler for the ContentPage
                var pageHandler = CreateHandler<PageHandler>(contentPage);
                handlerReference = new WeakReference(pageHandler);
                platformViewReference = new WeakReference(pageHandler.PlatformView);

                var rootVC = pageHandler.PlatformView;
                // var rootVC = pageHandler.PlatformView.RootViewController;

                // var windowVC = new WindowViewController(rootVC, contentPage.Window, pageHandler.MauiContext);
                // // var titleBar = new TitleBar();

                // // Get the ViewController from the Page.Handler
                // var pageViewController = (pageHandler as IPlatformViewHandler)?.ViewController;

                // if (pageViewController is not null)
                // {
                //     // Add the windowViewController to the pageViewController
                //     pageViewController.AddChildViewController(windowVC);

                //     // Add the windowViewController's view as a subview
                //     pageViewController.View.AddSubview(windowVC.View);
                // }
			});

			await AssertionExtensions.WaitForGC(handlerReference, platformViewReference);
		}
#endif

	}
}
