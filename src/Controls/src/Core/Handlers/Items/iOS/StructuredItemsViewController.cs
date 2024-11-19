#nullable disable
using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class StructuredItemsViewController<TItemsView> : ItemsViewController<TItemsView>
		where TItemsView : StructuredItemsView
	{
		public const int HeaderTag = 111;
		public const int FooterTag = 222;

		bool _disposed;

		UIView _headerUIView;
		VisualElement _headerViewFormsElement;

		UIView _footerUIView;
		VisualElement _footerViewFormsElement;

		public StructuredItemsViewController(TItemsView structuredItemsView, ItemsViewLayout layout)
			: base(structuredItemsView, layout)
		{
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
				if (_headerViewFormsElement != null)
				{
					_headerViewFormsElement.MeasureInvalidated -= OnFormsElementMeasureInvalidated;
				}

				if (_footerViewFormsElement != null)
				{
					_footerViewFormsElement.MeasureInvalidated -= OnFormsElementMeasureInvalidated;
				}

				if (_headerUIView is MauiView hv)
				{
					hv.LayoutChanged -= HeaderViewLayoutChanged;
				}

				if (_footerUIView is MauiView fv)
				{
					fv.LayoutChanged -= FooterViewLayoutChanged;
				}

				_headerUIView = null;
				_headerViewFormsElement = null;
				_footerUIView = null;
				_footerViewFormsElement = null;
			}

			base.Dispose(disposing);
		}

		protected override bool IsHorizontal => (ItemsView?.ItemsLayout as ItemsLayout)?.Orientation == ItemsLayoutOrientation.Horizontal;

		protected override CGRect DetermineEmptyViewFrame()
		{
			nfloat headerHeight = 0;
			var headerView = CollectionView.ViewWithTag(HeaderTag);

			if (headerView != null)
				headerHeight = headerView.Frame.Height;

			nfloat footerHeight = 0;
			var footerView = CollectionView.ViewWithTag(FooterTag);

			if (footerView != null)
				footerHeight = footerView.Frame.Height;

			return new CGRect(CollectionView.Frame.X, CollectionView.Frame.Y, CollectionView.Frame.Width,
				Math.Abs(CollectionView.Frame.Height - (headerHeight + footerHeight)));
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			// This update is only relevant if you have a footer view because it's used to place the footer view
			// based on the ContentSize so we just update the positions if the ContentSize has changed
			if (_footerUIView != null)
			{
				var emptyView = CollectionView.ViewWithTag(EmptyTag);

				if (IsHorizontal)
				{
					if (_footerUIView.Frame.X != ItemsViewLayout.CollectionViewContentSize.Width ||
						_footerUIView.Frame.X < emptyView?.Frame.X)
						UpdateHeaderFooterPosition();
				}
				else
				{
					if (_footerUIView.Frame.Y != ItemsViewLayout.CollectionViewContentSize.Height ||
						_footerUIView.Frame.Y < (emptyView?.Frame.Y + emptyView?.Frame.Height))
						UpdateHeaderFooterPosition();
				}
			}
		}

		internal void UpdateFooterView()
		{
			UpdateSubview(ItemsView?.Footer, ItemsView?.FooterTemplate, FooterTag,
				ref _footerUIView, ref _footerViewFormsElement);
			UpdateHeaderFooterPosition();

			if (_footerUIView is MauiView mv)
			{
				mv.LayoutChanged += FooterViewLayoutChanged;
			}
		}

		internal void UpdateHeaderView()
		{
			UpdateSubview(ItemsView?.Header, ItemsView?.HeaderTemplate, HeaderTag,
				ref _headerUIView, ref _headerViewFormsElement);
			UpdateHeaderFooterPosition();

			if (_headerUIView is MauiView mv)
			{
				mv.LayoutChanged += HeaderViewLayoutChanged;
			}
		}


		internal void UpdateSubview(object view, DataTemplate viewTemplate, nint viewTag, ref UIView uiView, ref VisualElement formsElement)
		{
			uiView?.RemoveFromSuperview();

			if (formsElement != null)
			{
				ItemsView.RemoveLogicalChild(formsElement);
				formsElement.MeasureInvalidated -= OnFormsElementMeasureInvalidated;
			}

			UpdateView(view, viewTemplate, ref uiView, ref formsElement);

			if (uiView != null)
			{
				uiView.Tag = viewTag;
				CollectionView.AddSubview(uiView);
			}

			if (formsElement != null)
			{
				ItemsView.AddLogicalChild(formsElement);
			}

			if (formsElement != null)
			{
				RemeasureLayout(formsElement);
				formsElement.MeasureInvalidated += OnFormsElementMeasureInvalidated;
			}
			else
			{
				uiView?.SizeToFit();
			}
		}

		void UpdateHeaderFooterPosition()
		{
			var emptyView = CollectionView.ViewWithTag(EmptyTag);

			if (IsHorizontal)
			{
				var currentInset = CollectionView.ContentInset;

				nfloat headerWidth = _headerUIView?.Frame.Width ?? 0f;
				nfloat footerWidth = _footerUIView?.Frame.Width ?? 0f;
				nfloat emptyWidth = emptyView?.Frame.Width ?? 0f;

				if (_headerUIView != null && _headerUIView.Frame.X != headerWidth)
				{
					_headerUIView.Frame = new CoreGraphics.CGRect(-headerWidth, 0, headerWidth, CollectionView.Frame.Height);
				}

				if (_footerUIView != null && (_footerUIView.Frame.X != ItemsViewLayout.CollectionViewContentSize.Width || emptyWidth > 0))
					_footerUIView.Frame = new CoreGraphics.CGRect(ItemsViewLayout.CollectionViewContentSize.Width + emptyWidth, 0, footerWidth, CollectionView.Frame.Height);

				if (CollectionView.ContentInset.Left != headerWidth || CollectionView.ContentInset.Right != footerWidth)
				{
					var currentOffset = CollectionView.ContentOffset;
					CollectionView.ContentInset = new UIEdgeInsets(0, headerWidth, 0, footerWidth);

					var xOffset = currentOffset.X + (currentInset.Left - CollectionView.ContentInset.Left);

					if (CollectionView.ContentSize.Width + headerWidth <= CollectionView.Bounds.Width)
						xOffset = -headerWidth;

					// if the header grows it will scroll off the screen because if you change the content inset iOS adjusts the content offset so the list doesn't move
					// this changes the offset of the list by however much the header size has changed
					CollectionView.ContentOffset = new CoreGraphics.CGPoint(xOffset, CollectionView.ContentOffset.Y);
				}
			}
			else
			{
				var currentInset = CollectionView.ContentInset;
				nfloat headerHeight = _headerUIView?.Frame.Height ?? 0f;
				nfloat footerHeight = _footerUIView?.Frame.Height ?? 0f;
				nfloat emptyHeight = emptyView?.Frame.Height ?? 0f;

				if (CollectionView.ContentInset.Top != headerHeight || CollectionView.ContentInset.Bottom != footerHeight)
				{
					var currentOffset = CollectionView.ContentOffset;
					CollectionView.ContentInset = new UIEdgeInsets(headerHeight, 0, footerHeight, 0);

					// if the header grows it will scroll off the screen because if you change the content inset iOS adjusts the content offset so the list doesn't move
					// this changes the offset of the list by however much the header size has changed

					var yOffset = currentOffset.Y + (currentInset.Top - CollectionView.ContentInset.Top);

					if (CollectionView.ContentSize.Height + headerHeight <= CollectionView.Bounds.Height)
						yOffset = -headerHeight;

					CollectionView.ContentOffset = new CoreGraphics.CGPoint(CollectionView.ContentOffset.X, yOffset);
				}

				if (_headerUIView != null && _headerUIView.Frame.Y != headerHeight)
				{
					_headerUIView.Frame = new CoreGraphics.CGRect(0, -headerHeight, CollectionView.Frame.Width, headerHeight);
				}

				nfloat height = 0;

				if (IsViewLoaded && View.Window != null)
				{
					height = ItemsViewLayout.CollectionViewContentSize.Height;
				}

				if (_footerUIView != null && (_footerUIView.Frame.Y != height || emptyHeight > 0))
				{
					_footerUIView.Frame = new CoreGraphics.CGRect(0, height + emptyHeight, CollectionView.Frame.Width, footerHeight);
				}
			}
		}

		protected override void HandleFormsElementMeasureInvalidated(VisualElement formsElement)
		{
			base.HandleFormsElementMeasureInvalidated(formsElement);
			UpdateHeaderFooterPosition();
		}

		internal void UpdateLayoutMeasurements()
		{
			if (_headerViewFormsElement != null)
				HandleFormsElementMeasureInvalidated(_headerViewFormsElement);

			if (_footerViewFormsElement != null)
				HandleFormsElementMeasureInvalidated(_footerViewFormsElement);
		}

		void HeaderViewLayoutChanged(object sender, EventArgs e)
		{
			HandleFormsElementMeasureInvalidated(_headerViewFormsElement);
		}

		void FooterViewLayoutChanged(object sender, EventArgs e)
		{
			HandleFormsElementMeasureInvalidated(_footerViewFormsElement);
		}
	}
}