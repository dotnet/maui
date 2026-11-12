using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26810, "Scroll To first item in CollectionView when updating the collection with KeepItemsInView", PlatformAffected.Android)]
public class Issue26810 : Issue26810Page
{
	public Issue26810() : base(ItemsLayoutOrientation.Horizontal)
	{
	}
}

public abstract class Issue26810Page : ContentPage
{
	protected Issue26810Page(ItemsLayoutOrientation orientation)
	{
		var items = new ObservableCollection<Issue26810ItemModel>();
		for (int i = 1; i <= 30; i++)
		{
			items.Add(new Issue26810ItemModel { Name = $"Preloaded Item {i}", AutomationId = $"Item{i}" });
		}

		var collectionView = new CollectionView
		{
			AutomationId = "26810CollectionView",
			ItemsSource = items,
			ItemsLayout = new LinearItemsLayout(orientation) { ItemSpacing = 5 },
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, nameof(Issue26810ItemModel.Name));
				label.SetBinding(AutomationIdProperty, nameof(Issue26810ItemModel.AutomationId));
				return new Grid { HeightRequest = 50, Children = { label } };
			})
		};

		var modes = new Grid
		{
			ColumnDefinitions = new ColumnDefinitionCollection
			{
				new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)
			}
		};
		AddModeButton(ItemsUpdatingScrollMode.KeepItemsInView, "26810FirstItemButton", 0);
		AddModeButton(ItemsUpdatingScrollMode.KeepScrollOffset, "26810ScrollOffsetButton", 1);
		AddModeButton(ItemsUpdatingScrollMode.KeepLastItemInView, "26810LastItemButton", 2);

		var scroll = new Button { Text = "Scroll to Random Item", AutomationId = "26810Button" };
		scroll.Clicked += (_, _) => collectionView.ScrollTo(19, position: ScrollToPosition.End, animate: true);
		var add = new Button { Text = "Add New Item", AutomationId = "26810AddButton" };
		add.Clicked += (_, _) => items.Add(new Issue26810ItemModel
		{
			Name = $"Item {items.Count + 1}",
			AutomationId = $"Item{items.Count + 1}"
		});

		var grid = new Grid
		{
			AutomationId = "26810MainGrid",
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition(50), new RowDefinition(50), new RowDefinition(50), new RowDefinition(GridLength.Star)
			}
		};
		grid.Add(modes, 0, 0);
		grid.Add(scroll, 0, 1);
		grid.Add(add, 0, 2);
		grid.Add(collectionView, 0, 3);
		Content = grid;

		void AddModeButton(ItemsUpdatingScrollMode mode, string automationId, int column)
		{
			var button = new Button { Text = mode.ToString(), AutomationId = automationId };
			button.Clicked += (_, _) => collectionView.ItemsUpdatingScrollMode = mode;
			modes.Add(button, column, 0);
		}
	}
}

public class Issue26810ItemModel
{
	public string Name { get; set; }
	public string AutomationId { get; set; }
}
