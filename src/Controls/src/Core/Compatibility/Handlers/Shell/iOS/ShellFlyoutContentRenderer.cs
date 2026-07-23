#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellFlyoutContentRenderer : UIViewController, IShellFlyoutContentRenderer
	{
		CGSize _previousBounds;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The blur view is owned by this renderer and released in Dispose.")]
		UIVisualEffectView _blurView;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The background image view is owned by this renderer and released in Dispose.")]
		UIImageView _bgImage;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The background image result owns the current native image and is disposed when replaced or when this renderer is disposed.")]
		IDisposable _bgImageResult;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The shell context is retained for the renderer lifetime and released after Shell.PropertyChanged is unsubscribed in Dispose.")]
		IShellContext _shellContext;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The header container is owned by this renderer and disposed when replaced or in Dispose.")]
		UIContainerView _headerView;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The footer container is owned by this renderer and disposed when replaced or in Dispose.")]
		UIContainerView _footerView;
		View _footer;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The table view controller is a child controller owned by this renderer and released in Dispose.")]
		ShellTableViewController _tableViewController;
		ShellFlyoutLayoutManager _shellFlyoutContentManager;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "The view ordering cache only references renderer-owned subviews and is cleared in Dispose.")]
		UIView[] _uIViews;
		bool _isDisposed;

		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Event subscribers are cleared in Dispose(bool).")]
		public event EventHandler WillAppear;

		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "Event subscribers are cleared in Dispose(bool).")]
		public event EventHandler WillDisappear;

		const short HeaderIndex = 0;
		const short FooterIndex = 1;
		const short ContentIndex = 2;
		const short BlurIndex = 3;
		const short BackgroundImageIndex = 4;

		public ShellFlyoutContentRenderer(IShellContext context)
		{
			_uIViews = new UIView[5];
			_shellContext = context;
			_tableViewController = CreateShellTableViewController();
			_shellFlyoutContentManager = _tableViewController?.ShellFlyoutContentManager;
			AddChildViewController(_tableViewController);

			context.Shell.PropertyChanged += HandleShellPropertyChanged;
		}

		protected virtual ShellTableViewController CreateShellTableViewController()
		{
			return new ShellTableViewController(_shellContext, OnElementSelected);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "The Shell.PropertyChanged subscription is removed in Dispose before the shell context is released.")]
		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.IsOneOf(
				Shell.FlyoutBackgroundColorProperty,
				Shell.FlyoutBackgroundProperty,
				Shell.FlyoutBackgroundImageProperty,
				Shell.FlyoutBackgroundImageAspectProperty))
				UpdateBackground();
			else if (e.Is(VisualElement.FlowDirectionProperty))
				UpdateFlowDirection();
			else if (e.IsOneOf(
				Shell.FlyoutHeaderProperty,
				Shell.FlyoutHeaderTemplateProperty))
			{
				UpdateFlyoutHeader();
			}
			else if (e.IsOneOf(
				Shell.FlyoutFooterProperty,
				Shell.FlyoutFooterTemplateProperty))
			{
				UpdateFlyoutFooter();
			}
			else if (e.IsOneOf(
				Shell.FlyoutContentProperty,
				Shell.FlyoutContentTemplateProperty))
			{
				UpdateFlyoutContent();
			}
		}

		void UpdateFlowDirection()
		{
			_tableViewController.View.UpdateFlowDirection(_shellContext.Shell);
			_headerView?.UpdateFlowDirection(_shellContext.Shell);
			_footerView?.UpdateFlowDirection(_shellContext.Shell);
		}

		void UpdateFlyoutHeader()
		{
			var header = ((IShellController)_shellContext.Shell).FlyoutHeader;

			if (header == _headerView?.View)
				return;

			int previousIndex = GetPreviousIndex(_headerView);
			if (_headerView is not null)
			{
				_tableViewController.HeaderView = null;
				_headerView.RemoveFromSuperview();
				_headerView.Dispose();
			}

			if (header is not null)
			{
				_headerView = new ShellFlyoutHeaderContainer(((IShellController)_shellContext.Shell).FlyoutHeader);

				// Resolve MatchParent to the Shell's concrete FlowDirection before calling UpdateFlowDirection.
				// Shell sub-elements have a disconnected MAUI visual tree, so MatchParent cannot traverse up
				// to the Shell automatically. This is a one-way mutation consistent with existing codebase
				// patterns; if Shell.FlowDirection changes at runtime, UpdateFlowDirection() will still
				// update the native UIView correctly because it uses the Shell as context.
				if (header.FlowDirection == FlowDirection.MatchParent)
				{
					header.FlowDirection = _shellContext.Shell.FlowDirection;
				}

				_headerView.UpdateFlowDirection(_shellContext.Shell);
			}
			else
				_headerView = null;

			_uIViews[HeaderIndex] = _headerView;
			AddViewInCorrectOrder(_headerView, previousIndex);
			_tableViewController.HeaderView = _headerView;
		}

		void UpdateFlyoutFooter()
		{
			UpdateFlyoutFooter(((IShellController)_shellContext.Shell).FlyoutFooter);
		}

		void UpdateFlyoutFooter(View view)
		{
			if (_footer == view)
				return;

			int previousIndex = GetPreviousIndex(_footerView);
			if (_footer is not null)
			{
				var oldFooterView = _footerView;
				_footer.MeasureInvalidated -= OnFooterMeasureInvalidated;
				_tableViewController.FooterView = null;
				_footerView = null;
				_uIViews[FooterIndex] = null;
				oldFooterView?.RemoveFromSuperview();
				oldFooterView?.Dispose();
			}

			_footer = view;

			if (_footer is not null)
			{
				_footerView = new UIContainerView(_footer);
				_uIViews[FooterIndex] = _footerView;
				AddViewInCorrectOrder(_footerView, previousIndex);

				_footerView.ClipsToBounds = true;
				_footer.MeasureInvalidated += OnFooterMeasureInvalidated;

				// Same approach as header — use Shell as context without mutating _footer.FlowDirection,
				// so runtime Shell.FlowDirection changes continue to resolve correctly.
				_footerView.UpdateFlowDirection(_shellContext.Shell);
			}

			_tableViewController.FooterView = _footerView;

		}

		int GetPreviousIndex(UIView oldView)
		{
			if (oldView is null)
				return -1;

			return Array.IndexOf(View.Subviews, oldView);
		}

		void AddViewInCorrectOrder(UIView newView, int previousIndex)
		{
			if (newView is null)
				return;

			if (Array.IndexOf(View.Subviews, newView) >= 0)
				return;

			if (previousIndex >= 0 && View.Subviews.Length <= previousIndex)
			{
				View.InsertSubview(newView, previousIndex);
				return;
			}

			int startingIndex = Array.IndexOf(_uIViews, newView);
			for (int i = startingIndex - 1; i >= 0; i--)
			{
				var topView = _uIViews[i];
				if (topView is null)
					continue;

				if (Array.IndexOf(View.Subviews, topView) >= 0)
				{
					View.InsertSubviewBelow(newView, topView);
					return;
				}
			}

			View.AddSubview(newView);
		}

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "The footer MeasureInvalidated subscription is removed when the footer is replaced and in Dispose.")]
		void OnFooterMeasureInvalidated(object sender, System.EventArgs e)
		{
			ReMeasureFooter();
		}

		void ReMeasureFooter()
		{
			var size = _footerView?.SizeThatFits(new CGSize(View.Frame.Width, double.PositiveInfinity));
			if (size is not null)
				UpdateFooterPosition(size.Value.Height);
		}

		void UpdateFooterPosition()
		{
			if (_footerView is null)
				return;

			if (double.IsNaN(_footerView.MeasuredHeight))
				ReMeasureFooter();
			else
				UpdateFooterPosition((nfloat)_footerView.MeasuredHeight);
		}

		void UpdateFooterPosition(nfloat footerHeight)
		{
			if (_footerView is null && !nfloat.IsNaN(footerHeight))
				return;

			var footerWidth = View.Frame.Width;

			_footerView.Frame = new CGRect(0, View.Frame.Height - footerHeight - View.SafeAreaInsets.Bottom, footerWidth, footerHeight);

			_tableViewController.LayoutParallax();
		}

		public override void ViewWillLayoutSubviews()
		{
			if (_isDisposed)
				return;

			base.ViewWillLayoutSubviews();
			UpdateFooterPosition();
			UpdateFlyoutContent();
			var currentSize = View.Bounds.Size;
			if (_previousBounds != currentSize)
			{
				// Whenever the layout changes, the background needs to be redrawn to match the new view dimensions. This is especially important for gradients.
				UpdateBackground();
				_previousBounds = currentSize;
			}
		}

		protected virtual void UpdateBackground()
		{
			var color = _shellContext.Shell.FlyoutBackgroundColor;
			var brush = _shellContext.Shell.FlyoutBackground;
			int previousIndex = GetPreviousIndex(_blurView);
			var backgroundImage = View.GetBackgroundImage(brush);
			View.BackgroundColor = backgroundImage is not null ? UIColor.FromPatternImage(backgroundImage) : color?.ToPlatform() ?? Maui.Platform.ColorExtensions.BackgroundColor;

			if (View.BackgroundColor.CGColor.Alpha < 1)
			{
				AddViewInCorrectOrder(_blurView, previousIndex);
			}
			else
			{
				if (_blurView.Superview is not null)
					_blurView.RemoveFromSuperview();
			}

			UpdateFlyoutBgImageAsync();
		}

		void UpdateFlyoutBgImageAsync()
		{
			if (_isDisposed || _shellContext?.Shell is not Shell shell || _bgImage is null)
				return;

			// Image
			var imageSource = shell.FlyoutBackgroundImage;
			if (imageSource is null || !shell.IsSet(Shell.FlyoutBackgroundImageProperty))
			{
				_bgImage.RemoveFromSuperview();
				_bgImage.Image = null;
				_bgImageResult?.Dispose();
				_bgImageResult = null;
				return;
			}

			var mauiContext = _shellContext.Shell.FindMauiContext();
			if (mauiContext is null)
				return;

			var bgImage = _bgImage;
			imageSource.LoadImage(mauiContext, result =>
			{
				if (_isDisposed)
				{
					result?.Dispose();
					return;
				}

				var nativeImage = result?.Value;
				var view = ViewIfLoaded;
				if (nativeImage is null ||
					view is null ||
					!ReferenceEquals(bgImage, _bgImage) ||
					_shellContext?.Shell is not Shell currentShell ||
					!ReferenceEquals(imageSource, currentShell.FlyoutBackgroundImage))
				{
					result?.Dispose();
					return;
				}

				int previousIndex = GetPreviousIndex(bgImage);
				var previousResult = _bgImageResult;
				_bgImageResult = result;
				bgImage.Image = nativeImage;
				previousResult?.Dispose();
				switch (currentShell.FlyoutBackgroundImageAspect)
				{
					default:
					case Aspect.AspectFit:
						bgImage.ContentMode = UIViewContentMode.ScaleAspectFit;
						break;
					case Aspect.AspectFill:
						bgImage.ContentMode = UIViewContentMode.ScaleAspectFill;
						break;
					case Aspect.Fill:
						bgImage.ContentMode = UIViewContentMode.ScaleToFill;
						break;
				}

				if (!_isDisposed && ReferenceEquals(bgImage, _bgImage) && bgImage.Superview != view)
					AddViewInCorrectOrder(bgImage, previousIndex);
			});
		}

		public UIViewController ViewController => this;

		public override void ViewDidLayoutSubviews()
		{
			if (_isDisposed)
				return;

			base.ViewDidLayoutSubviews();

			_tableViewController.LayoutParallax();
			_blurView.Frame = View.Bounds;
			_bgImage.Frame = View.Bounds;
		}

		public override void ViewDidLoad()
		{
			if (_isDisposed)
				return;

			base.ViewDidLoad();


			UpdateFlyoutHeader();
			UpdateFlyoutFooter();

			_tableViewController.TableView.BackgroundView = null;
			_tableViewController.TableView.BackgroundColor = UIColor.Clear;

			var effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Regular);
			_blurView = new UIVisualEffectView(effect);
			_blurView.Frame = View.Bounds;
			_bgImage = new UIImageView
			{
				Frame = View.Bounds,
				ContentMode = UIViewContentMode.ScaleAspectFit,
				ClipsToBounds = true
			};

			_uIViews[BlurIndex] = _blurView;
			_uIViews[BackgroundImageIndex] = _bgImage;

			UpdateFlowDirection();
		}

		void UpdateFlyoutContent()
		{
			var view = (_shellContext.Shell as IShellController).FlyoutContent;

			var previousIndex = GetPreviousIndex(_shellFlyoutContentManager.ContentView);
			if (view is not null)
			{
				_shellFlyoutContentManager.SetCustomContent(view);
			}
			else
			{
				_shellFlyoutContentManager.SetDefaultContent(_tableViewController.TableView);
			}

			_uIViews[ContentIndex] = _shellFlyoutContentManager.ContentView;
			AddViewInCorrectOrder(_uIViews[ContentIndex], previousIndex);
			_shellFlyoutContentManager.UpdateHeaderSize();
		}

		public override void ViewWillAppear(bool animated)
		{
			if (_isDisposed)
				return;

			UpdateFlowDirection();
			base.ViewWillAppear(animated);
			WillAppear?.Invoke(this, EventArgs.Empty);
		}

		public override void ViewWillDisappear(bool animated)
		{
			if (_isDisposed)
				return;

			base.ViewWillDisappear(animated);

			WillDisappear?.Invoke(this, EventArgs.Empty);
		}

		void OnElementSelected(Element element)
		{
			((IShellController)_shellContext.Shell).OnFlyoutItemSelected(element);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing)
			{
				if (_shellContext?.Shell is not null)
					_shellContext.Shell.PropertyChanged -= HandleShellPropertyChanged;

				if (_footer is not null)
					_footer.MeasureInvalidated -= OnFooterMeasureInvalidated;

				_headerView?.Disconnect();
				_footerView?.Disconnect();
				_headerView?.RemoveFromSuperview();
				_footerView?.RemoveFromSuperview();
				_blurView?.RemoveFromSuperview();
				_bgImage?.RemoveFromSuperview();
				if (_bgImage is not null)
					_bgImage.Image = null;
				_bgImageResult?.Dispose();
				_bgImage?.Dispose();
				_blurView?.Dispose();
				_headerView?.Dispose();
				_footerView?.Dispose();
				_tableViewController?.RemoveFromParentViewController();
				_tableViewController?.Dispose();
				WillAppear = null;
				WillDisappear = null;
			}

			_bgImageResult = null;
			_bgImage = null;
			_blurView = null;
			_headerView = null;
			_footerView = null;
			_footer = null;
			_tableViewController = null;
			_shellFlyoutContentManager = null;
			_shellContext = null;
			_uIViews = null;

			base.Dispose(disposing);
		}
	}
}
