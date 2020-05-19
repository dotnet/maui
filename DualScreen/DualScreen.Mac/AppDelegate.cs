using AppKit;
using Foundation;
using System.Maui;
using System.Maui.Platform.MacOS;

namespace DualScreen.Mac
{
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow window;
        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            window.Title = "System.Maui on Mac!"; // choose your own Title here
            window.TitleVisibility = NSWindowTitleVisibility.Hidden;
        }

        public override NSWindow MainWindow
        {
            get { return window; }
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            System.Maui.Maui.Init();
            LoadApplication(new DualScreen.App());
            base.DidFinishLaunching(notification);
        }
    }
}
