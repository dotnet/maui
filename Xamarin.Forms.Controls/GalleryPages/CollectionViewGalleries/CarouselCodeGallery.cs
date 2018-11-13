namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class CarouselCodeGallery : ContentPage
	{
		public CarouselCodeGallery(ItemsLayoutOrientation orientation)
		{
			Title = $"CarouselView (Code, {orientation})";

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var itemsLayout =
				new ListItemsLayout(orientation)
				{
					SnapPointsType = SnapPointsType.MandatorySingle,
					SnapPointsAlignment = SnapPointsAlignment.Center
				};

			var itemTemplate = ExampleTemplates.CarouselTemplate();

			var carouselView = new CarouselView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
			};

			var generator = new ItemsSourceGenerator(carouselView, initialItems: 50);

			layout.Children.Add(generator);
		
			var scrollToControl = new ScrollToIndexControl(carouselView, false);
			layout.Children.Add(scrollToControl);

			layout.Children.Add(carouselView);
			
			Grid.SetRow(scrollToControl, 1);
			Grid.SetRow(carouselView, 2);

			Content = layout;

			generator.GenerateItems();
		}
	}
}