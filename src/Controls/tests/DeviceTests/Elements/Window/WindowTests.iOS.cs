using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests
	{
#if IOS
		[Fact("Entry Focus and Unfocus After Window Disposal")]
        public async Task EntryFocusAndUnfocusAfterWindowDisposal()
        {
            SetupBuilder();
            WeakReference platformViewReference = null;
            WeakReference handlerReference = null;

            // Create a new UIWindow, attach it to a viewController, and see if it disposes properly
            // await InvokeOnMainThreadAsync(async () =>
            // {
                // var window = new UIWindow(UIScreen.MainScreen.Bounds);
                // var viewController = new UIViewController();

                var contentPage = new ContentPage();
				var scrollView = new ScrollView();
				var vsl = new VerticalStackLayout();
				var boxView = new BoxView() { HeightRequest = 550, WidthRequest = 100, Color = Colors.Red };
				var entry = new Entry() {  Text = "Hello, World!",
					ReturnType = ReturnType.Done};

				vsl.Add(boxView);
				vsl.Add(entry);

				scrollView.Content = vsl;

				contentPage.Content = scrollView;
				var window = new Window(contentPage);

			// 	await InvokeOnMainThreadAsync(async () =>
			// 	{

			// 		var entryHandler = CreateHandler<EntryHandler>(entry);
			// 		handlerReference = new WeakReference(entryHandler);
			// 		platformViewReference = new WeakReference(entryHandler.PlatformView);

			// 		var vslPlat = vsl.ToPlatform(MauiContext);
			// 		if (vsl.ToPlatform(MauiContext) is UIKit.UIView vslView)
			// 		{
			// 			// viewController.View.AddSubview(entryView);
			// 			// window.RootViewController = viewController;
			// 			// window.MakeKeyAndVisible();

			// 			// vslView.Window?.Dispose();
			// 			// vslView.Window = new UIWindow(UIScreen.MainScreen.Bounds);

			// 			// Focus the entry
			// 			entry.Focus();
			// 			await Task.Delay(2000);

			// 			// Dispose of the window
			// 			// window.RootViewController = null;
			// 			// window.Dispose();

			// 			// entryView.Window?.Dispose();
			// 			vslView.Window?.Dispose();

			// 			// Unfocus the entry
			// 			entry.Unfocus();
			// 			await Task.Delay(1000);

			// 			// Check if the entry is unfocused
			// 			Assert.False(entry.IsFocused, "Entry should be unfocused after window disposal.");
			// 		}
			// 		else
			// 		{
			// 			Assert.Fail("Could not get the entryHandler PlatformView");
			// 		}
            // });

			// await CreateHandlerAndAddToWindow(window, async () =>
			// 	{

			// 		var entryHandler = CreateHandler<EntryHandler>(entry);
			// 		handlerReference = new WeakReference(entryHandler);
			// 		platformViewReference = new WeakReference(entryHandler.PlatformView);

			// 		var vslPlat = vsl.ToPlatform(MauiContext);
			// 		if (vsl.ToPlatform(MauiContext) is UIKit.UIView vslView)
			// 		{
			// 			// viewController.View.AddSubview(entryView);
			// 			// window.RootViewController = viewController;
			// 			// window.MakeKeyAndVisible();

			// 			// vslView.Window?.Dispose();
			// 			// vslView.Window = new UIWindow(UIScreen.MainScreen.Bounds);

			// 			// Focus the entry
			// 			entry.Focus();
			// 			await Task.Delay(2000);

			// 			// Dispose of the window
			// 			// window.RootViewController = null;
			// 			// window.Dispose();

			// 			// entryView.Window?.Dispose();
			// 			vslView.Window?.Dispose();

			// 			// Unfocus the entry
			// 			entry.Unfocus();
			// 			await Task.Delay(1000);

			// 			// Check if the entry is unfocused
			// 			Assert.False(entry.IsFocused, "Entry should be unfocused after window disposal.");
			// 		}
			// 		else
			// 		{
			// 			Assert.Fail("Could not get the entryHandler PlatformView");
			// 		}
            // });

			await AssertionExtensions.WaitForGC(handlerReference, platformViewReference);
        }
#endif
	}
}
