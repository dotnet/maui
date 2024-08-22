#nullable disable
using System;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class StructuredItemsViewController2<TItemsView> : ItemsViewController2<TItemsView>
		where TItemsView : StructuredItemsView
	{
		public const int HeaderTag = 111;
		public const int FooterTag = 222;

		bool _disposed;

		public StructuredItemsViewController2(TItemsView structuredItemsView, UICollectionViewLayout layout)
			: base(structuredItemsView, layout)
		{
		}
		
		protected override void RegisterViewTypes()
		{
			base.RegisterViewTypes();
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

			}

			base.Dispose(disposing);
		}

		protected override bool IsHorizontal => (ItemsView?.ItemsLayout as ItemsLayout)?.Orientation == ItemsLayoutOrientation.Horizontal;

		
		public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView,
			NSString elementKind, NSIndexPath indexPath)
		{
			var reuseId = DetermineViewReuseId(elementKind);

			var view = collectionView.DequeueReusableSupplementaryView(elementKind, reuseId, indexPath) as UICollectionReusableView;

			switch (view)
			{
				case DefaultCell2 defaultCell:
					UpdateDefaultSupplementaryView(defaultCell, elementKind, indexPath);
					break;
				case TemplatedCell2 templatedCell:
					UpdateTemplatedSupplementaryView(templatedCell, elementKind, indexPath);
					break;
			}

			return view;
		}

		void UpdateDefaultSupplementaryView(DefaultCell2 cell, NSString elementKind, NSIndexPath indexPath)
		{
			var obj = elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.Header
				: ItemsView.Footer;

			cell.Label.Text = obj?.ToString();
		}

		void UpdateTemplatedSupplementaryView(TemplatedCell2 cell, NSString elementKind, NSIndexPath indexPath)
		{
			DataTemplate template = elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.HeaderTemplate
				: ItemsView.FooterTemplate;

			var bindingContext = ItemsView.Header;

			cell.Bind(template, bindingContext, ItemsView);
			cell.Tag = elementKind == UICollectionElementKindSectionKey.Header ? HeaderTag : FooterTag;
		}

		string DetermineViewReuseId(NSString elementKind)
		{
			return DetermineViewReuseId(elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.HeaderTemplate
				: ItemsView.FooterTemplate);
		}

		string DetermineViewReuseId(DataTemplate template)
		{		
			if (template == null)
			{
				// No template, fall back the the default supplemental views
				return  IsHorizontal
					? HorizontalDefaultSupplementalView2.ReuseId
					: VerticalDefaultSupplementalView2.ReuseId;
			}

			return IsHorizontal
				? HorizontalSupplementaryView2.ReuseId
				: VerticalSupplementaryView2.ReuseId;
		}

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
			
		}

		internal void UpdateFooterView()
		{
			//UpdateLayout();
		}

		internal void UpdateHeaderView()
		{
			// UpdateSubview(ItemsView?.Header, ItemsView?.HeaderTemplate, HeaderTag,
			// 	ref _headerUIView, ref _headerViewFormsElement);
			// UpdateHeaderFooterPosition();
		}
	}
}