using CoreGraphics;
using System;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellFlyoutContentRenderer : UIViewController, IShellFlyoutContentRenderer
	{
		UIVisualEffectView _blurView;
		readonly IShellContext _shellContext;
		UIContainerView _headerView;
		ShellTableViewController _tableViewController;

		public event EventHandler WillAppear;
		public event EventHandler WillDisappear;

		public ShellFlyoutContentRenderer(IShellContext context)
		{
			var header = ((IShellController)context.Shell).FlyoutHeader;
			if (header != null)
				_headerView = new UIContainerView(((IShellController)context.Shell).FlyoutHeader);
			_tableViewController = new ShellTableViewController(context, _headerView, OnElementSelected);

			AddChildViewController(_tableViewController);

			context.Shell.PropertyChanged += HandleShellPropertyChanged;

			_shellContext = context;
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutBackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		protected virtual void UpdateBackgroundColor()
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
		}

		public UIViewController ViewController => this;

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_tableViewController.LayoutParallax();
			_blurView.Frame = View.Bounds;
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

			UpdateBackgroundColor();
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