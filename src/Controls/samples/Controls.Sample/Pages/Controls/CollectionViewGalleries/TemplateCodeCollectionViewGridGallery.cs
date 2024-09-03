using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
{
	internal class TemplateCodeCollectionViewGridGallery : ContentPage
	{

		protected override async void OnNavigatedFrom(NavigatedFromEventArgs args)
		{
			base.OnNavigatedFrom(args);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			await Task.Yield();

			var result = GC.GetTotalMemory(forceFullCollection: true);
			System.Diagnostics.Debug.WriteLine($"Memory: {result}");
		}
		
		public TemplateCodeCollectionViewGridGallery(ItemsLayoutOrientation orientation = ItemsLayoutOrientation.Vertical)
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

			var itemsLayout = new GridItemsLayout(2, orientation);

			var itemTemplate = ExampleTemplates.PhotoTemplate();

			var collectionView = new CollectionView { ItemsLayout = itemsLayout, ItemTemplate = itemTemplate, AutomationId = "collectionview" };

			var generator = new ItemsSourceGenerator(collectionView, 100);
			var spanSetter = new SpanSetter(collectionView);

			layout.Children.Add(generator);
			layout.Children.Add(spanSetter);
			Grid.SetRow(spanSetter, 1);
			layout.Children.Add(collectionView);
			Grid.SetRow(collectionView, 2);

			Content = layout;

			spanSetter.UpdateSpan();
			generator.GenerateItems();
		}
	}
}