using CoreGraphics;
using System;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellFlyoutContentRenderer : UIViewController, IShellFlyoutContentRenderer
	{
		UIVisualEffectView _blurView;
		UIImageView _bgImage;
		readonly IShellContext _shellContext;
		UIContainerView _headerView;
		UIView _footerView;
		View _footer;
		ShellTableViewController _tableViewController;

		public event EventHandler WillAppear;
		public event EventHandler WillDisappear;

		public ShellFlyoutContentRenderer(IShellContext context)
		{
			_shellContext = context;
			_tableViewController = CreateShellTableViewController();
			AddChildViewController(_tableViewController);

			context.Shell.PropertyChanged += HandleShellPropertyChanged;

		}

		protected virtual ShellTableViewController CreateShellTableViewController()
		{
			return new ShellTableViewController(_shellContext, OnElementSelected);
		}

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
		}

		void UpdateFlowDirection()
		{
			_tableViewController.View.UpdateFlowDirection(_shellContext.Shell);
			_headerView.UpdateFlowDirection(_shellContext.Shell);
			_footerView.UpdateFlowDirection(_shellContext.Shell);
		}

		void UpdateFlyoutHeader()
		{
			var header = ((IShellController)_shellContext.Shell).FlyoutHeader;

			if (header == _headerView?.View)
				return;

			if(_headerView != null)
			{
				_tableViewController.HeaderView = null;
				_headerView.RemoveFromSuperview();
				_headerView.Dispose();
			}

			if (header != null)
				_headerView = new UIContainerView(((IShellController)_shellContext.Shell).FlyoutHeader);
			else
				_headerView = null;

			_tableViewController.HeaderView = _headerView;

			if(_headerView != null)
				View.AddSubview(_headerView);
		}

		void UpdateFlyoutFooter()
		{
			UpdateFlyoutFooter(((IShellController)_shellContext.Shell).FlyoutFooter);
		}

		void UpdateFlyoutFooter(View view)
		{
			if (_footer == view)
				return;

			if (_footer != null)
			{
				var oldRenderer = Platform.GetRenderer(_footer);
				var oldFooterView = _footerView;
				_tableViewController.FooterView = null;
				_footerView = null;
				oldFooterView?.RemoveFromSuperview();
				if (_footer != null)
					_footer.MeasureInvalidated -= OnFooterMeasureInvalidated;

				_footer.ClearValue(Platform.RendererProperty);
				oldRenderer?.Dispose();
			}

			_footer = view;

			if (_footer != null)
			{
				var renderer = Platform.CreateRenderer(_footer);
				_footerView = renderer.NativeView;
				Platform.SetRenderer(_footer, renderer);

				View.AddSubview(_footerView);
				_footerView.ClipsToBounds = true;
				_tableViewController.FooterView = _footerView;
				_footer.MeasureInvalidated += OnFooterMeasureInvalidated;
			}

			_tableViewController.FooterView = _footerView;

		}

		void OnFooterMeasureInvalidated(object sender, System.EventArgs e)
		{
			ReMeasureFooter();
		}

		void ReMeasureFooter()
		{
			var request = _footer.Measure(View.Frame.Width, double.PositiveInfinity, MeasureFlags.None);
			Layout.LayoutChildIntoBoundingRegion(_footer, new Rectangle(0, 0, View.Frame.Width, request.Request.Height));
			UpdateFooterPosition(_footerView.Frame.Height);
		}

		void UpdateFooterPosition()
		{
			if (_footerView == null)
				return;

			if (_footerView.Frame.Height == 0)
				ReMeasureFooter();
			else
				UpdateFooterPosition(_footerView.Frame.Height);
		}

		void UpdateFooterPosition(nfloat footerHeight)
		{
			if (_footerView == null)
				return;

			var footerWidth = View.Frame.Width;

			_footerView.Frame = new CoreGraphics.CGRect(0, View.Frame.Height - footerHeight, footerWidth, footerHeight);

			_tableViewController.LayoutParallax();
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();
			UpdateFooterPosition();
		}

		protected virtual void UpdateBackground()
		{
			var color = _shellContext.Shell.FlyoutBackgroundColor;
			View.BackgroundColor = color.ToUIColor(ColorExtensions.BackgroundColor);

			if (View.BackgroundColor.CGColor.Alpha < 1)
			{
				View.InsertSubview(_blurView, 0);
			}
			else
			{
				if (_blurView.Superview != null)
					_blurView.RemoveFromSuperview();
			}

			var brush = _shellContext.Shell.FlyoutBackground;
			View.UpdateBackground(brush);

			UpdateFlyoutBgImageAsync();
		}

		async void UpdateFlyoutBgImageAsync()
		{
			// image
			var imageSource = _shellContext.Shell.FlyoutBackgroundImage;
			if (imageSource == null || !_shellContext.Shell.IsSet(Shell.FlyoutBackgroundImageProperty))
			{
				_bgImage.RemoveFromSuperview();
				_bgImage.Image?.Dispose();
				_bgImage.Image = null;
				return;
			}

			using (var nativeImage = await imageSource.GetNativeImageAsync())
			{
				if (View == null)
					return;

				if (nativeImage == null)
				{
					_bgImage?.RemoveFromSuperview();
					return;
				}

				_bgImage.Image = nativeImage;
				switch (_shellContext.Shell.FlyoutBackgroundImageAspect)
				{
					default:
					case Aspect.AspectFit:
						_bgImage.ContentMode = UIViewContentMode.ScaleAspectFit;
						break;
					case Aspect.AspectFill:
						_bgImage.ContentMode = UIViewContentMode.ScaleAspectFill;
						break;
					case Aspect.Fill:
						_bgImage.ContentMode = UIViewContentMode.ScaleToFill;
						break;
				}

				if (_bgImage.Superview != View)
					View.InsertSubview(_bgImage, 0);
			}
		}

		public UIViewController ViewController => this;

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_tableViewController.LayoutParallax();
			_blurView.Frame = View.Bounds;
			_bgImage.Frame = View.Bounds;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.AddSubview(_tableViewController.View);

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

			UpdateBackground();
			UpdateFlowDirection();
		}

		public override void ViewWillAppear(bool animated)
		{
			UpdateFlowDirection();
			base.ViewWillAppear(animated);
			WillAppear?.Invoke(this, EventArgs.Empty);
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			WillDisappear?.Invoke(this, EventArgs.Empty);
		}

		void OnElementSelected(Element element)
		{
			((IShellController)_shellContext.Shell).OnFlyoutItemSelected(element);
		}
	}
}
