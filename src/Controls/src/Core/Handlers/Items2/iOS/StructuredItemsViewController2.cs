#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
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

			RegisterSupplementaryViews(UICollectionElementKindSection.Header);
			RegisterSupplementaryViews(UICollectionElementKindSection.Footer);
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

		public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView, NSString elementKind, NSIndexPath indexPath)
		{
			// We don't have a header or footer, so we don't need to do anything
			if (ItemsView.Header is null && ItemsView.Footer is null && ItemsView.HeaderTemplate is null && ItemsView.FooterTemplate is null)
			{
				return null;
			}

			var reuseId = DetermineViewReuseId(elementKind);

			var view = collectionView.DequeueReusableSupplementaryView(elementKind, reuseId, indexPath) as UICollectionReusableView;

			switch (view)
			{
				case DefaultCell2 defaultCell:
					UpdateDefaultSupplementaryView(defaultCell, elementKind);
					break;
				case TemplatedCell2 templatedCell:
					UpdateTemplatedSupplementaryView(templatedCell, elementKind);
					break;
			}

			return view;
		}

		private protected virtual void RegisterSupplementaryViews(UICollectionElementKindSection kind)
		{
			CollectionView.RegisterClassForSupplementaryView(typeof(HorizontalSupplementaryView2),
				kind, HorizontalSupplementaryView2.ReuseId);
			CollectionView.RegisterClassForSupplementaryView(typeof(HorizontalDefaultSupplementalView2),
				kind, HorizontalDefaultSupplementalView2.ReuseId);
			CollectionView.RegisterClassForSupplementaryView(typeof(VerticalSupplementaryView2),
				kind, VerticalSupplementaryView2.ReuseId);
			CollectionView.RegisterClassForSupplementaryView(typeof(VerticalDefaultSupplementalView2),
				kind, VerticalDefaultSupplementalView2.ReuseId);
		}

		string DetermineViewReuseId(NSString elementKind)
		{
			return DetermineViewReuseId(elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.HeaderTemplate
				: ItemsView.FooterTemplate, elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.Header
				: ItemsView.Footer);
		}

		void UpdateDefaultSupplementaryView(DefaultCell2 cell, NSString elementKind)
		{
			var obj = elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.Header
				: ItemsView.Footer;

			cell.Label.Text = obj?.ToString();
			cell.Tag = elementKind == UICollectionElementKindSectionKey.Header
				? HeaderTag
				: FooterTag;
		}

		void UpdateTemplatedSupplementaryView(TemplatedCell2 cell, NSString elementKind)
		{
			bool isHeader = elementKind == UICollectionElementKindSectionKey.Header;
			cell.isHeaderOrFooterChanged = true;

			if (isHeader)
			{
				if (ItemsView.Header is View headerView)
				{
					cell.Bind(headerView, ItemsView);
				}
				else if (ItemsView.HeaderTemplate is not null)
				{
					cell.Bind(ItemsView.HeaderTemplate, ItemsView.Header, ItemsView);
				}
				cell.Tag = HeaderTag;
			}
			else
			{
				if (ItemsView.Footer is View footerView)
				{
					cell.Bind(footerView, ItemsView);
				}
				else if (ItemsView.FooterTemplate is not null)
				{
					cell.Bind(ItemsView.FooterTemplate, ItemsView.Footer, ItemsView);
				}
				cell.Tag = FooterTag;
			}

			cell.isHeaderOrFooterChanged = false;
		}

		string DetermineViewReuseId(DataTemplate template, object item)
		{
			if (template == null)
			{
				if (item is View)
				{
					// No template, but we can fall back to the view
					return IsHorizontal
						? HorizontalSupplementaryView2.ReuseId
						: VerticalSupplementaryView2.ReuseId;
				}
				// No template, no item, fall back to the default supplemental views
				return IsHorizontal
					? HorizontalDefaultSupplementalView2.ReuseId
					: VerticalDefaultSupplementalView2.ReuseId;
			}

			return IsHorizontal
				? HorizontalSupplementaryView2.ReuseId
				: VerticalSupplementaryView2.ReuseId;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (ItemsSource.ItemCount == 0)
			{
				double headerHeight = 0;
				if (ItemsView?.Header is View headerView)
				{
					headerHeight = headerView.HeightRequest;
				}

				double footerHeight = 0;
				if (ItemsView?.Footer is View footerView)
				{
					footerHeight = footerView.HeightRequest;
				}

				return GetEmptyViewCell(collectionView, indexPath, headerHeight, footerHeight);
			}

			return base.GetCell(collectionView, indexPath);
		}

		public override void ViewWillLayoutSubviews()
		{
			if (CollectionView is Items.MauiCollectionView { NeedsCellLayout: true })
			{
				InvalidateLayoutIfItemsMeasureChanged();
			}

			base.ViewWillLayoutSubviews();
		}

		void InvalidateLayoutIfItemsMeasureChanged()
		{
			// If the header or footer is a view, we need to check if the measure has changed.
			// We could then invalidate the layout for supplementary cell only `collectionView.IndexPathForCell(headerCell)` like we do on standard cells,
			// but that causes other cells to oddly collapse (see Issue25362 UITest), so in this case we have to stick with `InvalidateLayout`.
			var collectionView = CollectionView;

			if (ItemsView.Header is not null || ItemsView.HeaderTemplate is not null)
			{
				var visibleHeaders = collectionView.GetVisibleSupplementaryViews(UICollectionElementKindSectionKey.Header);
				foreach (var header in visibleHeaders)
				{
					if (header is TemplatedCell2 { MeasureInvalidated: true })
					{
						collectionView.CollectionViewLayout.InvalidateLayout();
						return;
					}
				}
			}


			if (ItemsView.Footer is not null || ItemsView.FooterTemplate is not null)
			{
				var visibleFooters = collectionView.GetVisibleSupplementaryViews(UICollectionElementKindSectionKey.Footer);
				foreach (var footer in visibleFooters)
				{
					if (footer is TemplatedCell2 { MeasureInvalidated: true })
					{
						collectionView.CollectionViewLayout.InvalidateLayout();
						return;
					}
				}
			}
		}
	}
}