using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class GroupableItemsViewController<TItemsView> : SelectableItemsViewController<TItemsView>
		where TItemsView : GroupableItemsView
	{
		// Keep a cached value for the current state of grouping around so we can avoid hitting the 
		// BindableProperty all the time 
		bool _isGrouped;

		// Keep out header measurement cells for iOS handy so we don't have to
		// create new ones all the time. For other versions, the reusable cells
		// queueing mechanism does this for us.
		TemplatedCell _measurementCellTemplated;
		DefaultCell _measurementCellDefault;

		Action _scrollAnimationEndedCallback;

		public GroupableItemsViewController(TItemsView groupableItemsView, ItemsViewLayout layout) 
			: base(groupableItemsView, layout)
		{
			_isGrouped = ItemsView.IsGrouped;
		}

		protected override UICollectionViewDelegateFlowLayout CreateDelegator()
		{
			return new GroupableItemsViewDelegator<TItemsView, GroupableItemsViewController<TItemsView>>(ItemsViewLayout, this);
		}

		protected override IItemsViewSource CreateItemsViewSource()
		{
			// Use the BindableProperty here (instead of _isGroupingEnabled) because the cached value might not be set yet
			if (ItemsView.IsGrouped) 
			{
				return ItemsSourceFactory.CreateGrouped(ItemsView.ItemsSource, this);
			}

			return base.CreateItemsViewSource();
		}

		public override void UpdateItemsSource()
		{
			_isGrouped = ItemsView.IsGrouped;
			base.UpdateItemsSource();
		}

		protected override void RegisterViewTypes()
		{
			base.RegisterViewTypes();

			RegisterSupplementaryViews(UICollectionElementKindSection.Header);
			RegisterSupplementaryViews(UICollectionElementKindSection.Footer);
		}

		void RegisterSupplementaryViews(UICollectionElementKindSection kind)
		{
			CollectionView.RegisterClassForSupplementaryView(typeof(HorizontalSupplementaryView),
				kind, HorizontalSupplementaryView.ReuseId);
			CollectionView.RegisterClassForSupplementaryView(typeof(VerticalSupplementaryView),
				kind, VerticalSupplementaryView.ReuseId);
			CollectionView.RegisterClassForSupplementaryView(typeof(HorizontalDefaultSupplementalView),
				kind, HorizontalDefaultSupplementalView.ReuseId);
			CollectionView.RegisterClassForSupplementaryView(typeof(VerticalDefaultSupplementalView),
				kind, VerticalDefaultSupplementalView.ReuseId);
		}

		public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView,
			NSString elementKind, NSIndexPath indexPath)
		{
			var reuseId = DetermineViewReuseId(elementKind);

			var view = collectionView.DequeueReusableSupplementaryView(elementKind, reuseId, indexPath) as UICollectionReusableView;

			switch (view)
			{
				case DefaultCell defaultCell:
					UpdateDefaultSupplementaryView(defaultCell, elementKind, indexPath);
					break;
				case TemplatedCell templatedCell:
					UpdateTemplatedSupplementaryView(templatedCell, elementKind, indexPath);
					break;
			}

			return view;
		}

		void UpdateDefaultSupplementaryView(DefaultCell cell, NSString elementKind, NSIndexPath indexPath)
		{
			cell.Label.Text = ItemsSource.Group(indexPath).ToString();

			if (cell is ItemsViewCell)
			{
				cell.ConstrainTo(GetLayoutSpanCount() * ItemsViewLayout.ConstrainedDimension);
			}
		}

		void UpdateTemplatedSupplementaryView(TemplatedCell cell, NSString elementKind, NSIndexPath indexPath)
		{
			DataTemplate template = elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.GroupHeaderTemplate
				: ItemsView.GroupFooterTemplate;

			var bindingContext = ItemsSource.Group(indexPath);

			cell.Bind(template, bindingContext, ItemsView);

			if (cell is ItemsViewCell)
			{
				cell.ConstrainTo(GetLayoutSpanCount() * ItemsViewLayout.ConstrainedDimension);
			}
		}

		string DetermineViewReuseId(NSString elementKind)
		{
			return DetermineViewReuseId(elementKind == UICollectionElementKindSectionKey.Header 
				? ItemsView.GroupHeaderTemplate 
				: ItemsView.GroupFooterTemplate);
		}

		string DetermineViewReuseId(DataTemplate template)
		{
			if (template == null)
			{
				// No template, fall back the the default supplemental views
				return ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
					? HorizontalDefaultSupplementalView.ReuseId
					: VerticalDefaultSupplementalView.ReuseId;
			}

			return ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
				? HorizontalSupplementaryView.ReuseId
				: VerticalSupplementaryView.ReuseId;
		}

		internal CGSize GetReferenceSizeForHeader(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			if (!_isGrouped)
			{
				return CGSize.Empty;
			}

			// Currently we explicitly measure all of the headers/footers 
			// Long-term, we might want to look at performance hints (similar to ItemSizingStrategy) for 
			// headers/footers (if the dev knows for sure they'll all the be the same size)
			return GetReferenceSizeForheaderOrFooter(collectionView, ItemsView.GroupHeaderTemplate, UICollectionElementKindSectionKey.Header, section);
		}

		internal CGSize GetReferenceSizeForFooter(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			if (!_isGrouped)
			{
				return CGSize.Empty;
			}

			return GetReferenceSizeForheaderOrFooter(collectionView, ItemsView.GroupFooterTemplate, UICollectionElementKindSectionKey.Footer, section);
		}

		internal CGSize GetReferenceSizeForheaderOrFooter(UICollectionView collectionView, DataTemplate template, NSString elementKind, nint section)
		{
			if (!_isGrouped || template == null)
			{
				return CGSize.Empty;
			}

			if (ItemsSource.GroupCount < 1 || section > ItemsSource.GroupCount - 1)
			{
				return CGSize.Empty;
			}

			if (!Forms.IsiOS11OrNewer)
			{
				// iOS 10 crashes if we try to dequeue a cell for measurement
				// so we'll use an alternate method
				return MeasureSupplementaryView(elementKind, section);
			}

			var cell = GetViewForSupplementaryElement(collectionView, elementKind,
				NSIndexPath.FromItemSection(0, section)) as ItemsViewCell;

			return cell.Measure();
		}

		internal void SetScrollAnimationEndedCallback(Action callback)
		{
			_scrollAnimationEndedCallback = callback;
		}

		internal void HandleScrollAnimationEnded()
		{
			_scrollAnimationEndedCallback?.Invoke();
			_scrollAnimationEndedCallback = null;
		}

		int GetLayoutSpanCount()
		{
			var span = 1;

			if (ItemsView?.ItemsLayout is GridItemsLayout gridItemsLayout)
			{
				span = gridItemsLayout.Span;
			}

			return span;
		}

		internal UIEdgeInsets GetInsetForSection(ItemsViewLayout itemsViewLayout,
			UICollectionView collectionView, nint section)
		{
			var uIEdgeInsets = ItemsViewLayout.GetInsetForSection(collectionView, itemsViewLayout, section);

			if (!ItemsView.IsGrouped)
			{
				return uIEdgeInsets;
			}

			// If we're grouping, we'll need to inset the sections to maintain the spacing between the 
			// groups and their group headers/footers

			nfloat spacing = itemsViewLayout.GetMinimumLineSpacingForSection(collectionView, itemsViewLayout, section);

			var top = uIEdgeInsets.Top;
			var left = uIEdgeInsets.Left;

			if (itemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				left += spacing;
			}
			else
			{
				top += spacing;
			}

			return new UIEdgeInsets(top, left,
				uIEdgeInsets.Bottom, uIEdgeInsets.Right);
		}

		// These measurement methods are only necessary for iOS 10 and lower
		CGSize MeasureTemplatedSupplementaryCell(NSString elementKind, nint section, NSString reuseId)
		{
			if (_measurementCellTemplated == null)
			{
				if (reuseId == HorizontalSupplementaryView.ReuseId)
				{
					_measurementCellTemplated = new HorizontalSupplementaryView(CGRect.Empty);
				}
				else if (reuseId == VerticalSupplementaryView.ReuseId)
				{
					_measurementCellTemplated = new VerticalSupplementaryView(CGRect.Empty);
				}
			}

			if (_measurementCellTemplated == null)
			{
				return CGSize.Empty;
			}

			UpdateTemplatedSupplementaryView(_measurementCellTemplated, elementKind, NSIndexPath.FromItemSection(0, section));
			return _measurementCellTemplated.Measure();
		}

		CGSize MeasureDefaultSupplementaryCell(NSString elementKind, nint section, NSString reuseId)
		{
			if (_measurementCellDefault == null)
			{
				if (reuseId == HorizontalDefaultSupplementalView.ReuseId)
				{
					_measurementCellDefault = new HorizontalDefaultSupplementalView(CGRect.Empty);
				}
				else if (reuseId == VerticalDefaultSupplementalView.ReuseId)
				{
					_measurementCellDefault = new VerticalDefaultSupplementalView(CGRect.Empty);
				}
			}

			if (_measurementCellDefault == null)
			{
				return CGSize.Empty;
			}

			UpdateDefaultSupplementaryView(_measurementCellDefault, elementKind, NSIndexPath.FromItemSection(0, section));
			return _measurementCellDefault.Measure();
		}

		CGSize MeasureSupplementaryView(NSString elementKind, nint section)
		{
			var reuseId = (NSString)DetermineViewReuseId(elementKind);

			if (reuseId == HorizontalDefaultSupplementalView.ReuseId
				|| reuseId == VerticalDefaultSupplementalView.ReuseId)
			{
				return MeasureDefaultSupplementaryCell(elementKind, section, reuseId);
			}

			return MeasureTemplatedSupplementaryCell(elementKind, section, reuseId);
		}

		// end of iOS 10 workaround stuff
	}
}