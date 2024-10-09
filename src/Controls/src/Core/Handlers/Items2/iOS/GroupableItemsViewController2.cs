#nullable disable
using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class GroupableItemsViewController2<TItemsView> : SelectableItemsViewController2<TItemsView>
		where TItemsView : GroupableItemsView
	{
		// Keep a cached value for the current state of grouping around so we can avoid hitting the 
		// BindableProperty all the time 
		bool _isGrouped;

		// Keep out header measurement cells for iOS handy so we don't have to
		// create new ones all the time. For other versions, the reusable cells
		// queueing mechanism does this for us.
		// TemplatedCell2 _measurementCellTemplated;
		// DefaultCell2 _measurementCellDefault;

		Action _scrollAnimationEndedCallback;

		public GroupableItemsViewController2(TItemsView groupableItemsView, UICollectionViewLayout layout)
			: base(groupableItemsView, layout)
		{
			_isGrouped = ItemsView.IsGrouped;
		}

		protected override UICollectionViewDelegateFlowLayout CreateDelegator()
		{
			return new GroupableItemsViewDelegator2<TItemsView, GroupableItemsViewController2<TItemsView>>(ItemsViewLayout, this);
		}

		protected override Items.IItemsViewSource CreateItemsViewSource()
		{
			// Use the BindableProperty here (instead of _isGroupingEnabled) because the cached value might not be set yet
			if (ItemsView.IsGrouped)
			{
				return Items.ItemsSourceFactory.CreateGrouped(ItemsView.ItemsSource, this);
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

		private protected override void RegisterSupplementaryViews(UICollectionElementKindSection kind)
		{
			base.RegisterSupplementaryViews(kind);
			if (IsHorizontal)
			{
				CollectionView.RegisterClassForSupplementaryView(typeof(HorizontalSupplementaryView2),
					kind, HorizontalSupplementalView2ReuseId);
				CollectionView.RegisterClassForSupplementaryView(typeof(HorizontalDefaultSupplementalView2),
					kind, HorizontalDefaultSupplementalView2ReuseId);
			}
			else
			{
				CollectionView.RegisterClassForSupplementaryView(typeof(VerticalSupplementaryView2),
					kind, VerticalSupplementaryView2ReuseId);
				CollectionView.RegisterClassForSupplementaryView(typeof(VerticalDefaultSupplementalView2),
					kind, VerticalDefaultSupplementalView2ReuseId);
			}
		}

		string DetermineViewReuseId(NSString elementKind)
		{
			return DetermineViewReuseId(elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.GroupHeaderTemplate
				: ItemsView.GroupFooterTemplate);
		}

		public override UICollectionReusableView GetViewForSupplementaryElement(UICollectionView collectionView,
			NSString elementKind, NSIndexPath indexPath)
		{
			// If the IndexPath is less than 2, it's a header or footer for a section not a group
			if (indexPath.Length < 2)
			{
				var suplementaryViewFromStructuredView = base.GetViewForSupplementaryElement(collectionView, elementKind, indexPath);
				if (suplementaryViewFromStructuredView is not null)
				{
					return suplementaryViewFromStructuredView;
				}
			}

			var reuseId = DetermineViewReuseId(elementKind);

			var view = collectionView.DequeueReusableSupplementaryView(elementKind, reuseId, indexPath) as UICollectionReusableView;

			switch (view)
			{
				case DefaultCell2 DefaultCell2:
					UpdateDefaultSupplementaryView(DefaultCell2, elementKind, indexPath);
					break;
				case TemplatedCell2 TemplatedCell2:
					UpdateTemplatedSupplementaryView(TemplatedCell2, elementKind, indexPath);
					break;
			}

			return view;
		}

		void UpdateDefaultSupplementaryView(DefaultCell2 cell, NSString elementKind, NSIndexPath indexPath)
		{
			cell.Label.Text = ItemsSource?.Group(indexPath)?.ToString();

			// if (cell is ItemsViewCell2)
			// {
			// 	cell.ConstrainTo(GetLayoutSpanCount() * ItemsViewLayout.ConstrainedDimension);
			// }
		}

		void UpdateTemplatedSupplementaryView(TemplatedCell2 cell, NSString elementKind, NSIndexPath indexPath)
		{
			DataTemplate template = elementKind == UICollectionElementKindSectionKey.Header
				? ItemsView.GroupHeaderTemplate
				: ItemsView.GroupFooterTemplate;

			var bindingContext = ItemsSource.Group(indexPath);

			cell.Bind(template, bindingContext, ItemsView);

			// if (cell is ItemsViewCell2)
			// {
			// 	cell.ConstrainTo(GetLayoutSpanCount() * ItemsViewLayout.ConstrainedDimension);
			// }
		}



		string DetermineViewReuseId(DataTemplate template)
		{
			var orientation = ItemsLayoutOrientation.Vertical;

			if (this.ItemsView.ItemsLayout is LinearItemsLayout linearItemsLayout)
				orientation = linearItemsLayout.Orientation;
			else if (this.ItemsView.ItemsLayout is GridItemsLayout gridItemsLayout)
				orientation = gridItemsLayout.Orientation;

			if (template == null)
			{
				// No template, fall back the the default supplemental views
				return orientation == ItemsLayoutOrientation.Horizontal
					? HorizontalDefaultSupplementalView2ReuseId
					: VerticalDefaultSupplementalView2ReuseId;
			}

			return orientation == ItemsLayoutOrientation.Horizontal
				? HorizontalSupplementalView2ReuseId
				: VerticalSupplementaryView2ReuseId;
		}

		static NSString HorizontalDefaultSupplementalView2ReuseId = new NSString("Microsoft.Maui.Controls.HorizontalDefaultSupplementalGroupView2");
		static NSString VerticalDefaultSupplementalView2ReuseId = new NSString("Microsoft.Maui.Controls.VerticalDefaultSupplementaryGroupView2");
		static NSString HorizontalSupplementalView2ReuseId = new NSString("Microsoft.Maui.Controls.HorizontalSupplementalGroupView2");
		static NSString VerticalSupplementaryView2ReuseId = new NSString("Microsoft.Maui.Controls.VerticalSupplementaryGroupView2");


		// internal CGSize GetReferenceSizeForHeader(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		// {
		// 	if (!_isGrouped)
		// 	{
		// 		return CGSize.Empty;
		// 	}
		//
		// 	// Currently we explicitly measure all of the headers/footers 
		// 	// Long-term, we might want to look at performance hints (similar to ItemSizingStrategy) for 
		// 	// headers/footers (if the dev knows for sure they'll all the be the same size)
		// 	return GetReferenceSizeForheaderOrFooter(collectionView, ItemsView.GroupHeaderTemplate, UICollectionElementKindSectionKey.Header, section);
		// }

		// internal CGSize GetReferenceSizeForFooter(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		// {
		// 	if (!_isGrouped)
		// 	{
		// 		return CGSize.Empty;
		// 	}
		//
		// 	return GetReferenceSizeForheaderOrFooter(collectionView, ItemsView.GroupFooterTemplate, UICollectionElementKindSectionKey.Footer, section);
		// }

		// internal CGSize GetReferenceSizeForheaderOrFooter(UICollectionView collectionView, DataTemplate template, NSString elementKind, nint section)
		// {
		// 	if (!_isGrouped || template == null)
		// 	{
		// 		return CGSize.Empty;
		// 	}
		//
		// 	if (ItemsSource.GroupCount < 1 || section > ItemsSource.GroupCount - 1)
		// 	{
		// 		return CGSize.Empty;
		// 	}
		//
		// 	// if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
		// 	// {
		// 	// 	// iOS 10 crashes if we try to dequeue a cell for measurement
		// 	// 	// so we'll use an alternate method
		// 	// 	return MeasureSupplementaryView(elementKind, section);
		// 	// }
		//
		// 	var cell = GetViewForSupplementaryElement(collectionView, elementKind,
		// 		NSIndexPath.FromItemSection(0, section)) as ItemsViewCell2;
		//
		// 	return cell.Measure();
		// }

		internal void SetScrollAnimationEndedCallback(Action callback)
		{
			_scrollAnimationEndedCallback = callback;
		}

		internal void HandleScrollAnimationEnded()
		{
			_scrollAnimationEndedCallback?.Invoke();
			_scrollAnimationEndedCallback = null;
		}


		// internal UIEdgeInsets GetInsetForSection(UICollectionViewLayout itemsViewLayout,
		// 	UICollectionView collectionView, nint section)
		// {
		// 	var uIEdgeInsets = itemsViewLayout.GetInsetForSection(collectionView, itemsViewLayout, section);
		//
		// 	if (!ItemsView.IsGrouped)
		// 	{
		// 		return uIEdgeInsets;
		// 	}
		//
		// 	// If we're grouping, we'll need to inset the sections to maintain the spacing between the 
		// 	// groups and their group headers/footers
		// 	
		// 	nfloat spacing = itemsViewLayout.GetMinimumLineSpacingForSection(collectionView, itemsViewLayout, section);
		//
		// 	var top = uIEdgeInsets.Top;
		// 	var left = uIEdgeInsets.Left;
		//
		// 	if (itemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal)
		// 	{
		// 		left += spacing;
		// 	}
		// 	else
		// 	{
		// 		top += spacing;
		// 	}
		//
		// 	return new UIEdgeInsets(top, left,
		// 		uIEdgeInsets.Bottom, uIEdgeInsets.Right);
		// }

		// These measurement methods are only necessary for iOS 10 and lower
		// CGSize MeasureTemplatedSupplementaryCell(NSString elementKind, nint section, NSString reuseId)
		// {
		// 	if (_measurementCellTemplated == null)
		// 	{
		// 		if (reuseId == HorizontalSupplementaryView2.ReuseId)
		// 		{
		// 			_measurementCellTemplated = new HorizontalSupplementaryView2(CGRect.Empty);
		// 		}
		// 		else if (reuseId == VerticalSupplementaryView2.ReuseId)
		// 		{
		// 			_measurementCellTemplated = new VerticalSupplementaryView2(CGRect.Empty);
		// 		}
		// 	}
		//
		// 	if (_measurementCellTemplated == null)
		// 	{
		// 		return CGSize.Empty;
		// 	}
		//
		// 	UpdateTemplatedSupplementaryView(_measurementCellTemplated, elementKind, NSIndexPath.FromItemSection(0, section));
		// 	return _measurementCellTemplated.Measure();
		// }

		// CGSize MeasureDefaultSupplementaryCell(NSString elementKind, nint section, NSString reuseId)
		// {
		// 	if (_measurementCellDefault == null)
		// 	{
		// 		if (reuseId == HorizontalDefaultSupplementalView2.ReuseId)
		// 		{
		// 			_measurementCellDefault = new HorizontalDefaultSupplementalView2(CGRect.Empty);
		// 		}
		// 		else if (reuseId == VerticalDefaultSupplementalView2.ReuseId)
		// 		{
		// 			_measurementCellDefault = new VerticalDefaultSupplementalView2(CGRect.Empty);
		// 		}
		// 	}
		//
		// 	if (_measurementCellDefault == null)
		// 	{
		// 		return CGSize.Empty;
		// 	}
		//
		// 	UpdateDefaultSupplementaryView(_measurementCellDefault, elementKind, NSIndexPath.FromItemSection(0, section));
		// 	return _measurementCellDefault.Measure();
		// }
		//
		// CGSize MeasureSupplementaryView(NSString elementKind, nint section)
		// {
		// 	var reuseId = (NSString)DetermineViewReuseId(elementKind);
		//
		// 	if (reuseId == HorizontalDefaultSupplementalView2.ReuseId
		// 		|| reuseId == VerticalDefaultSupplementalView2.ReuseId)
		// 	{
		// 		return MeasureDefaultSupplementaryCell(elementKind, section, reuseId);
		// 	}
		//
		// 	return MeasureTemplatedSupplementaryCell(elementKind, section, reuseId);
		// }

		// end of iOS 10 workaround stuff
	}
}