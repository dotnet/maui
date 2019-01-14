using System;
using System.ComponentModel;
using System.Linq;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewRenderer
	{
		public CarouselViewRenderer()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(CarouselViewRenderer));
		}
	}

	//// TODO hartez 2018/05/30 09:05:38 Think about whether this Controller and/or the new Adapter should be internal or public
	public class CollectionViewRenderer : ViewRenderer<CollectionView, UIView>
	{
		CollectionViewController _collectionViewController;
		ItemsViewLayout _flowLayout;
		IItemsLayout _layout;
		bool _disposed;

		public CollectionViewRenderer()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(CollectionViewRenderer));
		}

		public override UIViewController ViewController => _collectionViewController;

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, 0, 0);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CollectionView> e)
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
				_collectionViewController.UpdateItemsSource();
			}
			else if (changedProperty.IsOneOf(ItemsView.EmptyViewProperty, ItemsView.EmptyViewTemplateProperty))
			{
				_collectionViewController.UpdateEmptyView();
			}
		}

		protected virtual ItemsViewLayout SelectLayout(IItemsLayout layoutSpecification)
		{
			if (layoutSpecification is GridItemsLayout gridItemsLayout)
			{
				return new GridViewLayout(gridItemsLayout);
			}

			if (layoutSpecification is ListItemsLayout listItemsLayout)
			{
				return new ListViewLayout(listItemsLayout);
			}

			// Fall back to vertical list
			return new ListViewLayout(new ListItemsLayout(ItemsLayoutOrientation.Vertical));
		}

		void TearDownOldElement(CollectionView oldElement)
		{
			if (oldElement == null)
			{
				return;
			}

			// Stop listening for ScrollTo requests
			oldElement.ScrollToRequested -= ScrollToRequested;
		}

		void SetUpNewElement(CollectionView newElement)
		{
			if (newElement == null)
			{
				return;
			}

			_layout = newElement.ItemsLayout;
			_flowLayout = SelectLayout(_layout);
			_collectionViewController = new CollectionViewController(newElement, _flowLayout);
			SetNativeControl(_collectionViewController.View);
			_collectionViewController.CollectionView.BackgroundColor = UIColor.Clear;
			_collectionViewController.CollectionView.WeakDelegate = _flowLayout;
			_collectionViewController.UpdateEmptyView();

			// Listen for ScrollTo requests
			newElement.ScrollToRequested += ScrollToRequested;
		}

		NSIndexPath DetermineIndex(ScrollToRequestEventArgs args)
		{
			if (args.Mode == ScrollToMode.Position)
			{
				// TODO hartez 2018/09/17 16:42:54 This will need to be overridden to account for grouping	
				// TODO hartez 2018/09/17 16:21:19 Handle LTR	
				return NSIndexPath.Create(0, args.Index);
			}

			return _collectionViewController.GetIndexForItem(args.Item);
		}

		void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			var indexPath = DetermineIndex(args);

			if (indexPath.Row < 0 || indexPath.Section < 0)
			{
				// Nothing found, nowhere to scroll to
				return;
			}

			_collectionViewController.CollectionView.ScrollToItem(indexPath, 
				args.ScrollToPosition.ToCollectionViewScrollPosition(_flowLayout.ScrollDirection),
				args.IsAnimated);
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
				if (Element != null)
				{
					TearDownOldElement(Element);
				}
			}

			base.Dispose(disposing);
		}
	}

	public static class ScrollToPositionExtensions
	{
		public static UICollectionViewScrollPosition ToCollectionViewScrollPosition(this ScrollToPosition scrollToPosition, 
			UICollectionViewScrollDirection scrollDirection = UICollectionViewScrollDirection.Vertical, bool isLtr = false)
		{
			CollectionView.VerifyCollectionViewFlagEnabled();

			if (scrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				return scrollToPosition.ToHorizontalCollectionViewScrollPosition(isLtr);
			}

			return scrollToPosition.ToVerticalCollectionViewScrollPosition();
		}

		public static UICollectionViewScrollPosition ToHorizontalCollectionViewScrollPosition(this ScrollToPosition scrollToPosition, bool isLtr)
		{
			CollectionView.VerifyCollectionViewFlagEnabled();

			switch (scrollToPosition)
			{
				case ScrollToPosition.MakeVisible:
				case ScrollToPosition.Start:
					return isLtr ? UICollectionViewScrollPosition.Right : UICollectionViewScrollPosition.Left;
				case ScrollToPosition.End:
					return isLtr ? UICollectionViewScrollPosition.Left : UICollectionViewScrollPosition.Right;
				case ScrollToPosition.Center:
				default:
					return UICollectionViewScrollPosition.CenteredHorizontally;
			}
		}

		public static UICollectionViewScrollPosition ToVerticalCollectionViewScrollPosition(this ScrollToPosition scrollToPosition)
		{
			CollectionView.VerifyCollectionViewFlagEnabled();

			switch (scrollToPosition)
			{
				case ScrollToPosition.MakeVisible:
				case ScrollToPosition.Start:
					return UICollectionViewScrollPosition.Top;
				case ScrollToPosition.End:
					return UICollectionViewScrollPosition.Bottom;
				case ScrollToPosition.Center:
				default:
					return UICollectionViewScrollPosition.CenteredVertically;
			}
		}
	}
}