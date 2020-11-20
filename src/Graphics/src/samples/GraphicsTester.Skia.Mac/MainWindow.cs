using System;

using Foundation;
using AppKit;

namespace GraphicsTester.Skia
{
    public partial class MainWindow : NSWindow
    {
        public MainWindow (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        [Export ("initWithCoder:")]
        public MainWindow (NSCoder coder) : base (coder)
        {
            Initialize ();
        }

        void Initialize()
		{
            ContentViewController = new TesterViewController ();
		}
        
    }
}
