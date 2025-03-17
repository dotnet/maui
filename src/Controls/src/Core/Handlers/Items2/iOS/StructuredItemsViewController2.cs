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
			if (IsHorizontal)
			{
				CollectionView.RegisterClassForSupplementaryView(typeof(HorizontalSupplementaryView2),
					kind, HorizontalSupplementaryView2.ReuseId);
				CollectionView.RegisterClassForSupplementaryView(typeof(HorizontalDefaultSupplementalView2),
					kind, HorizontalDefaultSupplementalView2.ReuseId);
			}
			else
			{
				CollectionView.RegisterClassForSupplementaryView(typeof(VerticalSupplementaryView2),
					kind, VerticalSupplementaryView2.ReuseId);
				CollectionView.RegisterClassForSupplementaryView(typeof(VerticalDefaultSupplementalView2),
					kind, VerticalDefaultSupplementalView2.ReuseId);
			}
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
		}

		void UpdateTemplatedSupplementaryView(TemplatedCell2 cell, NSString elementKind)
		{
			bool isHeader = elementKind == UICollectionElementKindSectionKey.Header;

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
	}
}