
using Maui.Controls.Sample.Pages.CollectionViewGalleries.AlternateLayoutGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.CarouselViewGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.EmptyViewGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.GroupingGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.HeaderFooterGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.ItemSizeGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.ReorderingGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.ScrollModeGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.SelectionGalleries;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.SpacingGalleries;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{

	public class CollectionViewGalleryNavigation : NavigationPage
	{
		public CollectionViewGalleryNavigation()
		{
			PushAsync(new TemplateCodeCollectionViewGallery(LinearItemsLayout.Vertical));
		}

	}
	public class CollectionViewGallery : ContentPage
	{
		public CollectionViewGallery()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Spacing = 5,
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
						GalleryBuilder.NavButton("Reordering Galleries", () => new ReorderingGallery(), Navigation),
						GalleryBuilder.NavButton("Item Spacing Galleries", () => new ItemsSpacingGallery(), Navigation),
						GalleryBuilder.NavButton("Item Size Galleries", () => new ItemsSizeGallery(), Navigation),
						GalleryBuilder.NavButton("Scroll Mode Galleries", () => new ScrollModeGallery(), Navigation),
						GalleryBuilder.NavButton("Alternate Layout Galleries", () => new AlternateLayoutGallery(), Navigation),
						GalleryBuilder.NavButton("Header/Footer Galleries", () => new HeaderFooterGallery(), Navigation),
						GalleryBuilder.NavButton("Nested CollectionViews", () => new NestedGalleries.NestedCollectionViewGallery(), Navigation),
						GalleryBuilder.NavButton("Online images", () => new OnlineImages(), Navigation),
						GalleryBuilder.NavButton("Adaptive CollectionView Gallery", () => new AdaptiveCollectionView(), Navigation),
					}
				}
			};
		}
	}
}
