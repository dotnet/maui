#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Primitives;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	class ShellFlyoutLayoutManager
	{
		double _headerOffset = 0;
		UIView _contentView;
		UIScrollView ScrollView { get; set; }
		UIContainerView _headerView;
		UIView _footerView;
		double _headerSize;
		readonly IShellContext _context;
		Action removeScolledEvent;

		// This is the height of the AppBar on Android, which is used
		// as the default minimum height on `Android`.
		// We use the same value here on iOS/Catalyst to stay consistent between the two platforms.
		// Users can set a MinimumHeightRequest if they want this value to be smaller.
		const double MinimumCollapsedHeaderHeight = 56;

		IShellController ShellController => _context.Shell;
		public ShellFlyoutLayoutManager(IShellContext context)
		{
			_context = context;
			_context.Shell.PropertyChanged += OnShellPropertyChanged;
			ShellController.StructureChanged += OnStructureChanged;
		}

		public void SetCustomContent(View content)
		{
			if (content == Content)
				return;

			removeScolledEvent?.Invoke();
			removeScolledEvent = null;

			if (Content is not null)
			{
				var oldRenderer = (IPlatformViewHandler)Content.Handler;
				var oldContentView = ContentView;
				var oldContent = Content;

				Content = null;
				ContentView = null;
				oldContent.Handler = null;
				oldContentView?.RemoveFromSuperview();
				oldRenderer?.DisconnectHandler();
			}
			// If the user hasn't defined custom content then only the ContentView is set
			else if (ContentView is not null)
			{
				var oldContentView = ContentView;
				ContentView = null;
				oldContentView.RemoveFromSuperview();
			}

			Content = content;
			if (Content is not null)
			{
				var renderer = Content.ToHandler(_context.Shell.FindMauiContext());
				ContentView = renderer.PlatformView;
				ContentView.ClipsToBounds = true;

				// not sure if there's a more efficient way to do this
				// I can test the native control to see if it inherits from UIScrollView
				// But the CollectionViewRenderer doesn't inherit from UIScrollView
				if (Content is ScrollView sv)
				{
					sv.Scrolled += ScrollViewScrolled;
					removeScolledEvent = () => sv.Scrolled -= ScrollViewScrolled;
					void ScrollViewScrolled(object sender, ScrolledEventArgs e) =>
						OnScrolled((nfloat)sv.ScrollY);
				}
				else if (Content is CollectionView cv)
				{
					cv.Scrolled += CollectionViewScrolled;
					removeScolledEvent = () => cv.Scrolled -= CollectionViewScrolled;
					void CollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e) =>
						OnScrolled((nfloat)e.VerticalOffset);
				}
				else if (Content is ListView lv)
				{
					lv.Scrolled += ListViewScrolled;
					removeScolledEvent = () => lv.Scrolled -= ListViewScrolled;
					void ListViewScrolled(object sender, ScrolledEventArgs e) =>
						OnScrolled((nfloat)e.ScrollY);
				}
			}
		}

		public void SetDefaultContent(UIView view)
		{
			if (ContentView == view)
				return;

			SetCustomContent(null);
			ContentView = view;
		}

		public View Content
		{
			get;
			private set;
		}

		public UIView ContentView
		{
			get
			{
				return _contentView;
			}
			private set
			{
				_contentView = value;

				ScrollView = null;

				if (ContentView is UIScrollView sv1)
					ScrollView = sv1;
				else if (ContentView is IPlatformViewHandler ver && ver.PlatformView is UIScrollView uIScroll)
					ScrollView = uIScroll;

				if (ScrollView is not null && (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#if TVOS
					|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
				))
				{
					ScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
				}

				UpdateHeaderSize();
			}
		}

		public virtual UIContainerView HeaderView
		{
			get => _headerView;
			set
			{
				if (_headerView == value)
					return;

				if (_headerView is not null)
					_headerView.HeaderSizeChanged -= OnHeaderViewMeasureChanged;

				_headerView = value;

				if (_headerView is not null)
					_headerView.HeaderSizeChanged += OnHeaderViewMeasureChanged;

				UpdateHeaderSize();
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
				UpdateHeaderSize();
			}
		}

		void OnHeaderViewMeasureChanged(object sender, EventArgs e)
		{
			if (HeaderView is null || ContentView?.Superview is null)
				return;

			HeaderView.SizeThatFits(new CGSize(ContentView.Superview.Frame.Width, double.PositiveInfinity));
			UpdateHeaderSize();
		}

		internal void UpdateHeaderSize()
		{
			if (HeaderView is null || ContentView?.Superview is null)
				return;

			// If the HeaderView hasn't been measured we need to measure it
			if (double.IsNaN(MeasuredHeaderViewHeightWithNoMargin))
			{
				HeaderView.SizeThatFits(new CGSize(ContentView.Superview.Frame.Width, double.PositiveInfinity));
			}

			SetHeaderContentInset();
			UpdateHeaderMaximumSize(ScrollView?.ContentOffset.Y);
			LayoutParallax();
		}


		internal void SetHeaderContentInset()
		{
			if (ScrollView is null)
				return;

			var offset = ScrollView.ContentInset.Top;

			if (HeaderView is not null)
			{
				if (double.IsNaN(MeasuredHeaderViewHeightWithNoMargin))
				{
					return;
				}

				ScrollView.ContentInset = new UIEdgeInsets((nfloat)(MeasuredHeaderViewHeightWithNoMargin), 0, 0, 0);
			}
			else
			{
				ScrollView.ContentInset = new UIEdgeInsets(UIApplication.SharedApplication.GetSafeAreaInsetsForWindow().Top, 0, 0, 0);
			}

			offset -= ScrollView.ContentInset.Top;

			var yContentOffset = ScrollView.ContentOffset.Y;
			ScrollView.ContentOffset =
				new CGPoint(ScrollView.ContentOffset.X, yContentOffset + offset);

			UpdateVerticalScrollMode();
		}

		public void UpdateVerticalScrollMode()
		{
			if (ScrollView is null)
				return;

			switch (_context.Shell.FlyoutVerticalScrollMode)
			{
				case ScrollMode.Auto:
					ScrollView.ScrollEnabled = true;
					ScrollView.AlwaysBounceVertical = false;
					break;
				case ScrollMode.Enabled:
					ScrollView.ScrollEnabled = true;
					ScrollView.AlwaysBounceVertical = true;
					break;
				case ScrollMode.Disabled:
					ScrollView.ScrollEnabled = false;
					ScrollView.AlwaysBounceVertical = false;
					break;
			}
		}

		public void LayoutParallax()
		{
			var parent = ContentView?.Superview;
			if (parent is null)
				return;

			nfloat footerHeight = 0;

			if (FooterView is not null)
				footerHeight = FooterView.Frame.Height;

			var contentViewYOffset = HeaderView?.Frame.Height ?? 0;

			if (ScrollView is not null)
			{
				if (Content is null)
				{
					ContentView.Frame =
							new CGRect(parent.Bounds.X, HeaderTopMargin, parent.Bounds.Width, parent.Bounds.Height - HeaderTopMargin - footerHeight);
				}
				else
				{
					var contentFrame = new Rect(parent.Bounds.X, HeaderTopMargin, parent.Bounds.Width, parent.Bounds.Height - HeaderTopMargin - footerHeight);
					(Content as IView)?.Arrange(contentFrame);
				}
			}
			else
			{
				float topMargin = 0;
				if (!Content.IsSet(View.MarginProperty))
				{
					if (HeaderView is null)
						topMargin = (float)UIApplication.SharedApplication.GetSafeAreaInsetsForWindow().Top;
				}
				else
					contentViewYOffset -= (nfloat)HeaderTopMargin;

				var contentFrame = new Rect(parent.Bounds.X, topMargin + contentViewYOffset, parent.Bounds.Width, parent.Bounds.Height - topMargin - footerHeight - contentViewYOffset);
				(Content as IView)?.Arrange(contentFrame);
			}

			if (HeaderView is not null && !double.IsNaN(ArrangedHeaderViewHeightWithNoMargin))
			{
				HeaderView.Frame = new CGRect(0, _headerOffset + HeaderTopMargin, parent.Frame.Width, ArrangedHeaderViewHeightWithNoMargin + HeaderTopMargin);

				if (_context.Shell.FlyoutHeaderBehavior == FlyoutHeaderBehavior.Scroll && HeaderTopMargin > 0 && _headerOffset < 0)
				{
					var headerHeight = Math.Max(HeaderMinimumHeight, ArrangedHeaderViewHeightWithNoMargin + _headerOffset + HeaderTopMargin);
					CAShapeLayer shapeLayer = new CAShapeLayer();
					CGRect rect = new CGRect(0, _headerOffset * -1, parent.Frame.Width, headerHeight);
					var path = CGPath.FromRect(rect);
					shapeLayer.Path = path;
					HeaderView.Layer.Mask = shapeLayer;

				}
				else if (HeaderView.Layer.Mask is not null)
					HeaderView.Layer.Mask = null;
			}
		}

		void OnStructureChanged(object sender, EventArgs e) => UpdateVerticalScrollMode();

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

		public void ViewDidLoad()
		{
			UpdateHeaderSize();
		}

		public void OnScrolled(nfloat contentOffsetY)
		{
			var headerBehavior = _context.Shell.FlyoutHeaderBehavior;

			switch (headerBehavior)
			{
				case FlyoutHeaderBehavior.Default:
				case FlyoutHeaderBehavior.Fixed:
					ArrangedHeaderViewHeightWithNoMargin = MeasuredHeaderViewHeightWithNoMargin;
					_headerOffset = 0;
					break;

				case FlyoutHeaderBehavior.Scroll:
					ArrangedHeaderViewHeightWithNoMargin = MeasuredHeaderViewHeightWithNoMargin;
					_headerOffset = Math.Min(0, -(MeasuredHeaderViewHeightWithNoMargin + contentOffsetY));
					break;

				case FlyoutHeaderBehavior.CollapseOnScroll:
					UpdateHeaderMaximumSize(contentOffsetY);
					_headerOffset = 0;
					break;
			}

			LayoutParallax();
		}

		void UpdateHeaderMaximumSize(nfloat? contentOffsetY)
		{
			if (HeaderView is not null)
			{
				if (ScrollView is not null && contentOffsetY is not null && _context.Shell.FlyoutHeaderBehavior == FlyoutHeaderBehavior.CollapseOnScroll)
					ArrangedHeaderViewHeightWithNoMargin = Math.Max(HeaderMinimumHeight, -contentOffsetY.Value);
				else
					ArrangedHeaderViewHeightWithNoMargin = MeasuredHeaderViewHeightWithNoMargin;
			}
		}

		double ArrangedHeaderViewHeightWithNoMargin
		{
			get => _headerSize;
			set
			{
				if (HeaderView is not null &&
					HeaderView.Height != (value))
				{
					HeaderView.Height = value;
				}

				_headerSize = value;
			}
		}

		double MeasuredHeaderViewHeightWithNoMargin =>
			HeaderView?.MeasuredHeight ?? 0;

		double HeaderMinimumHeight
		{
			get
			{
				if (HeaderView is null ||
					HeaderView.View.MinimumHeightRequest == -1 ||
					HeaderView.View.MinimumHeightRequest == Dimension.Unset)
				{
					if (_context.Shell.FlyoutHeaderBehavior == FlyoutHeaderBehavior.CollapseOnScroll)
						return MinimumCollapsedHeaderHeight;
					else
						return 0;
				}

				return HeaderView.View.MinimumHeightRequest;
			}
		}

		double HeaderTopMargin => (HeaderView is not null) ?
			HeaderView.Margin.Top - HeaderView.Margin.Bottom : 0;

		public void TearDown()
		{
			_context.Shell.PropertyChanged -= OnShellPropertyChanged;
			ShellController.StructureChanged -= OnStructureChanged;
			SetCustomContent(null);
			ContentView = null;
			HeaderView = null;
			FooterView = null;
		}
	}
}
