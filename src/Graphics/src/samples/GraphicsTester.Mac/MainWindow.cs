
using System;
using Foundation;
using AppKit;
using CoreGraphics;

namespace GraphicsTester.Mac
{
    public partial class MainWindow : AppKit.NSWindow
    {
        private TesterView testerView;

        // Called when created from unmanaged code
        public MainWindow (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
		
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public MainWindow (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
		
        // Shared initialization code
        void Initialize ()
        {
            var bounds = Frame;

            testerView = new TesterView (new CGRect (0, 0, bounds.Width, bounds.Height));
            testerView.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;

            ContentView.AutoresizesSubviews = true;
            ContentView.AddSubview (testerView);
        }
    }
}

