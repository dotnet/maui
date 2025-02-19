using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeViewGallery : ContentPage
	{
		public SwipeViewGallery()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
	 					GalleryBuilder.NavButton("Basic SwipeView Gallery", () => new BasicSwipeGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeView Threshold Gallery", () => new SwipeThresholdGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeView Events Gallery", () => new SwipeViewEventsGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeItems from Resource Gallery", () => new ResourceSwipeItemsGallery(), Navigation),
						GalleryBuilder.NavButton("BindableLayout Gallery", () => new SwipeBindableLayoutGallery(), Navigation),
						GalleryBuilder.NavButton("ListView (RecycleElement) Gallery", () => new SwipeListViewGallery(), Navigation),
						GalleryBuilder.NavButton("CollectionView Galleries", () => new SwipeCollectionViewGallery(), Navigation),
						GalleryBuilder.NavButton("CollectionView using VisualStates Gallery", () => new SwipeViewVisualStatesCollectionGallery(), Navigation),
						GalleryBuilder.NavButton("CarouselView Gallery", () => new SwipeCarouselViewGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeView GestureRecognizer Gallery", () => new SwipeViewGestureRecognizerGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeBehaviorOnInvoked Gallery", () => new SwipeBehaviorOnInvokedGallery(), Navigation),
						GalleryBuilder.NavButton("Custom SwipeItem Galleries", () => new CustomSwipeItemGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeView BindingContext Gallery", () => new SwipeViewBindingContextGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeItem Icon Gallery", () => new SwipeItemIconGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeItem Position Gallery", () => new PositionGallery(), Navigation),
 						GalleryBuilder.NavButton("SwipeView Margin Gallery", () => new SwipeViewMarginGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeItem Size Gallery", () => new SwipeItemSizeGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeItem IsEnabled Gallery", () => new SwipeItemIsEnabledGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeItemView IsEnabled Gallery", () => new SwipeItemViewIsEnabledGallery(), Navigation),
 						GalleryBuilder.NavButton("SwipeItem IsVisible Gallery", () => new SwipeItemIsVisibleGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeTransitionMode Gallery", () => new SwipeTransitionModeGallery(), Navigation),
						GalleryBuilder.NavButton("Add/Remove SwipeItems Gallery", () => new AddRemoveSwipeItemsGallery(), Navigation),
						GalleryBuilder.NavButton("Open/Close SwipeView Gallery", () => new OpenCloseSwipeGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeItems Dispose Gallery", () => new SwipeItemsDisposeGallery(), Navigation)
					}
				}
			};
		}
	}
}