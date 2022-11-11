using System;
using System.Linq;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete]
	internal class PlatformRenderer : UIViewController
	{
		bool _disposed;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		internal PlatformRenderer(Platform platform)
		{
			Platform = platform;
		}

		public Platform Platform { get; set; }

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			if ((ChildViewControllers != null) && (ChildViewControllers.Length > 0))
			{
				return ChildViewControllers[0].GetSupportedInterfaceOrientations();
			}
			return base.GetSupportedInterfaceOrientations();
		}

		public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
		{
			if ((ChildViewControllers != null) && (ChildViewControllers.Length > 0))
			{
				return ChildViewControllers[0].PreferredInterfaceOrientationForPresentation();
			}
			return base.PreferredInterfaceOrientationForPresentation();
		}

		public override UIViewController ChildViewControllerForStatusBarHidden()
		{
			return (UIViewController)Platform.GetRenderer(Platform.Page);
		}

		public override UIViewController ChildViewControllerForStatusBarStyle()
		{
			return ChildViewControllers?.LastOrDefault();
		}

		public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden
		{
			get => (UIViewController)Platform.GetRenderer(Platform.Page);
		}
#pragma warning disable CA1416, CA1422  // TODO: The API has [UnsupportedOSPlatform("ios6.0")]
		public override bool ShouldAutorotate()
		{
			if ((ChildViewControllers != null) && (ChildViewControllers.Length > 0))
			{
				return ChildViewControllers[0].ShouldAutorotate();
			}
			return base.ShouldAutorotate();
		}

		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{

			if ((ChildViewControllers != null) && (ChildViewControllers.Length > 0))
			{
				return ChildViewControllers[0].ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
			}
			return base.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
#pragma warning restore CA1416, CA1422
		}

		public override bool ShouldAutomaticallyForwardRotationMethods => true;

		public override void ViewDidAppear(bool animated)
		{
			// For some reason iOS calls this after it's already been disposed
			// while it's being replaced on the Window.RootViewController with a new MainPage
			if (!_disposed)
				Platform.DidAppear();

			base.ViewDidAppear(animated);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			Platform.LayoutSubviews();
		}

		public override void ViewWillAppear(bool animated)
		{
			// For some reason iOS calls this after it's already been disposed
			// while it's being replaced on the Window.RootViewController with a new MainPage
			if (!_disposed)
			{
				View.BackgroundColor = ColorExtensions.BackgroundColor;
				Platform.WillAppear();
			}

			base.ViewWillAppear(animated);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			SetNeedsStatusBarAppearanceUpdate();
			if (OperatingSystem.IsIOSVersionAtLeast(11))
				SetNeedsUpdateOfHomeIndicatorAutoHidden();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				Platform.UnsubscribeFromAlertsAndActionsSheets();
				Platform.CleanUpPages();
			}

			base.Dispose(disposing);
		}
	}
}
