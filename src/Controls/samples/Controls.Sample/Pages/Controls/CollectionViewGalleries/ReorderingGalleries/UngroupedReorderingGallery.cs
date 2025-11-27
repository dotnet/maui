using System;
using System.Collections.Specialized;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.SpacingGalleries;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.ReorderingGalleries
{
	internal class UngroupedReorderingGallery : ContentPage
	{
		public UngroupedReorderingGallery(IItemsLayout itemsLayout)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var reorderCompletedLabel = new Label
			{
				Text = "ReorderCompleted (event): NA",
			};

			var itemTemplate = ExampleTemplates.SpacingTemplate();

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				AutomationId = "collectionview",
				CanReorderItems = true
			};
			collectionView.ReorderCompleted += (sender, e) => reorderCompletedLabel.Text = $"ReorderCompleted (event): {DateTime.Now}";

			var generator = new ItemsSourceGenerator(collectionView, initialItems: 20, itemsSourceType: ItemsSourceType.ObservableCollection);

			var itemsSourceTypeSelector = new EnumSelector<ItemsSourceType>(() => generator.ItemsSourceType, sourceType => UpdateItemsSourceType(generator, sourceType, collectionView));

			var spacingModifier = new SpacingModifier(collectionView.ItemsLayout, "Update_Spacing");

			var reloadButton = new Button { Text = "Reload Current Source", AutomationId = "btnReload", HorizontalOptions = LayoutOptions.Start };
			reloadButton.Clicked += (sender, e) => ReloadItemsSource(collectionView);

			layout.Children.Add(generator);
			layout.Children.Add(itemsSourceTypeSelector);
			layout.Children.Add(spacingModifier);
			layout.Children.Add(reorderCompletedLabel);
			layout.Children.Add(reloadButton);
			layout.Children.Add(collectionView);

			Grid.SetRow(itemsSourceTypeSelector, 1);
			Grid.SetRow(spacingModifier, 2);
			Grid.SetRow(reorderCompletedLabel, 3);
			Grid.SetRow(reloadButton, 4);
			Grid.SetRow(collectionView, 5);

			Content = layout;

			generator.GenerateItems();
		}

		void ReloadItemsSource(CollectionView collectionView)
		{
			var currentSource = collectionView.ItemsSource;
			collectionView.ItemsSource = null;
			collectionView.ItemsSource = currentSource;
		}

		async void UpdateItemsSourceType(ItemsSourceGenerator generator, ItemsSourceType itemsSourceType, CollectionView collectionView)
		{
			generator.GenerateItems(itemsSourceType);

			if (DeviceInfo.Platform == DevicePlatform.WinUI && !(collectionView.ItemsSource is INotifyCollectionChanged))
			{
				await DisplayAlertAsync("Warning!", "Reordering on UWP/WinUI only works with ObservableCollections!", "OK");
			}
		}
	}
}