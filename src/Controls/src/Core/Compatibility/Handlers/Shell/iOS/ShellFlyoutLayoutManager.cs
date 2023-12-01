#nullable disable
using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Primitives;
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
			if (double.IsNaN(MeasuredHeaderViewHeightWithMargin))
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
				if (double.IsNaN(MeasuredHeaderViewHeightWithMargin))
				{
					return;
				}

				ScrollView.ContentInset = new UIEdgeInsets((nfloat)(MeasuredHeaderViewHeightWithMargin), 0, 0, 0);
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
			{
				return;
			}

			nfloat footerHeight = 0;
			if (FooterView is not null)
			{
				footerHeight = FooterView.Frame.Height;
			}

			LayoutContent(parent.Bounds, footerHeight);

			if (HeaderView is not null && !double.IsNaN(ArrangedHeaderViewHeightWithMargin))
			{
				HeaderView.Frame = new CGRect(0, _headerOffset + HeaderViewVerticalOffset, parent.Frame.Width, ArrangedHeaderViewHeightWithMargin);

				if (_context.Shell.FlyoutHeaderBehavior == FlyoutHeaderBehavior.Scroll && HeaderViewVerticalOffset > 0 && _headerOffset < 0)
				{
					var headerHeight = Math.Max(HeaderMinimumHeight, ArrangedHeaderViewHeightWithMargin + _headerOffset);
					CAShapeLayer shapeLayer = new CAShapeLayer();
					CGRect rect = new CGRect(0, _headerOffset * -1, parent.Frame.Width, headerHeight);
					var path = CGPath.FromRect(rect);
					shapeLayer.Path = path;
					HeaderView.Layer.Mask = shapeLayer;

				}
				else if (HeaderView.Layer.Mask is not null)
				{
					HeaderView.Layer.Mask = null;
				}
			}
		}

		void LayoutContent(CGRect parentBounds, nfloat footerHeight)
		{
			// Initially we offset the content by the header's offset (which could include the safe area) + the calculated top margin for the content
			var contentViewYOffset = HeaderViewVerticalOffset + ContentTopMargin;
			if (Content?.IsSet(View.MarginProperty) == false)
			{
				if (HeaderView is null)
				{
					// If HeaderView is null, we need add the SafeArea.Top explicitly. 
					// We get this value through HeaderViewVerticalOffset when there is a HeaderView.
					contentViewYOffset += (float)UIApplication.SharedApplication.GetSafeAreaInsetsForWindow().Top;
				}
			}

			if (ScrollView is null)
			{
				contentViewYOffset += HeaderView?.Frame.Height ?? 0;
			}

			var contentFrame = new Rect(parentBounds.X, contentViewYOffset, parentBounds.Width, parentBounds.Height - contentViewYOffset - footerHeight);
			if (Content is null)
			{
				ContentView.Frame = contentFrame.AsCGRect();
			}
			else
			{
				(Content as IView)?.Arrange(contentFrame);
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
					ArrangedHeaderViewHeightWithMargin = MeasuredHeaderViewHeightWithMargin;
					_headerOffset = 0;
					break;

				case FlyoutHeaderBehavior.Scroll:
					ArrangedHeaderViewHeightWithMargin = MeasuredHeaderViewHeightWithMargin;
					_headerOffset = Math.Min(0, -(MeasuredHeaderViewHeightWithMargin + contentOffsetY));
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
					ArrangedHeaderViewHeightWithMargin = Math.Max(HeaderMinimumHeight, -contentOffsetY.Value);
				else
					ArrangedHeaderViewHeightWithMargin = MeasuredHeaderViewHeightWithMargin;
			}
		}

		double ArrangedHeaderViewHeightWithMargin
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

		double MeasuredHeaderViewHeightWithMargin =>
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

		/// <summary>
		/// This represents the header's vertical offset caused by either Margin.Top or SafeArea.Top.
		/// It should not be assumed as margin, because if Margin.Top = 0, it will return SafeAre.Top.
		/// </summary>
		double HeaderViewVerticalOffset => HeaderView?.Margin.Top ?? 0;

		double ContentTopMargin => HeaderView?.Margin.Bottom ?? 0 + Content?.Margin.Top ?? 0;

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
