using CoreAnimation;
using CoreGraphics;
using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTableViewController : UITableViewController
	{
		readonly IShellContext _context;
		readonly UIContainerView _headerView;
		readonly ShellTableViewSource _source;
		double _headerMin = 56;
		double _headerOffset = 0;
		double _headerSize;

		public ShellTableViewController(IShellContext context, UIContainerView headerView, Action<Element> onElementSelected)
		{
			_context = context;
			_headerView = headerView;
			_source = new ShellTableViewSource(context, onElementSelected);
			_source.ScrolledEvent += OnScrolled;
			if (_headerView != null)
				_headerView.HeaderSizeChanged += OnHeaderSizeChanged;
			((IShellController)_context.Shell).StructureChanged += OnStructureChanged;
		}

		void OnHeaderSizeChanged(object sender, EventArgs e)
		{
			_headerSize = HeaderMax;
			TableView.ContentInset = new UIEdgeInsets((nfloat)HeaderMax, 0, 0, 0);
			LayoutParallax();
		}

		void OnStructureChanged(object sender, EventArgs e)
		{
			_source.ClearCache();
			TableView.ReloadData();
		}

		public void LayoutParallax()
		{
			if (TableView?.Superview == null)
				return;

			var parent = TableView.Superview;
			TableView.Frame = parent.Bounds.Inset(0, SafeAreaOffset);
			if (_headerView != null)
			{
				_headerView.Frame = new CGRect(0, _headerOffset + SafeAreaOffset, parent.Frame.Width, _headerSize);

				var headerHeight = Math.Max(_headerMin, _headerSize + _headerOffset);
				if (_headerOffset < 0)
				{
					CAShapeLayer shapeLayer = new CAShapeLayer();
					CGRect rect = new CGRect(0, _headerOffset * -1, parent.Frame.Width, headerHeight);
					var path = CGPath.FromRect(rect);
					shapeLayer.Path = path;
					_headerView.Layer.Mask = shapeLayer;
				}
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			_headerView?.MeasureIfNeeded();

			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			if (Forms.IsiOS11OrNewer)
				TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
			TableView.ContentInset = new UIEdgeInsets((nfloat)HeaderMax, 0, 0, 0);
			TableView.Source = _source;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if ((_context?.Shell as IShellController) != null)
					((IShellController)_context.Shell).StructureChanged -= OnStructureChanged;

				if (_source != null)
					_source.ScrolledEvent -= OnScrolled;

				if (_headerView != null)
					_headerView.HeaderSizeChanged -= OnHeaderSizeChanged;
			}

			base.Dispose(disposing);
		}


		void OnScrolled(object sender, UIScrollView e)
		{
			var headerBehavior = _context.Shell.FlyoutHeaderBehavior;

			switch (headerBehavior)
			{
				case FlyoutHeaderBehavior.Default:
				case FlyoutHeaderBehavior.Fixed:
					_headerSize = HeaderMax;
					break;

				case FlyoutHeaderBehavior.Scroll:
					_headerSize = HeaderMax;
					_headerOffset = Math.Min(0, -(HeaderMax + e.ContentOffset.Y));
					break;

				case FlyoutHeaderBehavior.CollapseOnScroll:
					_headerSize = Math.Max(_headerMin, Math.Min(HeaderMax, HeaderMax - e.ContentOffset.Y - HeaderMax));
					break;
			}

			LayoutParallax();
		}

		float SafeAreaOffset => (float)Platform.SafeAreaInsetsForWindow.Top;
		double HeaderMax => _headerView?.MeasuredHeight ?? 0;
	}
}