
using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace GraphicsTester.Mac
{
	public partial class MainWindow : AppKit.NSWindow
	{
		// Called when created from unmanaged code
		public MainWindow(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public MainWindow(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		// Shared initialization code
		void Initialize()
		{
			ContentViewController = new TesterViewController();
		}
	}
}
