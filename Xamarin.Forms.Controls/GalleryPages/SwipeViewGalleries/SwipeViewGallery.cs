using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeViewGallery : ContentPage
	{
		public SwipeViewGallery()
		{
			var button = new Button
			{
				Text = "Enable SwipeView",
				AutomationId = "EnableSwipeView"
			};
			button.Clicked += ButtonClicked;

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
	 					button,
						GalleryBuilder.NavButton("Basic SwipeView Gallery", () => new BasicSwipeGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeView Events Gallery", () => new SwipeViewEventsGallery(), Navigation),   
	  					GalleryBuilder.NavButton("SwipeItems from Resource Gallery", () => new ResourceSwipeItemsGallery(), Navigation),
						GalleryBuilder.NavButton("BindableLayout Gallery", () => new SwipeBindableLayoutGallery(), Navigation),
						GalleryBuilder.NavButton("ListView (RecycleElement) Gallery", () => new SwipeListViewGallery(), Navigation),
						GalleryBuilder.NavButton("CollectionView Galleries", () => new SwipeCollectionViewGallery(), Navigation),
						GalleryBuilder.NavButton("CarouselView Gallery", () => new SwipeCarouselViewGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeBehaviorOnInvoked Gallery", () => new SwipeBehaviorOnInvokedGallery(), Navigation),
						GalleryBuilder.NavButton("Custom SwipeItem Galleries", () => new CustomSwipeItemGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeItem Icon Gallery", () => new SwipeItemIconGallery(), Navigation),
						GalleryBuilder.NavButton("SwipeTransitionMode Gallery", () => new SwipeTransitionModeGallery(), Navigation),
						GalleryBuilder.NavButton("Add/Remove SwipeItems Gallery", () => new AddRemoveSwipeItemsGallery(), Navigation),
						GalleryBuilder.NavButton("Close SwipeView Gallery", () => new CloseSwipeGallery(), Navigation)
					}
				}
			};
		}

		void ButtonClicked(object sender, System.EventArgs e)
		{
			var button = sender as Button;

			button.Text = "SwipeView Enabled!";
			button.TextColor = Color.Black;
			button.IsEnabled = false;

			Device.SetFlags(new[] { ExperimentalFlags.SwipeViewExperimental });
		}
	}
}