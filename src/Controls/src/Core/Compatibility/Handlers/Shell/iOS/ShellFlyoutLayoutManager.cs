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
		Action removeScrolledEvent;

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

			removeScrolledEvent?.Invoke();
			removeScrolledEvent = null;

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
					removeScrolledEvent = () => sv.Scrolled -= ScrollViewScrolled;
					void ScrollViewScrolled(object sender, ScrolledEventArgs e) =>
						OnScrolled((nfloat)sv.ScrollY);
				}
				else if (Content is CollectionView cv)
				{
					cv.Scrolled += CollectionViewScrolled;
					removeScrolledEvent = () => cv.Scrolled -= CollectionViewScrolled;
					void CollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e) =>
						// OnScrolled expects a negative offset based on the header height since it is based on ScrollView.ScrollY
						// So we fix it up here by subtracting the header height
						OnScrolled((nfloat)(e.VerticalOffset - MeasuredHeaderViewHeightWithMargin));
				}
				else if (Content is ListView lv)
				{
					lv.Scrolled += ListViewScrolled;
					removeScrolledEvent = () => lv.Scrolled -= ListViewScrolled;
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

				if (ContentView is UIScrollView sv)
					ScrollView = sv;
				else if (ContentView is IPlatformViewHandler ver && ver.PlatformView is UIScrollView uIScroll)
					ScrollView = uIScroll;
				else if (Content is ItemsView && ContentView.Subviews.Length > 0 && ContentView.Subviews[0] is UICollectionView cv)
					ScrollView = cv;

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
				if (double.IsNaN(MeasuredHeaderViewHeightWithNoMargin))
				{
					return;
				}

				// We take the measured header height without margin, since the margin is already accounted for in the positioning of the scroll view itself.
				ScrollView.ContentInset = new UIEdgeInsets((nfloat)Math.Max(HeaderMinimumHeight, MeasuredHeaderViewHeightWithNoMargin), 0, 0, 0);
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

			LayoutHeader(parent.Frame);
			LayoutContent(parent.Bounds, footerHeight);
		}

		void LayoutHeader(CGRect parentFrame)
		{
			if (HeaderView is not null && !double.IsNaN(ArrangedHeaderViewHeightWithMargin))
			{
				nfloat safeArea = 0;
				if (ShouldHonorSafeArea(HeaderView.View))
				{
					// We add the safe area if margin is not explicitly set.
					safeArea = UIApplication.SharedApplication.GetSafeAreaInsetsForWindow().Top;
				}

				// For header's Y offset, we should only consider the safe area but not its margin, since it will be handled by MAUI's layout system.
				HeaderView.Frame = new CGRect(0, _headerOffset + safeArea, parentFrame.Width, ArrangedHeaderViewHeightWithMargin);

				if (_context.Shell.FlyoutHeaderBehavior == FlyoutHeaderBehavior.Scroll && HeaderViewTopVerticalOffset > 0 && _headerOffset < 0)
				{
					var headerHeight = Math.Max(HeaderMinimumHeight, ArrangedHeaderViewHeightWithMargin + _headerOffset);
					CAShapeLayer shapeLayer = new StaticCAShapeLayer();
					CGRect rect = new CGRect(0, _headerOffset * -1, parentFrame.Width, headerHeight);
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
			double contentYOffset = 0;

			if (ShouldHonorSafeArea(HeaderView?.View) ||
				(HeaderView is null && ShouldHonorSafeArea(Content)))
			{
				// We add the safe area if margin is not explicitly set. This matches the header behavior.
				contentYOffset += (float)UIApplication.SharedApplication.GetSafeAreaInsetsForWindow().Top;
			}

			if (HeaderView is not null)
			{
				if (ScrollView is null)
				{
					// The margin is already managed by MAUI's layout system, so we don't need to add it here and we just offset the content by the header's height.				
					contentYOffset += HeaderView.Frame.Height;
				}
				else
				{
					// For ScrollView, we need to consider the margin, but we should not consider the header height, since it should overlap with the scroll view. 
					// The content inset is already managed by SetHeaderContentInset.
					contentYOffset += HeaderView.View.Margin.VerticalThickness;
				}
			}

			var contentFrame = new Rect(parentBounds.X, contentYOffset, parentBounds.Width, parentBounds.Height - contentYOffset - footerHeight);
			if (Content is null)
			{
				ContentView.Frame = contentFrame.AsCGRect();
			}
			else
			{
				(Content as IView)?.Arrange(contentFrame);
			}
		}

		bool ShouldHonorSafeArea(View view)
		{
			return view != null
				&& !view.IsSet(View.MarginProperty)
				&& !(view is ISafeAreaView sav && sav.IgnoreSafeArea);
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
				{
					// Neither HeaderMinimumHeight nor contentOffsetY (calculated in SetHeaderContentInset) contain the header's margin, so we need to add it here.
					ArrangedHeaderViewHeightWithMargin = Math.Max(HeaderMinimumHeight, -contentOffsetY.Value) + HeaderView.View.Margin.VerticalThickness;
				}
				else
				{
					ArrangedHeaderViewHeightWithMargin = MeasuredHeaderViewHeightWithMargin;
				}
			}
		}

		double ArrangedHeaderViewHeightWithMargin
		{
			get => _headerSize;
			set
			{
				if (HeaderView is not null &&
					HeaderView.Height != value)
				{
					HeaderView.Height = value;
				}

				_headerSize = value;
			}
		}

		double MeasuredHeaderViewHeightWithMargin =>
			HeaderView?.MeasuredHeight ?? 0;

		double MeasuredHeaderViewHeightWithNoMargin
		{
			get
			{
				if (!double.IsNaN(MeasuredHeaderViewHeightWithMargin) && HeaderView?.View.IsSet(View.MarginProperty) == true)
				{
					return MeasuredHeaderViewHeightWithMargin - HeaderView.View.Margin.VerticalThickness;
				}

				return MeasuredHeaderViewHeightWithMargin;
			}
		}

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
		/// This represents the header's top vertical offset caused by either Margin.Top or SafeArea.Top.
		/// It should not be assumed as margin, because if Margin.Top = 0, it will return SafeAre.Top.
		/// </summary>
		double HeaderViewTopVerticalOffset => HeaderView?.Margin.Top ?? 0;

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
