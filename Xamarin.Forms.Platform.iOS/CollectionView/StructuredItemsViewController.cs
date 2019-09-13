using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class StructuredItemsViewController : ItemsViewController
	{
		bool _disposed;

		StructuredItemsView StructuredItemsView => (StructuredItemsView)ItemsView;

		UIView _headerUIView;
		VisualElement _headerViewFormsElement;

		UIView _footerUIView;
		VisualElement _footerViewFormsElement;

		public StructuredItemsViewController(StructuredItemsView structuredItemsView, ItemsViewLayout layout)
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
					_headerViewFormsElement.MeasureInvalidated -= OnFormsElementMeasureInvalidated;

				if (_footerViewFormsElement != null)
					_footerViewFormsElement.MeasureInvalidated -= OnFormsElementMeasureInvalidated;

				_headerUIView = null;
				_headerViewFormsElement = null;
				_footerUIView = null;
				_footerViewFormsElement = null;
				
			}

			base.Dispose(disposing);
		}

		protected override bool IsHorizontal => (StructuredItemsView?.ItemsLayout as ItemsLayout)?.Orientation == ItemsLayoutOrientation.Horizontal;

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			// This update is only relevant if you have a footer view because it's used to place the footer view
			// based on the ContentSize so we just update the positions if the ContentSize has changed
			if (_footerUIView != null)
			{
				if (IsHorizontal)
				{
					if (_footerUIView.Frame.X != ItemsViewLayout.CollectionViewContentSize.Width)
						UpdateHeaderFooterPosition();
				}
				else
				{
					if (_footerUIView.Frame.Y != ItemsViewLayout.CollectionViewContentSize.Height)
						UpdateHeaderFooterPosition();
				}
			}
		}

		internal void UpdateFooterView()
		{
			UpdateSubview(StructuredItemsView?.Footer, StructuredItemsView?.FooterTemplate, 
				ref _footerUIView, ref _footerViewFormsElement);
		}

		internal void UpdateHeaderView()
		{
			UpdateSubview(StructuredItemsView?.Header, StructuredItemsView?.HeaderTemplate, 
				ref _headerUIView, ref _headerViewFormsElement);
		}

		void UpdateHeaderFooterPosition()
		{
			if (IsHorizontal)
			{
				var currentInset = CollectionView.ContentInset;

				nfloat headerWidth = _headerUIView?.Frame.Width ?? 0f;
				nfloat footerWidth = _footerUIView?.Frame.Width ?? 0f;

				if (_headerUIView != null && _headerUIView.Frame.X != headerWidth)
					_headerUIView.Frame = new CoreGraphics.CGRect(-headerWidth, 0, headerWidth, CollectionView.Frame.Height);

				if (_footerUIView != null && (_footerUIView.Frame.X != ItemsViewLayout.CollectionViewContentSize.Width))
					_footerUIView.Frame = new CoreGraphics.CGRect(ItemsViewLayout.CollectionViewContentSize.Width, 0, footerWidth, CollectionView.Frame.Height);

				if (CollectionView.ContentInset.Left != headerWidth || CollectionView.ContentInset.Right != footerWidth)
				{
					CollectionView.ContentInset = new UIEdgeInsets(0, headerWidth, 0, footerWidth);

					// if the header grows it will scroll off the screen because if you change the content inset iOS adjusts the content offset so the list doesn't move
					// this changes the offset of the list by however much the header size has changed
					CollectionView.ContentOffset = new CoreGraphics.CGPoint(CollectionView.ContentOffset.X + (currentInset.Left - CollectionView.ContentInset.Left), CollectionView.ContentOffset.Y);
				}
			}
			else
			{
				var currentInset = CollectionView.ContentInset;

				nfloat headerHeight = _headerUIView?.Frame.Height ?? 0f;
				nfloat footerHeight = _footerUIView?.Frame.Height ?? 0f;

				if (_headerUIView != null && _headerUIView.Frame.Y != headerHeight)
					_headerUIView.Frame = new CoreGraphics.CGRect(0, -headerHeight, CollectionView.Frame.Width, headerHeight);

				if (_footerUIView != null && (_footerUIView.Frame.Y != ItemsViewLayout.CollectionViewContentSize.Height))
					_footerUIView.Frame = new CoreGraphics.CGRect(0, ItemsViewLayout.CollectionViewContentSize.Height, CollectionView.Frame.Width, footerHeight);

				if (CollectionView.ContentInset.Top != headerHeight || CollectionView.ContentInset.Bottom != footerHeight)
				{
					CollectionView.ContentInset = new UIEdgeInsets(headerHeight, 0, footerHeight, 0);

					// if the header grows it will scroll off the screen because if you change the content inset iOS adjusts the content offset so the list doesn't move
					// this changes the offset of the list by however much the header size has changed
					CollectionView.ContentOffset = new CoreGraphics.CGPoint(CollectionView.ContentOffset.X, CollectionView.ContentOffset.Y + (currentInset.Top - CollectionView.ContentInset.Top));
				}
			}
		}

		protected override void HandleFormsElementMeasureInvalidated(VisualElement formsElement)
		{
			base.HandleFormsElementMeasureInvalidated(formsElement);
			UpdateHeaderFooterPosition();
		}
	}
}