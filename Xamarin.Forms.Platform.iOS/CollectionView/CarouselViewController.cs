using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewController : ItemsViewController<CarouselView>
	{
		CarouselView _carouselView;
		bool _viewInitialized;

		public CarouselViewController(CarouselView itemsView, ItemsViewLayout layout) : base(itemsView, layout)
		{
			_carouselView = itemsView;
			CollectionView.AllowsSelection = false;
			CollectionView.AllowsMultipleSelection = false;
		}

		protected override UICollectionViewDelegateFlowLayout CreateDelegator()
		{
			return new CarouselViewDelegator(ItemsViewLayout, this);
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = base.GetCell(collectionView, indexPath);

			var element = (cell as CarouselTemplatedCell)?.VisualElementRenderer?.Element;
			if (element != null)
				VisualStateManager.GoToState(element, CarouselView.DefaultItemVisualState);
			return cell;
		}

		// Here because ViewDidAppear (and associates) are not fired consistently for this class
		// See a more extensive explanation in the ItemsViewController.ViewWillLayoutSubviews method
		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			if (!_viewInitialized)
			{
				UpdateInitialPosition();

				_viewInitialized = true;
			}
		}

		protected override bool IsHorizontal => (_carouselView?.ItemsLayout as ItemsLayout)?.Orientation == ItemsLayoutOrientation.Horizontal;

		protected override string DetermineCellReuseId()
		{
			if (_carouselView.ItemTemplate != null)
			{
				return CarouselTemplatedCell.ReuseId;
			}
			return base.DetermineCellReuseId();
		}

		protected override void RegisterViewTypes()
		{
			CollectionView.RegisterClassForCell(typeof(CarouselTemplatedCell), CarouselTemplatedCell.ReuseId);
			base.RegisterViewTypes();
		}

		internal void TearDown()
		{
		}

		public override void DraggingStarted(UIScrollView scrollView)
		{
			_carouselView.SetIsDragging(true);
		}

		public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
		{
			_carouselView.SetIsDragging(false);
		}

		internal void UpdateIsScrolling(bool isScrolling)
		{
			_carouselView.IsScrolling = isScrolling;
		}

		void UpdateInitialPosition()
		{
			if (_carouselView.Position != 0)
				_carouselView.ScrollTo(_carouselView.Position, -1, ScrollToPosition.Center, false);
		}
	}
}