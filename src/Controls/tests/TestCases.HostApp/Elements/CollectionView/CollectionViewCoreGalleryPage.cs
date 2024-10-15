using Controls.Sample.UITests;
using Maui.Controls.Sample.CollectionViewGalleries.ItemSizeGalleries;
using Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries;

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