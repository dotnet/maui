using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class TemplateCodeCollectionViewGallery : ContentPage
	{
		public TemplateCodeCollectionViewGallery() : this(LinearItemsLayout.Vertical)
		{

		}
		public TemplateCodeCollectionViewGallery(IItemsLayout itemsLayout)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var itemTemplate = ExampleTemplates.PhotoTemplate();

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				AutomationId = "collectionview",
				BackgroundColor = Colors.Red,

			};

			var generator = new ItemsSourceGenerator(collectionView, initialItems: 2);

			layout.Children.Add(generator);
			layout.Children.Add(collectionView);

			Grid.SetRow(collectionView, 1);

			Content = layout;

			generator.GenerateItems();
		}
	}
}