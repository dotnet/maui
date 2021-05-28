using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class ModalWrapper : UIViewController, IUIAdaptivePresentationControllerDelegate
	{
		IVisualElementRenderer _modal;
		bool _isDisposed;

		internal ModalWrapper(IVisualElementRenderer modal)
		{
			_modal = modal;

			var elementConfiguration = modal.Element as IElementConfiguration<Page>;
			if (elementConfiguration?.On<PlatformConfiguration.iOS>()?.ModalPresentationStyle() is PlatformConfiguration.iOSSpecific.UIModalPresentationStyle style)
			{
				var result = style.ToNativeModalPresentationStyle();

				if (!Forms.IsiOS13OrNewer && result == UIKit.UIModalPresentationStyle.Automatic)
				{
					result = UIKit.UIModalPresentationStyle.FullScreen;
				}

				if (result == UIKit.UIModalPresentationStyle.FullScreen)
				{
					Color modalBkgndColor = ((Page)_modal.Element).BackgroundColor;

					if (modalBkgndColor?.Alpha > 0)
						result = UIKit.UIModalPresentationStyle.OverFullScreen;
				}

				ModalPresentationStyle = result;
			}

			UpdateBackgroundColor();
			View.AddSubview(modal.ViewController.View);
			TransitioningDelegate = modal.ViewController.TransitioningDelegate;
			AddChildViewController(modal.ViewController);

			modal.ViewController.DidMoveToParentViewController(this);

			if (Forms.IsiOS13OrNewer)
				PresentationController.Delegate = this;

			((Page)modal.Element).PropertyChanged += OnModalPagePropertyChanged;
		}

		[Export("presentationControllerDidDismiss:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public async void DidDismiss(UIPresentationController presentationController)
		{
			await Application.Current.NavigationProxy.PopModalAsync(false);
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
			_modal?.SetElementSize(new Size(View.Bounds.Width, View.Bounds.Height));
		}

		public override void ViewWillAppear(bool animated)
		{
			if (!_isDisposed)
				UpdateBackgroundColor();

			base.ViewWillAppear(animated);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				if (_modal?.Element is Page modalPage)
				{
					modalPage.PropertyChanged -= OnModalPagePropertyChanged;
				}
				_modal = null;
			}

			base.Dispose(disposing);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			SetNeedsStatusBarAppearanceUpdate();
			if (Forms.RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden)
				SetNeedsUpdateOfHomeIndicatorAutoHidden();
		}

		public override UIViewController ChildViewControllerForStatusBarStyle()
		{
			return ChildViewControllers?.LastOrDefault();
		}

		void OnModalPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		void UpdateBackgroundColor()
		{
			if (_isDisposed)
				return;

			if (ModalPresentationStyle == UIKit.UIModalPresentationStyle.FullScreen)
			{
				Color modalBkgndColor = ((Page)_modal.Element).BackgroundColor;
				View.BackgroundColor = modalBkgndColor?.ToUIColor() ?? ColorExtensions.BackgroundColor;
			}
			else
			{
				View.BackgroundColor = UIColor.Clear;
			}
		}
	}
}