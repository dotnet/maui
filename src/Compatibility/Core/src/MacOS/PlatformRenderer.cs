using AppKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	internal class PlatformRenderer : NSViewController
	{
		PlatformNavigation _platformNavigation;
		bool _disposed;

		internal PlatformRenderer(Platform platform)
		{
			Platform = platform;
#pragma warning disable CS0618 // Type or member is obsolete
			View = new NSView(NSApplication.SharedApplication.Windows[0].Frame);
#pragma warning restore CS0618 // Type or member is obsolete
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
			if (disposing)
			{
				if (!_disposed)
				{
					_platformNavigation.Dispose();
					_platformNavigation = null;
					Platform = null;
				}
				_disposed = true;
			}
			base.Dispose(disposing);
		}
	}
}