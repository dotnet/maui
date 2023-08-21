//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.ItemSizeGalleries
{
	internal class DynamicItemSizeGallery : ContentPage
	{
		public DynamicItemSizeGallery(IItemsLayout itemsLayout)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var instructions = new Label
			{
				Text = "Tap the buttons in each item to increase/decrease the amount of text. The items should expand and contract to accommodate the text."
			};

			var itemTemplate = ExampleTemplates.DynamicTextTemplate();

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				AutomationId = "collectionview"
			};

			var generator = new ItemsSourceGenerator(collectionView, initialItems: 20);

			layout.Children.Add(generator);
			layout.Children.Add(instructions);
			layout.Children.Add(collectionView);

			Grid.SetRow(instructions, 1);
			Grid.SetRow(collectionView, 2);

			Content = layout;

			generator.GenerateItems();
		}
	}
}
