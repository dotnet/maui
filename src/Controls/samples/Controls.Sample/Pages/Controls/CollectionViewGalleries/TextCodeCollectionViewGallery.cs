using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
	internal class TextCodeCollectionViewGallery : ContentPage
	{
		public TextCodeCollectionViewGallery(IItemsLayout itemsLayout)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				SelectionMode = SelectionMode.Single,
				AutomationId = "collectionview"
			};

			var generator = new ItemsSourceGenerator(collectionView, 200000);

			layout.Children.Add(generator);

			Grid.SetRow(collectionView, 1);
			layout.Children.Add(collectionView);

			Content = layout;

			generator.GenerateItems();
		}
	}
}