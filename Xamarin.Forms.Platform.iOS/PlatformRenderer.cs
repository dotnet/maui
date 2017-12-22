using System;
using System.Diagnostics;
using System.Linq;
using UIKit;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Platform.iOS
{
	internal class ModalWrapper : UIViewController
	{
		IVisualElementRenderer _modal;

		internal ModalWrapper(IVisualElementRenderer modal)
		{
			_modal = modal;

			View.BackgroundColor = UIColor.White;
			View.AddSubview(modal.ViewController.View);
			TransitioningDelegate = modal.ViewController.TransitioningDelegate;
			AddChildViewController(modal.ViewController);

			modal.ViewController.DidMoveToParentViewController(this);
		}

		public override void DismissViewController(bool animated, Action completionHandler)
		{
			if (PresentedViewController == null)
			{
				// After dismissing a UIDocumentMenuViewController, (for instance, if a WebView with an Upload button
				// is asking the user for a source (camera roll, etc.)), the view controller accidentally calls dismiss
				// again on itself before presenting the UIImagePickerController; this leaves the UIImagePickerController
				// without an anchor to the view hierarchy and it doesn't show up. This appears to be an iOS bug.

				// We can work around it by ignoring the dismiss call when PresentedViewController is null. 
				return;
			}

			base.DismissViewController(animated, completionHandler);
		}

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
		}

		public override bool ShouldAutomaticallyForwardRotationMethods => true;

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			if (_modal != null)
				_modal.SetElementSize(new Size(View.Bounds.Width, View.Bounds.Height));
		}

		public override void ViewWillAppear(bool animated)
		{
			View.BackgroundColor = UIColor.White;
			base.ViewWillAppear(animated);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_modal = null;
			base.Dispose(disposing);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			SetNeedsStatusBarAppearanceUpdate();
		}

		public override UIViewController ChildViewControllerForStatusBarStyle()
		{
			return ChildViewControllers?.LastOrDefault();
		}
	}

	internal class PlatformRenderer : UIViewController
	{
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
			return (UIViewController)Platform.GetRenderer(this.Platform.Page);
		}

		public override UIViewController ChildViewControllerForStatusBarStyle()
		{
			return ChildViewControllers?.LastOrDefault();
		}

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
		}

		public override bool ShouldAutomaticallyForwardRotationMethods => true;

		public override void ViewDidAppear(bool animated)
		{
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
			View.BackgroundColor = UIColor.White;
			Platform.WillAppear();
			base.ViewWillAppear(animated);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			SetNeedsStatusBarAppearanceUpdate();
		}
	}
}