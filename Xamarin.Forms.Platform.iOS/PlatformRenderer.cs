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
			AddChildViewController(modal.ViewController);

			modal.ViewController.DidMoveToParentViewController(this);
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
	}
}