using System;
using AppKit;
using Foundation;

#pragma warning disable CA1422

namespace GraphicsTester.Skia
{
	public partial class MainWindow : NSWindow
	{
		public MainWindow(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		[Export("initWithCoder:")]
		public MainWindow(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		void Initialize()
		{
			ContentViewController = new TesterViewController();
		}

	}
}
#pragma warning restore CA1422