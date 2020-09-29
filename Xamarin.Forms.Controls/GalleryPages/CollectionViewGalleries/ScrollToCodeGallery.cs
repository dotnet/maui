using System;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ScrollToCodeGallery : ContentPage
	{
		public ScrollToCodeGallery(IItemsLayout itemsLayout, ScrollToMode mode = ScrollToMode.Position, Func<DataTemplate> dataTemplate = null, Func<CollectionView> createCollectionView = null)
		{
			createCollectionView = createCollectionView ?? (() => new CollectionView());

			Title = $"ScrollTo (Code, {itemsLayout})";

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var itemTemplate = dataTemplate == null ? ExampleTemplates.ScrollToIndexTemplate() : dataTemplate();

			var collectionView = createCollectionView();

			collectionView.ItemsLayout = itemsLayout;
			collectionView.ItemTemplate = itemTemplate;

			var generator = new ItemsSourceGenerator(collectionView, initialItems: 50);
			layout.Children.Add(generator);

			if (mode == ScrollToMode.Position)
			{
				var scrollToControl = new ScrollToIndexControl(collectionView);
				layout.Children.Add(scrollToControl);
				Grid.SetRow(scrollToControl, 1);
			}
			else
			{
				var scrollToControl = new ScrollToItemControl(collectionView);
				layout.Children.Add(scrollToControl);
				Grid.SetRow(scrollToControl, 1);
			}

			layout.Children.Add(collectionView);

			Grid.SetRow(collectionView, 2);

			Content = layout;

			generator.GenerateItems();
		}
	}
}