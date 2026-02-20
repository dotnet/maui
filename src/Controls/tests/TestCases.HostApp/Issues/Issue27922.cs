using Maui.Controls.Sample.Issues;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27922, "[WinUI]CollectionView with GroupHeader enabled should scroll properly with groupheader",
		PlatformAffected.UWP)]
	public partial class Issue27922 : TestContentPage
	{
		private CollectionView _collectionView;
		private ObservableCollection<Issue27922GroupedItems> _groupedItemsSource;

		protected override void Init()
		{
			Title = "Grouped CollectionView";

			// Initialize CollectionView
			_collectionView = new CollectionView
			{
				IsGrouped = true,
				AutomationId = "27922GroupedCollection",
				ItemTemplate = new DataTemplate(() =>
				{
					Label label = new Label { Padding = 10, HeightRequest = 80 };
					label.FontAttributes = FontAttributes.Bold;
					label.SetBinding(Label.TextProperty, "Name");
					return label;
				}),
				GroupHeaderTemplate = new DataTemplate(() =>
				{
					Label headerLabel = new Label
					{
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						Padding = 10,
						VerticalTextAlignment = TextAlignment.Center,
						BackgroundColor = Colors.LightGray
					};
					headerLabel.SetBinding(Label.TextProperty, "Category");
					return headerLabel;
				})
			};

			// Create grouped data
			_groupedItemsSource = new ObservableCollection<Issue27922GroupedItems>
				{
				new Issue27922GroupedItems("Fruits", new List<Issue27922Item>
				{
				new Issue27922Item { Name = "Apple" },
				new Issue27922Item { Name = "Banana" },
				new Issue27922Item { Name = "Orange" }
				}),
				new Issue27922GroupedItems("Vegetables", new List<Issue27922Item>
				{
				new Issue27922Item { Name = "Carrot" },
				new Issue27922Item { Name = "Broccoli" },
				new Issue27922Item { Name = "Spinach" }
				}),
				new Issue27922GroupedItems("Desserts", new List<Issue27922Item>
				{
				new Issue27922Item { Name = "Cake" },
				new Issue27922Item { Name = "IceCream" },
				new Issue27922Item { Name = "GulabJamun" }
				}),
				new Issue27922GroupedItems("Grains", new List<Issue27922Item>
				{
				new Issue27922Item { Name = "Rice" },
				new Issue27922Item { Name = "Wheat" },
				new Issue27922Item { Name = "Oats" }
				}),
				new Issue27922GroupedItems("Beverages", new List<Issue27922Item>
				{
				new Issue27922Item { Name = "Coffee" },
				new Issue27922Item { Name = "Tea" },
				new Issue27922Item { Name = "Juice" }
				}),
				new Issue27922GroupedItems("Dairy", new List<Issue27922Item>
				{
				new Issue27922Item { Name = "Milk" },
				new Issue27922Item { Name = "Cheese" },
				new Issue27922Item { Name = "Yogurt" }
				}),
				new Issue27922GroupedItems("Meat", new List<Issue27922Item>
				{
				new Issue27922Item { Name = "Chicken" },
				new Issue27922Item { Name = "Beef" },
				new Issue27922Item { Name = "Fish" }
				}),
				new Issue27922GroupedItems("Snacks", new List<Issue27922Item>
				{
				new Issue27922Item { Name = "Chips" },
				new Issue27922Item { Name = "Cookies" },
				new Issue27922Item { Name = "Nuts" }
				})
				};

			_collectionView.ItemsSource = _groupedItemsSource;
			Grid.SetRow(_collectionView, 1);

			// Scroll Button
			Button scrollToButton = new Button
			{
				Text = "Scroll to Coffee",
				AutomationId = "27922ScrollToButton",
			};
			scrollToButton.Clicked += OnScrollButtonClicked;
			Grid.SetRow(scrollToButton, 0);
			// Layout
			var grid = new Grid
			{
				RowDefinitions =
				{
				 new RowDefinition { Height = new GridLength(50) } ,   // Auto-sized row
				 new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Expands to fill remaining space
				}
			};
			grid.Children.Add(scrollToButton);
			grid.Children.Add(_collectionView);
			Content = grid;
		}

		private void OnScrollButtonClicked(object sender, EventArgs e)
		{
			var targetGroup = _groupedItemsSource.FirstOrDefault(g => g.Category == "Beverages");
			var targetItem = targetGroup?.FirstOrDefault(i => i.Name == "Coffee");

			if (targetGroup != null && targetItem != null)
			{
				_collectionView.ScrollTo(targetItem, targetGroup, ScrollToPosition.Start, true);
			}
		}
	}

	public class Issue27922GroupedItems : ObservableCollection<Issue27922Item>
	{
		public string Category { get; }

		public Issue27922GroupedItems(string category, IEnumerable<Issue27922Item> items) : base(items)
		{
			Category = category;
		}
	}

	public class Issue27922Item
	{
		public string Name { get; set; }
	}
}
