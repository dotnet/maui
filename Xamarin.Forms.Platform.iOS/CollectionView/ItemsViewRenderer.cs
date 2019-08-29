using System.ComponentModel;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ItemsViewRenderer : ViewRenderer<ItemsView, UIView>
	{
		ItemsViewLayout _layout;
		bool _disposed;
		bool? _defaultHorizontalScrollVisibility;
		bool? _defaultVerticalScrollVisibility;

		public ItemsViewRenderer()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(ItemsViewRenderer));
		}

		public override UIViewController ViewController => ItemsViewController;

		protected ItemsViewController ItemsViewController { get; private set; }

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, 0, 0);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ItemsView> e)
		{
			TearDownOldElement(e.OldElement);
			SetUpNewElement(e.NewElement);

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(ItemsView.ItemsSourceProperty))
			{
				ItemsViewController.UpdateItemsSource();
			}
			else if (changedProperty.IsOneOf(ItemsView.EmptyViewProperty, ItemsView.EmptyViewTemplateProperty))
			{
				ItemsViewController.UpdateEmptyView();
			}
			else if (changedProperty.IsOneOf(ItemsView.HeaderProperty, ItemsView.HeaderTemplateProperty))
			{
				ItemsViewController.UpdateHeaderView();
			}
			else if (changedProperty.IsOneOf(ItemsView.FooterProperty, ItemsView.FooterTemplateProperty))
			{
				ItemsViewController.UpdateFooterView();
			}
			else if (changedProperty.Is(ItemsView.ItemSizingStrategyProperty))
			{
				UpdateItemSizingStrategy();
			}
			else if (changedProperty.Is(ItemsView.HorizontalScrollBarVisibilityProperty))
			{
				UpdateHorizontalScrollBarVisibility();
			}
			else if (changedProperty.Is(ItemsView.VerticalScrollBarVisibilityProperty))
			{
				UpdateVerticalScrollBarVisibility();
			}
			else if (changedProperty.Is(ItemsView.ItemsUpdatingScrollModeProperty))
			{
				UpdateItemsUpdatingScrollMode();
			}
		}

		protected virtual ItemsViewLayout SelectLayout(IItemsLayout layoutSpecification, ItemSizingStrategy itemSizingStrategy)
		{
			if (layoutSpecification is GridItemsLayout gridItemsLayout)
			{
				return new GridViewLayout(gridItemsLayout, itemSizingStrategy);
			}

			if (layoutSpecification is ListItemsLayout listItemsLayout)
			{
				return new ListViewLayout(listItemsLayout, itemSizingStrategy);
			}

			// Fall back to vertical list
			return new ListViewLayout(new ListItemsLayout(ItemsLayoutOrientation.Vertical), itemSizingStrategy);
		}

		protected virtual void TearDownOldElement(ItemsView oldElement)
		{
			if (oldElement == null)
			{
				return;
			}

			// Stop listening for ScrollTo requests
			oldElement.ScrollToRequested -= ScrollToRequested;
		}

		protected virtual void SetUpNewElement(ItemsView newElement)
		{
			if (newElement == null)
			{
				return;
			}

			UpdateLayout();
			ItemsViewController = CreateController(newElement, _layout);
			 
			if (Forms.IsiOS11OrNewer)
			{
				// We set this property to keep iOS from trying to be helpful about insetting all the 
				// CollectionView content when we're in landscape mode (to avoid the notch)
				// The SetUseSafeArea Platform Specific is already taking care of this for us 
				// That said, at some point it's possible folks will want a PS for controlling this behavior
				ItemsViewController.CollectionView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
			}

			SetNativeControl(ItemsViewController.View);
			ItemsViewController.CollectionView.BackgroundColor = UIColor.Clear;
			ItemsViewController.UpdateEmptyView();
			ItemsViewController.UpdateFooterView();
			ItemsViewController.UpdateHeaderView();

			UpdateHorizontalScrollBarVisibility();
			UpdateVerticalScrollBarVisibility();

			// Listen for ScrollTo requests
			newElement.ScrollToRequested += ScrollToRequested;
		}

		protected virtual void UpdateLayout()
		{
			_layout = SelectLayout(Element.ItemsLayout, Element.ItemSizingStrategy);

			if (ItemsViewController != null)
			{
				ItemsViewController.UpdateLayout(_layout);
			}
		}

		protected virtual void UpdateItemSizingStrategy()
		{
			UpdateLayout();
		}

		protected virtual void UpdateItemsUpdatingScrollMode()
		{
			_layout.ItemsUpdatingScrollMode = Element.ItemsUpdatingScrollMode;
		}

		protected virtual ItemsViewController CreateController(ItemsView newElement, ItemsViewLayout layout)
		{
			return new ItemsViewController(newElement, layout);
		}

		NSIndexPath DetermineIndex(ScrollToRequestEventArgs args)
		{
			if (args.Mode == ScrollToMode.Position)
			{
				if (args.GroupIndex == -1)
				{
					return NSIndexPath.Create(0, args.Index);
				}

				return NSIndexPath.Create(args.GroupIndex, args.Index);
			}

			return ItemsViewController.GetIndexForItem(args.Item);
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == null)
				_defaultVerticalScrollVisibility = ItemsViewController.CollectionView.ShowsVerticalScrollIndicator;

			switch (Element.VerticalScrollBarVisibility)
			{
				case ScrollBarVisibility.Always:
					ItemsViewController.CollectionView.ShowsVerticalScrollIndicator = true;
					break;
				case ScrollBarVisibility.Never:
					ItemsViewController.CollectionView.ShowsVerticalScrollIndicator = false;
					break;
				case ScrollBarVisibility.Default:
					ItemsViewController.CollectionView.ShowsVerticalScrollIndicator = _defaultVerticalScrollVisibility.Value;
					break;
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == null)
				_defaultHorizontalScrollVisibility = ItemsViewController.CollectionView.ShowsHorizontalScrollIndicator;

			switch (Element.HorizontalScrollBarVisibility)
			{
				case ScrollBarVisibility.Always:
					ItemsViewController.CollectionView.ShowsHorizontalScrollIndicator = true;
					break;
				case ScrollBarVisibility.Never:
					ItemsViewController.CollectionView.ShowsHorizontalScrollIndicator = false;
					break;
				case ScrollBarVisibility.Default:
					ItemsViewController.CollectionView.ShowsHorizontalScrollIndicator = _defaultHorizontalScrollVisibility.Value;
					break;
			}
		}

		protected virtual void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			using (var indexPath = DetermineIndex(args))
			{
				if (!IsIndexPathValid(indexPath))
				{
					// Specified path wasn't valid, or item wasn't found
					return;
				}

				ItemsViewController.CollectionView.ScrollToItem(indexPath,
					args.ScrollToPosition.ToCollectionViewScrollPosition(_layout.ScrollDirection), args.IsAnimated);
			}
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
				TearDownOldElement(Element);

				ItemsViewController?.Dispose();
				ItemsViewController = null;
			}

			base.Dispose(disposing);
		}

		bool IsIndexPathValid(NSIndexPath indexPath)
		{
			if (indexPath.Item < 0 || indexPath.Section < 0)
			{
				return false;
			}

			var collectionView = ItemsViewController.CollectionView;
			if (indexPath.Section >= collectionView.NumberOfSections())
			{
				return false;
			}

			if (indexPath.Item >= collectionView.NumberOfItemsInSection(indexPath.Section))
			{
				return false;
			}

			return true;
		}
	}
}
