using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class PlatformRenderer : NSViewController
	{
		PlatformNavigation _platformNavigation;
		bool _disposed;

		internal PlatformRenderer(Platform platform)
		{
			Platform = platform;
			View = new NSView(NSApplication.SharedApplication.Windows[0].Frame);
			_platformNavigation = new PlatformNavigation(this);
		}

		public Platform Platform { get; set; }

		public PlatformNavigation Navigation => _platformNavigation;

		public override void ViewDidAppear()
		{
			Platform.DidAppear();
			base.ViewDidAppear();
		}

		public override void ViewDidLayout()
		{
			base.ViewDidLayout();
			Platform.LayoutSubviews();
			_platformNavigation?.ModalPageTracker?.LayoutSubviews();
		}

		public override void ViewWillAppear()
		{
			Platform.WillAppear();
			base.ViewWillAppear();
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_platformNavigation.Dispose();
				_platformNavigation = null;
			}
			_disposed = true;
			base.Dispose(disposing);
		}
	}
}