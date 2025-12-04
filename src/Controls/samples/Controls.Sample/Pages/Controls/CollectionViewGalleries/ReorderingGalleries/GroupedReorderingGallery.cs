using System;
using System.Diagnostics.CodeAnalysis;
using Maui.Controls.Sample.Pages.CollectionViewGalleries.GroupingGalleries;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.ReorderingGalleries
{
	internal class GroupedReorderingGallery : ContentPage
	{
		[RequiresUnreferencedCode("Calls Microsoft.Maui.Controls.Binding.Binding(String, BindingMode, IValueConverter, Object, String, Object)")]
		public GroupedReorderingGallery(IItemsLayout itemsLayout)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var canMixGroupsLabel = new Label { Text = "CanMixGroups: ", VerticalTextAlignment = TextAlignment.Center };
			var canMixGroupsSwitch = new Switch { IsToggled = false };
			var canMixGroupsControl = new StackLayout { Orientation = StackOrientation.Horizontal };
			canMixGroupsControl.Children.Add(canMixGroupsLabel);
			canMixGroupsControl.Children.Add(canMixGroupsSwitch);

			var supportLabel = new Label
			{
				Text = "Reordering of grouped sources is not supported on Windows!",
				FontSize = 24,
				FontAttributes = FontAttributes.Bold,
				IsVisible = (DeviceInfo.Platform == DevicePlatform.WinUI),
				BackgroundColor = Colors.Red
			};

			var reorderCompletedLabel = new Label
			{
				Text = "ReorderCompleted (event): NA",
			};

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = ExampleTemplates.GroupItemTemplate(),
				GroupHeaderTemplate = ExampleTemplates.GroupHeaderTemplate(),
				GroupFooterTemplate = ExampleTemplates.GroupFooterTemplate(),
				AutomationId = "collectionview",
				IsGrouped = true,
				CanReorderItems = true
			};
			collectionView.SetBinding(ReorderableItemsView.CanMixGroupsProperty, new Binding("IsToggled", BindingMode.OneWay, source: canMixGroupsSwitch));
			collectionView.ReorderCompleted += (sender, e) => reorderCompletedLabel.Text = $"ReorderCompleted (event): {DateTime.Now}";

			var reloadButton = new Button { Text = "Reload Current Source", AutomationId = "btnReload", HorizontalOptions = LayoutOptions.Start };
			reloadButton.Clicked += (sender, e) => ReloadItemsSource(collectionView);

			layout.Children.Add(supportLabel);
			layout.Children.Add(canMixGroupsControl);
			layout.Children.Add(reorderCompletedLabel);
			layout.Children.Add(reloadButton);
			layout.Children.Add(collectionView);

			Grid.SetRow(supportLabel, 0);
			Grid.SetRow(canMixGroupsControl, 1);
			Grid.SetRow(reorderCompletedLabel, 2);
			Grid.SetRow(reloadButton, 3);
			Grid.SetRow(collectionView, 4);

			Content = layout;

			collectionView.ItemsSource = new SuperTeams();
		}

		void ReloadItemsSource(CollectionView collectionView)
		{
			var currentSource = collectionView.ItemsSource;
			collectionView.ItemsSource = null;
			collectionView.ItemsSource = currentSource;
		}

	}
}