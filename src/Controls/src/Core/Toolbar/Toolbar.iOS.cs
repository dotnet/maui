using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls
{
	public partial class Toolbar
	{
		NavigationManager? NavigationManager => Handler.MauiContext?.GetNavigationManager();

		UINavigationController? NavigationController => NavigationManager?.NavigationController;

		public static void MapIsVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			if (toolbar == null)
			{
				return;
			}

			if (toolbar?.NavigationController == null)
			{
				throw new NullReferenceException("NavigationController is null.");
			}

			if (toolbar.NavigationController.NavigationBarHidden == toolbar.IsVisible)
			{
				// TODO: the view underneath this jumps up when the toolbar is hidden and down when it's shown
				// rather than smoothly animating the transition. It's not clear how to fix this
				// Also, when the toolbar is hidden and another page is pushed, the toolbar for the page that's about to hide
				// gets its toolbar back right before the animation of the push of the new page
				toolbar.NavigationController.SetNavigationBarHidden(!toolbar.IsVisible, true);
			}
		}

		public static void MapBackButtonVisible(IToolbarHandler handler, Toolbar toolbar)
		{
			if (toolbar.NavigationController == null)
			{
				throw new NullReferenceException("NavigationController is null.");
			}

			var navigationItem = toolbar.NavigationController.TopViewController.NavigationItem;
			if (navigationItem.HidesBackButton == !toolbar.BackButtonVisible)
			{
				return;
			}

			navigationItem.HidesBackButton = !toolbar.BackButtonVisible;
		}

		public static void MapTitleIcon(IToolbarHandler arg1, Toolbar arg2)
		{

		}
	}

	internal class NavigationTitleAreaContainer : UIView
	{
		View? _view;
		IPlatformViewHandler? _child;
		UIImageView? _icon;
		bool _disposed;

		public NavigationTitleAreaContainer(View view, UINavigationBar bar) : base(bar.Bounds)
		{
			TranslatesAutoresizingMaskIntoConstraints = false;

			if (view != null)
			{
				_view = view;

				if (_view.Parent is null)
				{
					_view.ParentSet += OnTitleViewParentSet;
				}
				else
				{
					SetupTitleView();
				}
			}

			ClipsToBounds = true;
		}

		void OnTitleViewParentSet(object? sender, EventArgs e)
		{
			if (sender is View view)
			{
				view.ParentSet -= OnTitleViewParentSet;
			}

			SetupTitleView();
		}

		void SetupTitleView()
		{
			var mauiContext = _view?.FindMauiContext();
			if (_view is not null && mauiContext is not null)
			{
				var platformView = _view.ToPlatform(mauiContext);
				_child = (IPlatformViewHandler)_view.Handler!;
				AddSubview(platformView);
			}
		}

		public override CGSize IntrinsicContentSize => UILayoutFittingExpandedSize;

		nfloat IconHeight => _icon?.Frame.Height ?? 0;
		nfloat IconWidth => _icon?.Frame.Width ?? 0;

		nfloat ToolbarHeight
		{
			get
			{
				return Superview?.Bounds.Height ?? 0;
			}
		}

		public override CGRect Frame
		{
			get => base.Frame;
			set
			{
				if (Superview != null)
				{
					value.Height = ToolbarHeight;
				}

				base.Frame = value;
			}
		}

		public UIImageView Icon
		{
			set
			{
				_icon?.RemoveFromSuperview();

				_icon = value;

				if (_icon != null)
				{
					AddSubview(_icon);
				}
			}
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			return new SizeF(size.Width, ToolbarHeight);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (Frame == CGRect.Empty || Frame.Width >= 10000 || Frame.Height >= 10000)
			{
				return;
			}

			nfloat toolbarHeight = ToolbarHeight;

			double height = Math.Min(toolbarHeight, Bounds.Height);

			if (_icon != null)
			{
				_icon.Frame = new RectangleF(0, 0, IconWidth, Math.Min(toolbarHeight, IconHeight));
			}

			if (_child?.VirtualView != null)
			{
				Rect layoutBounds = new Rect(IconWidth, 0, Bounds.Width - IconWidth, height);

				_child.PlatformArrangeHandler(layoutBounds);
			}
			else if (_icon != null && Superview != null)
			{
				_icon.Center = new PointF(Superview.Frame.Width / 2 - Frame.X, Superview.Frame.Height / 2);
			}
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

				if (_child != null)
				{
					_child.PlatformView?.RemoveFromSuperview();
					_child.DisconnectHandler();
					_child = null;
				}

				if (_view is not null)
				{
					_view.ParentSet -= OnTitleViewParentSet;
				}

				_view = null;

				_icon?.Dispose();
				_icon = null;
			}

			base.Dispose(disposing);
		}
	}
}