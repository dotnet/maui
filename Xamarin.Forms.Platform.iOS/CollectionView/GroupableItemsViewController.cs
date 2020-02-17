using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class GroupableItemsViewController<TItemsView> : SelectableItemsViewController<TItemsView>
		where TItemsView : GroupableItemsView
	{
		// Keep a cached value for the current state of grouping around so we can avoid hitting the 
		// BindableProperty all the time 
		bool _isGrouped;

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
			// Currently we explicitly measure all of the headers/footers 
			// Long-term, we might want to look at performance hints (similar to ItemSizingStrategy) for 
			// headers/footers (if the dev knows for sure they'll all the be the same size)
			return GetReferenceSizeForheaderOrFooter(collectionView, ItemsView.GroupHeaderTemplate, UICollectionElementKindSectionKey.Header, section);
		}

		internal CGSize GetReferenceSizeForFooter(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			return GetReferenceSizeForheaderOrFooter(collectionView, ItemsView.GroupFooterTemplate, UICollectionElementKindSectionKey.Footer, section);
		}

		internal CGSize GetReferenceSizeForheaderOrFooter(UICollectionView collectionView, DataTemplate template, NSString elementKind, nint section)
		{
			if (!_isGrouped || template == null)
			{
				return CGSize.Empty;
			}

			if (ItemsSource.GroupCount < 1)
			{
				return CGSize.Empty;
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

			// If we're grouping, we'll need to inset the sections to maintain the item spacing between the 
			// groups and/or their group headers/footers

			var itemsLayout = ItemsView.ItemsLayout;
			var scrollDirection = itemsViewLayout.ScrollDirection;
			nfloat lineSpacing = itemsViewLayout.GetMinimumLineSpacingForSection(collectionView, itemsViewLayout, section);

			if (itemsLayout is GridItemsLayout)
			{
				nfloat itemSpacing = itemsViewLayout.GetMinimumInteritemSpacingForSection(collectionView, itemsViewLayout, section);

				if (scrollDirection == UICollectionViewScrollDirection.Horizontal)
				{
					return new UIEdgeInsets(itemSpacing + uIEdgeInsets.Top, lineSpacing + uIEdgeInsets.Left, 
						uIEdgeInsets.Bottom, uIEdgeInsets.Right);
				}

				return new UIEdgeInsets(lineSpacing + uIEdgeInsets.Top, itemSpacing + uIEdgeInsets.Left, 
					uIEdgeInsets.Bottom, uIEdgeInsets.Right);
			}

			if (scrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				return new UIEdgeInsets(uIEdgeInsets.Top, lineSpacing + uIEdgeInsets.Left, 
					uIEdgeInsets.Bottom, uIEdgeInsets.Right);
			}

			return new UIEdgeInsets(lineSpacing + uIEdgeInsets.Top, uIEdgeInsets.Left, 
				uIEdgeInsets.Bottom, uIEdgeInsets.Right);
		}
	}
}