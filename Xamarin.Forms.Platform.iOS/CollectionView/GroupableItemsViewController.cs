using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class GroupableItemsViewController : SelectableItemsViewController
	{
		GroupableItemsView GroupableItemsView => (GroupableItemsView)ItemsView;

		// Keep a cached value for the current state of grouping around so we can avoid hitting the 
		// BindableProperty all the time 
		bool _isGrouped;

		Action _scrollAnimationEndedCallback;

		public GroupableItemsViewController(GroupableItemsView groupableItemsView, ItemsViewLayout layout) 
			: base(groupableItemsView, layout)
		{
			_isGrouped = GroupableItemsView.IsGrouped;
		}

		protected override IItemsViewSource CreateItemsViewSource()
		{
			// Use the BindableProperty here (instead of _isGroupingEnabled) because the cached value might not be set yet
			if (GroupableItemsView.IsGrouped) 
			{
				return ItemsSourceFactory.CreateGrouped(GroupableItemsView.ItemsSource, CollectionView);
			}

			return base.CreateItemsViewSource();
		}

		public override void UpdateItemsSource()
		{
			_isGrouped = GroupableItemsView.IsGrouped;
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
				cell.ConstrainTo(ItemsViewLayout.ConstrainedDimension);
			}
		}

		void UpdateTemplatedSupplementaryView(TemplatedCell cell, NSString elementKind, NSIndexPath indexPath)
		{
			DataTemplate template = elementKind == UICollectionElementKindSectionKey.Header
				? GroupableItemsView.GroupHeaderTemplate
				: GroupableItemsView.GroupFooterTemplate;

			var bindingContext = ItemsSource.Group(indexPath);

			cell.Bind(template, bindingContext, ItemsView);

			if (cell is ItemsViewCell)
			{
				cell.ConstrainTo(ItemsViewLayout.ConstrainedDimension);
			}
		}

		string DetermineViewReuseId(NSString elementKind)
		{
			if (elementKind == UICollectionElementKindSectionKey.Header)
			{
				return DetermineViewReuseId(GroupableItemsView.GroupHeaderTemplate);
			}

			return DetermineViewReuseId(GroupableItemsView.GroupFooterTemplate);
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

			var cell = GetViewForSupplementaryElement(collectionView, UICollectionElementKindSectionKey.Header, 
				NSIndexPath.FromItemSection(0, section)) as ItemsViewCell;

			return cell.Measure();
		}

		internal CGSize GetReferenceSizeForFooter(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			if (!_isGrouped)
			{
				return CGSize.Empty;
			}

			var cell = GetViewForSupplementaryElement(collectionView, UICollectionElementKindSectionKey.Footer, 
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
	}
}