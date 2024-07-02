using Controls.Sample.UITests;
using Maui.Controls.Sample.CollectionViewGalleries.AlternateLayoutGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.EmptyViewGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.HeaderFooterGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.ItemSizeGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.ReorderingGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.ScrollModeGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.SpacingGalleries;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.CollectionViewGalleries
{

	public class CollectionViewGalleryNavigation : NavigationPage
	{
		public CollectionViewGalleryNavigation()
		{
			PushAsync(new TemplateCodeCollectionViewGallery(LinearItemsLayout.Vertical));
		}

	}

	public class CollectionViewCoreGalleryPage : NavigationPage
	{
		public CollectionViewCoreGalleryPage() : base(new CollectionViewCoreGalleryContentPage())
		{

		}

		public class CollectionViewCoreGalleryContentPage : ContentPage
		{
			public CollectionViewCoreGalleryContentPage()
			{
				Content = new ScrollView
				{
					Content = new StackLayout
					{
						Spacing = 5,
						Children =
						{
							TestBuilder.NavButton("Default Text Galleries", () => new DefaultTextGallery(), Navigation),
							TestBuilder.NavButton("DataTemplate Galleries", () => new DataTemplateGallery(), Navigation),
							TestBuilder.NavButton("Observable Collection Galleries", () => new ObservableCollectionGallery(), Navigation),
							TestBuilder.NavButton("Snap Points Galleries", () => new SnapPointsGallery(), Navigation),
							TestBuilder.NavButton("ScrollTo Galleries", () => new ScrollToGallery(), Navigation),
							TestBuilder.NavButton("EmptyView Galleries", () => new EmptyViewGallery(), Navigation),
							TestBuilder.NavButton("Selection Galleries", () => new SelectionGallery(), Navigation),
							TestBuilder.NavButton("Propagation Galleries", () => new PropagationGallery(), Navigation),
							TestBuilder.NavButton("Grouping Galleries", () => new GroupingGallery(), Navigation),
							TestBuilder.NavButton("Reordering Galleries", () => new ReorderingGallery(), Navigation),
							TestBuilder.NavButton("Item Spacing Galleries", () => new ItemsSpacingGallery(), Navigation),
							TestBuilder.NavButton("Item Size Galleries", () => new ItemsSizeGallery(), Navigation),
							TestBuilder.NavButton("Scroll Mode Galleries", () => new ScrollModeGallery(), Navigation),
							TestBuilder.NavButton("Alternate Layout Galleries", () => new AlternateLayoutGallery(), Navigation),
							TestBuilder.NavButton("Header/Footer Galleries", () => new HeaderFooterGallery(), Navigation),
							TestBuilder.NavButton("Nested CollectionViews", () => new NestedGalleries.NestedCollectionViewGallery(), Navigation),
							TestBuilder.NavButton("Online images", () => new OnlineImages(), Navigation),
							TestBuilder.NavButton("Adaptive CollectionView Gallery", () => new AdaptiveCollectionView(), Navigation),

						}
					}
				};
			}
		}
	}
}