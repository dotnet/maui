using System;
using System.Collections.Generic;
using System.ComponentModel;
using Foundation;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Platform
{
	internal class ControlsModalWrapper : ModalWrapper, IUIAdaptivePresentationControllerDelegate
	{
		IPlatformViewHandler? _modal;
		bool _isDisposed;
		Page Page => ((Page?)_modal?.VirtualView) ?? throw new InvalidOperationException("Page cannot be null here");

		internal ControlsModalWrapper(IPlatformViewHandler modal)
		{
			_modal = modal;

			if (_modal.VirtualView is IElementConfiguration<Page> elementConfiguration)
			{
				if (elementConfiguration.On<PlatformConfiguration.iOS>()?.ModalPresentationStyle() is PlatformConfiguration.iOSSpecific.UIModalPresentationStyle style)
				{
					var result = style.ToPlatformModalPresentationStyle();

					if (!(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13)) && result == UIKit.UIModalPresentationStyle.Automatic)
					{
						result = UIKit.UIModalPresentationStyle.FullScreen;
					}

					if (result == UIKit.UIModalPresentationStyle.FullScreen)
					{
						Color modalBkgndColor = ((Page)_modal.VirtualView).BackgroundColor;

						if (modalBkgndColor?.Alpha < 1)
							result = UIKit.UIModalPresentationStyle.OverFullScreen;
					}
					ModalPresentationStyle = result;
				}
				if (PopoverPresentationController != null)
				{
					if (elementConfiguration.On<PlatformConfiguration.iOS>()?.ModalPopoverSourceView() is View popoverSourceView)
					{
						PopoverPresentationController.SourceView = popoverSourceView.ToPlatform();
					}
					if (elementConfiguration.On<PlatformConfiguration.iOS>()?.ModalPopoverRect() is System.Drawing.Rectangle rect)
					{
						if (!rect.IsEmpty)
						{
							PopoverPresentationController.SourceRect = new CoreGraphics.CGRect(rect.X, rect.Y, rect.Width, rect.Height);
						}
					}
				}
			}

			UpdateBackgroundColor();
			_ = modal?.ViewController?.View ?? throw new InvalidOperationException("View Controller Not Initialized on Modal Page");

			View!.AddSubview(modal.ViewController.View);
			TransitioningDelegate = modal.ViewController.TransitioningDelegate;
			AddChildViewController(modal.ViewController);

			modal.ViewController.DidMoveToParentViewController(this);

			if (PresentationController != null &&
				(OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13)))
			{
				PresentationController.Delegate = this;
			}

			if (modal.VirtualView is Page page)
				page.PropertyChanged += OnModalPagePropertyChanged;
		}

		[Export("presentationControllerDidDismiss:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public void DidDismiss(UIPresentationController _)
		{
			var window = (_modal?.VirtualView as Page)?.Window;
			if (window?.Page is Shell shell)
			{
				// The modal page might have a NavigationPage so it's not enough to just send
				// GotoAsync(..) we need build up what the uri will be once the last modal page is removed
				// and then submit that to shell
				var modalStack = new List<Page>(shell.CurrentItem.CurrentItem.Navigation.ModalStack);
				if (modalStack.Count > 0)
					modalStack.RemoveAt(modalStack.Count - 1);

				var result = ShellNavigationManager.GetNavigationParameters(
					shell.CurrentItem,
					shell.CurrentItem.CurrentItem,
					shell.CurrentItem.CurrentItem.CurrentItem,
					shell.CurrentItem.CurrentItem.Stack, modalStack);

				shell.NavigationManager.GoToAsync(result).FireAndForget();
			}
			else
				Page.Navigation.PopModalAsync(false).FireAndForget();
		}

		public override void DismissViewController(bool animated, Action? completionHandler)
		{
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

		// TODO: [UnsupportedOSPlatform("ios16.0")]
#pragma warning disable CA1416, CA1422
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
#pragma warning restore CA1416, CA1422

		public override bool ShouldAutomaticallyForwardRotationMethods => true;

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			
			// Defensive check to prevent crashes during iOS snapshot creation
			// when VirtualView may be disposed during app state transitions
			if (_modal?.VirtualView is Page)
			{
				_modal?.PlatformArrange(new Rect(0, 0, View!.Bounds.Width, View.Bounds.Height));
			}
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
				if (_modal?.VirtualView is Page modalPage)
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
			if (OperatingSystem.IsIOSVersionAtLeast(11))
				SetNeedsUpdateOfHomeIndicatorAutoHidden();
		}

		public override UIViewController ChildViewControllerForStatusBarStyle()
		{
			return ChildViewControllers.Last();
		}

		void OnModalPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
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
				Color modalBkgndColor = Page.BackgroundColor;
				View!.BackgroundColor = modalBkgndColor?.ToPlatform() ?? Maui.Platform.ColorExtensions.BackgroundColor;
			}
			else
			{
				View!.BackgroundColor = UIColor.Clear;
			}
		}
	}
}