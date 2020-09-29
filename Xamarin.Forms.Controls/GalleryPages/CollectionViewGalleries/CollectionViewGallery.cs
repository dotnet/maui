
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.AlternateLayoutGalleries;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.CarouselViewGalleries;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.GroupingGalleries;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.HeaderFooterGalleries;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.ItemSizeGalleries;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.ScrollModeGalleries;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.SelectionGalleries;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.SpacingGalleries;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	public class CollectionViewGallery : ContentPage
	{
		public CollectionViewGallery()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						GalleryBuilder.NavButton("Default Text Galleries", () => new DefaultTextGallery(), Navigation),
						GalleryBuilder.NavButton("DataTemplate Galleries", () => new DataTemplateGallery(), Navigation),
						GalleryBuilder.NavButton("Observable Collection Galleries", () => new ObservableCollectionGallery(), Navigation),
						GalleryBuilder.NavButton("Snap Points Galleries", () => new SnapPointsGallery(), Navigation),
						GalleryBuilder.NavButton("ScrollTo Galleries", () => new ScrollToGallery(), Navigation),
						GalleryBuilder.NavButton("CarouselView Galleries", () => new CarouselViewGallery(), Navigation),
						GalleryBuilder.NavButton("EmptyView Galleries", () => new EmptyViewGallery(), Navigation),
						GalleryBuilder.NavButton("Selection Galleries", () => new SelectionGallery(), Navigation),
						GalleryBuilder.NavButton("Propagation Galleries", () => new PropagationGallery(), Navigation),
						GalleryBuilder.NavButton("Grouping Galleries", () => new GroupingGallery(), Navigation),
						GalleryBuilder.NavButton("Item Spacing Galleries", () => new ItemsSpacingGallery(), Navigation),
						GalleryBuilder.NavButton("Item Size Galleries", () => new ItemsSizeGallery(), Navigation),
						GalleryBuilder.NavButton("Scroll Mode Galleries", () => new ScrollModeGallery(), Navigation),
						GalleryBuilder.NavButton("Alternate Layout Galleries", () => new AlternateLayoutGallery(), Navigation),
						GalleryBuilder.NavButton("Header/Footer Galleries", () => new HeaderFooterGallery(), Navigation),
						GalleryBuilder.NavButton("Nested CollectionViews", () => new NestedGalleries.NestedCollectionViewGallery(), Navigation),
					}
				}
			};
		}
	}
}
