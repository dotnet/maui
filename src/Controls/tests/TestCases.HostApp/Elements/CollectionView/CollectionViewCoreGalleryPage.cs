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
						// VisitAndUpdateItemsSource (src\Compatibility\ControlGallery\src\UITests.Shared\Tests\CollectionViewUITests.cs)
						TestBuilder.NavButton("Default Text Galleries", () => new DefaultTextGallery(), Navigation),
						TestBuilder.NavButton("DataTemplate Galleries", () => new DataTemplateGallery(), Navigation),
						TestBuilder.NavButton("Grouping Galleries", () => new GroupingGallery(), Navigation),
						TestBuilder.NavButton("Header Footer Galleries", () => new HeaderFooterGallery(), Navigation),
						TestBuilder.NavButton("Observable Collection Galleries", () => new ObservableCollectionGallery(), Navigation),
						// SelectionShouldUpdateBinding (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewBoundSingleSelection.cs)
						// ItemsFromViewModelShouldBeSelected (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewBoundMultiSelection.cs)
						TestBuilder.NavButton("Selection Galleries", () => new SelectionGallery(), Navigation),
						TestBuilder.NavButton("Item Size Galleries", () => new ItemsSizeGallery(), Navigation),
					}
					}
				};
			}
		}
	}
}