#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellFlyoutContentRenderer : UIViewController, IShellFlyoutContentRenderer
	{
		UIVisualEffectView _blurView;
		UIImageView _bgImage;
		readonly IShellContext _shellContext;
		UIContainerView _headerView;
		UIContainerView _footerView;
		View _footer;
		ShellTableViewController _tableViewController;
		ShellFlyoutLayoutManager _shellFlyoutContentManager;
		UIView[] _uIViews;
		public event EventHandler WillAppear;
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
				_headerView = new ShellFlyoutHeaderContainer(((IShellController)_shellContext.Shell).FlyoutHeader);
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
				var oldRenderer = (IPlatformViewHandler)_footer.Handler;
				var oldFooterView = _footerView;
				_tableViewController.FooterView = null;
				_footerView?.Disconnect();
				_footerView = null;
				_uIViews[FooterIndex] = null;
				oldFooterView?.RemoveFromSuperview();

				_footer.Handler = null;
				oldRenderer?.DisconnectHandler();
			}

			_footer = view;

			if (_footer is not null)
			{
				_footerView = new UIContainerView(_footer);
				_uIViews[FooterIndex] = _footerView;
				AddViewInCorrectOrder(_footerView, previousIndex);

				_footerView.ClipsToBounds = true;
				_footer.MeasureInvalidated += OnFooterMeasureInvalidated;
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

		void OnFooterMeasureInvalidated(object sender, System.EventArgs e)
		{
			ReMeasureFooter();
		}

		void ReMeasureFooter()
		{
			var size = _footerView?.UpdateSize(new CGSize(View.Frame.Width, double.PositiveInfinity));
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

			_footerView.Frame = new CoreGraphics.CGRect(0, View.Frame.Height - footerHeight, footerWidth, footerHeight);

			_tableViewController.LayoutParallax();
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();
			UpdateFooterPosition();
			UpdateFlyoutContent();
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
			// Image
			var imageSource = _shellContext.Shell.FlyoutBackgroundImage;
			if (imageSource is null || !_shellContext.Shell.IsSet(Shell.FlyoutBackgroundImageProperty))
			{
				_bgImage.RemoveFromSuperview();
				_bgImage.Image?.Dispose();
				_bgImage.Image = null;
				return;
			}

			var mauiContext = _shellContext.Shell.FindMauiContext();
			if (mauiContext is null)
				return;

			imageSource.LoadImage(mauiContext, result =>
			{
				var nativeImage = result?.Value;

				if (View is null || nativeImage is null)
					return;

				int previousIndex = GetPreviousIndex(_bgImage);
				if (nativeImage is null ||
					_shellContext.Shell.FlyoutBackgroundImage != imageSource)
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
				{
					AddViewInCorrectOrder(_bgImage, previousIndex);
				}
			});
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

			UpdateBackground();
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
