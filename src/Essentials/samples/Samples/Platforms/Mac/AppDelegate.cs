using AppKit;
using CoreGraphics;
using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xamarin.Forms.Platform.MacOS;

namespace Samples.Mac
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : FormsApplicationDelegate
    {
        static App formsApp;

        NSWindow window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var screenSize = NSScreen.MainScreen.Frame.Size;
            var rect = new CGRect(0, 0, 1024, 768);
            rect.Offset((screenSize.Width - rect.Width) / 2, (screenSize.Height - rect.Height) / 2);

            window = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "Microsoft.Maui.Essentials",
                TitleVisibility = NSWindowTitleVisibility.Hidden,
            };
        }

        public override NSWindow MainWindow => window;

        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();

            LoadApplication(formsApp ??= new App());

            base.DidFinishLaunching(notification);
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
    }
}
