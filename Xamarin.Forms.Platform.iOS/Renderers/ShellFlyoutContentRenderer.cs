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
		ShellTableViewController _tableViewController;

		public event EventHandler WillAppear;
		public event EventHandler WillDisappear;

		public ShellFlyoutContentRenderer(IShellContext context)
		{
			_shellContext = context;

			var header = ((IShellController)context.Shell).FlyoutHeader;
			if (header != null)
				_headerView = new UIContainerView(((IShellController)context.Shell).FlyoutHeader);

			_tableViewController = CreateShellTableViewController();

			AddChildViewController(_tableViewController);

			context.Shell.PropertyChanged += HandleShellPropertyChanged;

		}

		protected virtual ShellTableViewController CreateShellTableViewController()
		{
			return new ShellTableViewController(_shellContext, _headerView, OnElementSelected);
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.IsOneOf(
				Shell.FlyoutBackgroundColorProperty, 
				Shell.FlyoutBackgroundImageProperty,
				Shell.FlyoutBackgroundImageAspectProperty))
				UpdateBackground();
		}

		protected virtual void UpdateBackground()
		{
			var color = _shellContext.Shell.FlyoutBackgroundColor;
			View.BackgroundColor = color.ToUIColor(Color.White);

			if (View.BackgroundColor.CGColor.Alpha < 1)
			{
				View.InsertSubview(_blurView, 0);
			}
			else
			{
				if (_blurView.Superview != null)
					_blurView.RemoveFromSuperview();
			}

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
			if (_headerView != null)
				View.AddSubview(_headerView);

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
		}

		public override void ViewWillAppear(bool animated)
		{
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
