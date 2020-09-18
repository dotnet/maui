using AppKit;
using CoreGraphics;
using Foundation;

namespace Sample.Mac
{
	[Register("AppDelegate")]
	public class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate()
		{
		}

		public override void DidFinishLaunching(NSNotification notification)
		{
			var mainWindow = NSApplication.SharedApplication.KeyWindow;

			if (mainWindow != null)
			{
				var frame = mainWindow.Frame;
				frame.Size = new CGSize(1024, 768);
				mainWindow.SetFrame(frame, true);
			}
		}

		public override void WillTerminate(NSNotification notification)
		{
			// Insert code here to tear down your application
		}
	}
}