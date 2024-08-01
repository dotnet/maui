#nullable disable
using System;
using Foundation;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public partial class CarouselViewHandler2 : ItemsViewHandler2<CarouselView>
	{
		protected override CarouselViewController2 CreateController(CarouselView newElement, UICollectionViewLayout layout)
				=> new(newElement, layout);

		protected override UICollectionViewLayout SelectLayout()
		{
			bool IsHorizontal = VirtualView.ItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal;
			UICollectionViewScrollDirection scrollDirection = IsHorizontal ? UICollectionViewScrollDirection.Horizontal : UICollectionViewScrollDirection.Vertical;

			NSCollectionLayoutDimension itemWidth = NSCollectionLayoutDimension.CreateFractionalWidth(1);
			NSCollectionLayoutDimension itemHeight = NSCollectionLayoutDimension.CreateFractionalHeight(1);
			NSCollectionLayoutDimension groupWidth = NSCollectionLayoutDimension.CreateFractionalWidth(1);
			NSCollectionLayoutDimension groupHeight = NSCollectionLayoutDimension.CreateFractionalHeight(1);
			nfloat itemSpacing = 0;

			var layout = new UICollectionViewCompositionalLayout((sectionIndex, environment) =>
			{
				double sectionMargin = 0.0;
				if (!IsHorizontal)
				{
					sectionMargin = VirtualView.PeekAreaInsets.VerticalThickness / 2;
					var newGroupHeight = environment.Container.ContentSize.Height - VirtualView.PeekAreaInsets.VerticalThickness;
					groupHeight = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupHeight);
					groupWidth = NSCollectionLayoutDimension.CreateFractionalWidth(1);
				}
				else
				{
					sectionMargin = VirtualView.PeekAreaInsets.HorizontalThickness / 2;
					var newGroupWidth = environment.Container.ContentSize.Width - VirtualView.PeekAreaInsets.HorizontalThickness;
					groupWidth = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupWidth);
					groupHeight = NSCollectionLayoutDimension.CreateFractionalHeight(1);
				}

				// Each item has a size
				var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
				// Create the item itself from the size
				var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

				//item.ContentInsets = new NSDirectionalEdgeInsets(0, itemInset, 0, 0);

				var groupSize = NSCollectionLayoutSize.Create(groupWidth, groupHeight);

				var group = IsHorizontal ? NSCollectionLayoutGroup.GetHorizontalGroup(groupSize, item, 1) :
										 NSCollectionLayoutGroup.GetVerticalGroup(groupSize, item, 1);

				int currentPosition = 0;

				// Create our section layout
				var section = NSCollectionLayoutSection.Create(group: group);
				section.InterGroupSpacing = itemSpacing;
				section.OrthogonalScrollingBehavior = UICollectionLayoutSectionOrthogonalScrollingBehavior.GroupPagingCentered;
				section.VisibleItemsInvalidationHandler = (items, offset, env) =>
				{
					var page = (offset.X + sectionMargin) / env.Container.ContentSize.Width;

					// Check if we are at the beginning or end of the page
					if (Math.Abs(page % 1) <= (double.Epsilon * 100) && Controller.ItemsSource.ItemCount > 0)
					{
						var pageIndex = (int)page;

						if (ItemsView.Loop)
						{
							var maxIndex = (Controller.ItemsSource as Items.ILoopItemsViewSource).LoopCount - 1;

							//To mimic looping, we needed to modify the ItemSource and inserted a new item at the beginning and at the end
							if (pageIndex == maxIndex)
							{
								//When at last item, we need to change to 2nd item, so we can scroll right or left
								pageIndex = 1;
							}
							else if (pageIndex == 0)
							{
								//When at first item, need to change to one before last, so we can scroll right or left
								pageIndex = maxIndex - 1;
							}

							//since we added one item at the beginning, we need to subtract one
							var realPage = pageIndex - 1;

							if (currentPosition != realPage)
							{
								currentPosition = realPage;
								var pageNumberIndex = NSIndexPath.FromRowSection(pageIndex, 0);
								Controller.CollectionView.ScrollToItem(pageNumberIndex, UICollectionViewScrollPosition.Left, false);

								//Update the CarouselView position
								(Controller as CarouselViewController2)?.SetPosition(realPage);
							}
						}
						else
						{
							(Controller as CarouselViewController2)?.SetPosition((int)page);
						}
					}
				};

				return section;
			});

			return layout;
		}

		protected override void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			// if (VirtualView?.Loop == true)
			// {
			// 	var goToIndexPath = (Controller as CarouselViewController2).GetScrollToIndexPath(args.Index);

			// 	if (!IsIndexPathValid(goToIndexPath))
			// 	{
			// 		return;
			// 	}

			// 	Controller.CollectionView.ScrollToItem(goToIndexPath,
			// 		args.ScrollToPosition.ToCollectionViewScrollPosition( UICollectionViewScrollDirection.Vertical), // TODO: Fix _layout.ScrollDirection),
			// 		args.IsAnimated);
			// }
			// else
			// {
			base.ScrollToRequested(sender, args);
			// }
		}

		public static void MapIsSwipeEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.Controller.CollectionView.ScrollEnabled = carouselView.IsSwipeEnabled;
		}

		public static void MapIsBounceEnabled(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.Controller.CollectionView.Bounces = carouselView.IsBounceEnabled;
		}

		public static void MapPeekAreaInsets(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			handler.UpdateLayout();
		}

		public static void MapCurrentItem(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController2)?.UpdateFromCurrentItem();
		}

		public static void MapPosition(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			// If the initial position hasn't been set, we have a UpdateInitialPosition call on CarouselViewController2
			// that will handle this so we want to skip this mapper call. We need to wait for the CollectionView to be ready
			if (handler.Controller is CarouselViewController2 CarouselViewController2 && CarouselViewController2.InitialPositionSet)
			{
				CarouselViewController2.UpdateFromPosition();
			}
		}

		public static void MapLoop(CarouselViewHandler2 handler, CarouselView carouselView)
		{
			(handler.Controller as CarouselViewController2)?.UpdateLoop();
		}

	}
}