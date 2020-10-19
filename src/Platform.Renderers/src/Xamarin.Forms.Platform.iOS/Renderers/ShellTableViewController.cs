using CoreAnimation;
using CoreGraphics;
using System;
using UIKit;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTableViewController : UITableViewController
	{
		readonly IShellContext _context;
		readonly ShellTableViewSource _source;
		double _headerMin = 56;
		double _headerOffset = 0;
		double _headerSize;
		bool _isDisposed;
		Action<Element> _onElementSelected;
		UIContainerView _headerView;
		UIView _footerView;

		IShellController ShellController => ((IShellController)_context.Shell);

		public ShellTableViewController(IShellContext context, UIContainerView headerView, Action<Element> onElementSelected) : this(context, onElementSelected)
		{
			HeaderView = headerView;
		}

		public ShellTableViewController(IShellContext context, Action<Element> onElementSelected)
		{
			_context = context;
			_onElementSelected = onElementSelected;
			_source = CreateShellTableViewSource();
			_source.ScrolledEvent += OnScrolled;

			ShellController.StructureChanged += OnStructureChanged;
			_context.Shell.PropertyChanged += OnShellPropertyChanged;
		}

		public virtual UIContainerView HeaderView
		{
			get => _headerView;
			set
			{
				if (_headerView == value)
					return;

				if (_headerView != null)
					_headerView.HeaderSizeChanged -= OnHeaderFooterSizeChanged;

				_headerView = value;

				if (_headerView != null)
					_headerView.HeaderSizeChanged += OnHeaderFooterSizeChanged;
			}
		}

		public virtual UIView FooterView
		{
			get => _footerView;
			set
			{
				if (_footerView == value)
					return;

				_footerView = value;
			}
		}

		protected ShellTableViewSource CreateShellTableViewSource()
		{
			return new ShellTableViewSource(_context, _onElementSelected);
		}

		void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(Shell.FlyoutHeaderBehaviorProperty))
			{
				SetHeaderContentInset();
				LayoutParallax();
			}
			else if (e.Is(Shell.FlyoutVerticalScrollModeProperty))
				UpdateVerticalScrollMode();
		}

		void OnHeaderFooterSizeChanged(object sender, EventArgs e)
		{
			_headerSize = HeaderMax;
			SetHeaderContentInset();
			LayoutParallax();
		}

		void OnStructureChanged(object sender, EventArgs e)
		{
			_source.ClearCache();
			TableView.ReloadData();
			UpdateVerticalScrollMode();
		}

		void UpdateVerticalScrollMode()
		{
			switch (_context.Shell.FlyoutVerticalScrollMode)
			{
				case ScrollMode.Auto:
					TableView.ScrollEnabled = true;
					TableView.AlwaysBounceVertical = false;
					break;
				case ScrollMode.Enabled:
					TableView.ScrollEnabled = true;
					TableView.AlwaysBounceVertical = true;
					break;
				case ScrollMode.Disabled:
					TableView.ScrollEnabled = false;
					TableView.AlwaysBounceVertical = false;
					break;
			}
		}

		public void LayoutParallax()
		{
			if (TableView?.Superview == null)
				return;

			var parent = TableView.Superview;

			nfloat footerHeight = 0;

			if (FooterView != null)
				footerHeight = FooterView.Frame.Height;

			TableView.Frame =
					new CGRect(parent.Bounds.X, HeaderTopMargin, parent.Bounds.Width, parent.Bounds.Height - HeaderTopMargin - footerHeight);

			if (HeaderView != null)
			{
				var margin = HeaderView.Margin;
				var leftMargin = margin.Left - margin.Right;

				HeaderView.Frame = new CGRect(leftMargin, _headerOffset + HeaderTopMargin, parent.Frame.Width, _headerSize);

				if (_context.Shell.FlyoutHeaderBehavior == FlyoutHeaderBehavior.Scroll && HeaderTopMargin > 0 && _headerOffset < 0)
				{
					var headerHeight = Math.Max(_headerMin, _headerSize + _headerOffset);
					CAShapeLayer shapeLayer = new CAShapeLayer();
					CGRect rect = new CGRect(0, _headerOffset * -1, parent.Frame.Width, headerHeight);
					var path = CGPath.FromRect(rect);
					shapeLayer.Path = path;
					HeaderView.Layer.Mask = shapeLayer;
				}
				else if (HeaderView.Layer.Mask != null)
					HeaderView.Layer.Mask = null;
			}
		}

		void SetHeaderContentInset()
		{
			if (HeaderView != null)
				TableView.ContentInset = new UIEdgeInsets((nfloat)HeaderMax, 0, 0, 0);
			else
				TableView.ContentInset = new UIEdgeInsets(Platform.SafeAreaInsetsForWindow.Top, 0, 0, 0);
			UpdateVerticalScrollMode();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			HeaderView?.MeasureIfNeeded();

			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			if (Forms.IsiOS11OrNewer)
				TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;

			SetHeaderContentInset();
			TableView.Source = _source;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if ((_context?.Shell as IShellController) != null)
					((IShellController)_context.Shell).StructureChanged -= OnStructureChanged;

				if (_source != null)
					_source.ScrolledEvent -= OnScrolled;

				if (HeaderView != null)
					HeaderView.HeaderSizeChanged -= OnHeaderFooterSizeChanged;

				_context.Shell.PropertyChanged -= OnShellPropertyChanged;

				_onElementSelected = null;
			}


			_isDisposed = true;
			base.Dispose(disposing);
		}


		void OnScrolled(object sender, UIScrollView e)
		{
			if (HeaderView == null)
				return;

			var headerBehavior = _context.Shell.FlyoutHeaderBehavior;

			switch (headerBehavior)
			{
				case FlyoutHeaderBehavior.Default:
				case FlyoutHeaderBehavior.Fixed:
					_headerSize = HeaderMax;
					_headerOffset = 0;
					break;

				case FlyoutHeaderBehavior.Scroll:
					_headerSize = HeaderMax;
					_headerOffset = Math.Min(0, -(HeaderMax + e.ContentOffset.Y));
					break;

				case FlyoutHeaderBehavior.CollapseOnScroll:
					_headerSize = Math.Max(_headerMin, -e.ContentOffset.Y);
					_headerOffset = 0;
					break;
			}

			LayoutParallax();
		}

		double HeaderMax => HeaderView?.MeasuredHeight ?? 0;
		double HeaderTopMargin => (HeaderView != null) ? HeaderView.Margin.Top - HeaderView.Margin.Bottom : 0;
	}
}
