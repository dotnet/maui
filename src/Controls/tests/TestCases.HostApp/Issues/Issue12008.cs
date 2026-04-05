using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12008, "CollectionView Drag and Drop Reordering Can't Drop in Empty Group", PlatformAffected.iOS)]
public class Issue12008 : ContentPage
{
	public ObservableCollection<Issue12008Group> Groups { get; }

	public Issue12008()
	{
		Title = "Issue 12008 - Drag into Empty Group";

		// Create grouped data with one empty group
		Groups = new ObservableCollection<Issue12008Group>
		{
			new Issue12008Group("Group A", new ObservableCollection<Item>
			{
				new Item("Item A1"),
				new Item("Item A2"),
				new Item("Item A3")
			}),
			new Issue12008Group("Group B", new ObservableCollection<Item>
			{
				new Item("Item B1"),
				new Item("Item B2")
			}),
			new Issue12008Group("Empty Group", new ObservableCollection<Item>()), // Empty group
			new Issue12008Group("Group C", new ObservableCollection<Item>
			{
				new Item("Item C1"),
			})
		};

		var collectionView = new CollectionView
		{
			AutomationId = "ReorderCollectionView",
			ItemsSource = Groups,
			IsGrouped = true,
			CanReorderItems = true,
			CanMixGroups = true,
			SelectionMode = SelectionMode.None
		};

		// Set GroupHeaderTemplate to display group name and item count
		collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			var groupNameLabel = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 18,
				TextColor = Colors.Black
			};
			groupNameLabel.SetBinding(Label.TextProperty, new Binding("Name"));

			var countLabel = new Label
			{
				FontSize = 16,
				TextColor = Colors.Gray,
				HorizontalTextAlignment = TextAlignment.End
			};
			countLabel.SetBinding(Label.TextProperty, new Binding("Count", stringFormat: "({0} items)"));

			var headerGrid = new Grid
			{
				Padding = 12,
				BackgroundColor = Color.FromArgb("#F0F0F0"),
				ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
				Children =
				{
					groupNameLabel,
					countLabel
				}
			};
			headerGrid.SetColumn(countLabel, 1);

			// Set AutomationId based on group name
			headerGrid.SetBinding(AutomationProperties.NameProperty, new Binding("Name"));

			return headerGrid;
		});

		// Set ItemTemplate
		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var itemLabel = new Label
			{
				FontSize = 16,
				Padding = 16
			};
			itemLabel.SetBinding(Label.TextProperty, "Name");

			var itemContainer = new Border
			{
				Padding = 0,
				Margin = new Thickness(12, 4, 12, 4),
				Content = itemLabel,
				BackgroundColor = Colors.White,
				StrokeShape = new RoundRectangle { CornerRadius = 5 },
				Shadow = new Shadow { Opacity = 0.3f, Radius = 2 }
			};

			// Set AutomationId based on item name for testability
			itemContainer.SetBinding(AutomationProperties.NameProperty, new Binding("Name"));

			return itemContainer;
		});

		// Add ReorderCompleted event handler to update status
		collectionView.ReorderCompleted += OnReorderCompleted;

		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Ready to reorder items",
			FontSize = 14,
			Padding = 12,
			BackgroundColor = Color.FromArgb("#E8F5E9")
		};

		Content = new VerticalStackLayout
		{
			Spacing = 8,
			Padding = 8,
			Children =
			{
				statusLabel,
				collectionView
			}
		};

		BindingContext = this;
	}

	void OnReorderCompleted(object sender, EventArgs e)
	{
		// Update status label with per-group counts so tests can verify actual data model changes
		if (Content is VerticalStackLayout layout && layout.Children[0] is Label statusLabel)
		{
			var groupCounts = string.Join(", ", Groups.Select(g => $"{g.Name}:{g.Count}"));
			statusLabel.Text = $"Reorder completed! {groupCounts}";
			statusLabel.BackgroundColor = Color.FromArgb("#C8E6C9");
		}
	}

	public class Issue12008Group : ObservableCollection<Item>
	{
		public string Name { get; set; }

		public Issue12008Group(string name, ObservableCollection<Item> items)
		{
			Name = name;

			foreach (var item in items)
				Add(item);
		}
	}

	public class Item
	{
		public string Name { get; set; }

		public Item(string name)
		{
			Name = name;
		}
	}
}
